using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using StoreListings.Library.Internal;

namespace StoreListings.Library;

/// <summary>
/// Represents a product from the StoreEdgeFD API.
/// </summary>
public class StoreEdgeFDProduct
{
    public required string ProductId { get; set; }

    public required string Title { get; set; }

    public required string ShortDescription { get; set; }

    public required string Description { get; set; }

    public required string PublisherName { get; set; }

    public required List<Image> Screenshots { get; set; }

    public required Image Logo { get; set; }

    public required string RevisionId { get; set; }

    public required double Rating { get; set; }

    public required long RatingCount { get; set; }

    public required long Size { get; set; }

    public required bool IsBundle { get; set; }

    public required InstallerType InstallerType { get; set; }

    public string? PackageFamilyName { get; set; }

    [SetsRequiredMembers]
    private StoreEdgeFDProduct(
        string productId,
        string title,
        Image logo,
        List<Image> screenshots,
        string shortDescription,
        string description,
        string publisherName,
        string revisionId,
        double rating,
        long ratingCount,
        long size,
        InstallerType installerType,
        bool isBundle,
        string? packageFamilyName
    )
    {
        ProductId = productId;
        Title = title;
        Logo = logo;
        Screenshots = screenshots;
        ShortDescription = shortDescription;
        Description = description;
        PublisherName = publisherName;
        RevisionId = revisionId;
        Rating = rating;
        RatingCount = ratingCount;
        Size = size;
        InstallerType = installerType;
        IsBundle = isBundle;
        PackageFamilyName = packageFamilyName;
    }

    public static async Task<Result<StoreEdgeFDProduct>> GetProductAsync(
        string productId,
        DeviceFamily deviceFamily,
        Market market,
        Lang language,
        CancellationToken cancellationToken = default
    )
    {
        HttpClient client = Helpers.GetStoreHttpClient();

        try
        {
            string url =
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/products/{productId}?market={market}&locale={language}-{market}&deviceFamily=Windows.{deviceFamily}&architecture=x64&deviceFamilyVersion=281475124959641";

            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

                    return Result<StoreEdgeFDProduct>.Failure(
                        new Exception($"API Error {response.StatusCode}: {errorContent}")
                    );
                }
                catch
                {
                    response.EnsureSuccessStatusCode();
                }
            }

            using JsonDocument jsondoc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken
            );

            JsonElement payload = jsondoc.RootElement.GetPropertySafe("Payload");

            string id = payload.GetStringSafe("ProductId") ?? productId;
            string title = payload.GetStringSafe("Title") ?? string.Empty;
            string publisher = payload.GetStringSafe("PublisherName") ?? string.Empty;
            string revisionId = payload.GetStringSafe("RevisionId") ?? string.Empty;
            double rating = payload.GetDoubleSafe("AverageRating");
            long ratingCount = payload.GetLongSafe("RatingCount");
            long size = payload.GetLongSafe("ApproximateSizeInBytes");
            var (logo, screenshots) = ExtractImages(payload);
            var (shortDesc, fullDesc) = Helpers.ProcessDescriptions(payload);
            InstallerType installerType = DetermineInstallerType(
                payload.GetPropertySafe("Installer").GetStringSafe("Type")
            );
            JsonElement? firstSku = payload.GetFirstArrayElementOrNull("Skus");
            bool isBundle =
                firstSku?.GetPropertySafe("BundledSkus").ValueKind == JsonValueKind.Array;

            string? version = ExtractVersion(payload);

            string? pfn = payload.GetFirstArrayElementOrNull("PackageFamilyNames")?.GetString();

            return Result<StoreEdgeFDProduct>.Success(
                new(
                    id,
                    title,
                    logo,
                    screenshots,
                    shortDesc,
                    fullDesc,
                    publisher,
                    revisionId,
                    rating,
                    ratingCount,
                    size,
                    installerType,
                    isBundle,
                    pfn
                )
            );
        }
        catch (Exception ex)
        {
            return Result<StoreEdgeFDProduct>.Failure(ex);
        }
    }

    // --------------

    // HELPERS

    // --------------

    private static (Image Logo, List<Image> Screenshots) ExtractImages(JsonElement payload)
    {
        var logos = new List<Image>();

        var screenshots = new List<Image>();

        if (payload.TryGetProperty("Images", out JsonElement images))
        {
            foreach (JsonElement img in images.EnumerateArray())
            {
                string? url = img.GetStringSafe("Url");

                if (string.IsNullOrEmpty(url))
                    continue;

                string type = img.GetStringSafe("ImageType") ?? "";

                string bg = img.GetStringSafe("BackgroundColor") ?? "Transparent";

                if (!bg.StartsWith('#'))
                    bg = "Transparent";

                int h = img.GetIntSafe("Height");

                int w = img.GetIntSafe("Width");

                var imageObj = new Image(url, bg, h, w);

                if (
                    type.Equals("logo", StringComparison.OrdinalIgnoreCase)
                    || type.Equals("Poster", StringComparison.OrdinalIgnoreCase)
                    || type.Equals("BoxArt", StringComparison.OrdinalIgnoreCase)
                )
                {
                    logos.Add(imageObj);
                }
                else if (type.Equals("screenshot", StringComparison.OrdinalIgnoreCase))
                {
                    screenshots.Add(imageObj);
                }
            }
        }

        Image finalLogo =
            logos.LastOrDefault(img => img.Height == 100 && img.Width == 100)
            ?? logos.FirstOrDefault()
            ?? new Image(string.Empty, "Transparent", 0, 0);

        return (finalLogo, screenshots);
    }

    private static InstallerType DetermineInstallerType(string? type) =>
        type switch
        {
            "WindowsUpdate" => InstallerType.Packaged,

            "WPM" or "DirectInstall" => InstallerType.Unpackaged,

            _ => InstallerType.Unknown,
        };

    private static string? ExtractVersion(JsonElement payload)
    {
        var architectures = payload.GetPropertySafe("Installer").GetPropertySafe("Architectures");

        if (architectures.ValueKind != JsonValueKind.Object)
            return null;

        return architectures.GetPropertySafe("x64").GetStringSafe("Version") ?? architectures
                .GetPropertySafe("x86")
                .GetStringSafe("Version");
    }

    public static async Task<
        Result<
            List<(
                string InstallerUrl,
                string FileName,
                string InstallerSwitches,
                string Version,
                string InstallerSha256,
                string architecture
            )>
        >
    > GetUnpackagedInstall(
        string productId,
        Market market,
        Lang language,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            HttpClient client = Helpers.GetStoreHttpClient();

            string url =
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/packageManifests/{productId}";

            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using JsonDocument json = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(),
                cancellationToken: cancellationToken
            );

            JsonElement data = json.RootElement.GetPropertySafe("Data");

            var allInstallers = new List<(string, string, string, string, string, string)>();

            foreach (JsonElement versionObj in data.GetArraySafe("Versions").EnumerateArray())
            {
                string rawVersion = versionObj.GetStringSafe("PackageVersion");

                string version = ExtractNumericVersionPrefix(rawVersion);

                string packageName = versionObj
                    .GetPropertySafe("DefaultLocale")
                    .GetStringSafe("PackageName");

                foreach (
                    JsonElement installer in versionObj.GetArraySafe("Installers").EnumerateArray()
                )
                {
                    string installerUrl = installer.GetStringSafe("InstallerUrl");
                    string installerSha256 = installer.GetStringSafe("InstallerSha256");

                    string installerSwitches = installer
                        .GetPropertySafe("InstallerSwitches")
                        .GetStringSafe("Silent");

                    string extension = installer.GetStringSafe("InstallerType").ToLowerInvariant();
                    if (string.IsNullOrEmpty(extension))
                    {
                        extension = "exe";
                    }

                    string architecture = installer.GetStringSafe("Architecture");
                    if (string.IsNullOrEmpty(architecture))
                    {
                        architecture = "unknown";
                    }
                    string fileName = $"{packageName}_{architecture}.{extension}";

                    allInstallers.Add(
                        (
                            installerUrl,
                            fileName,
                            installerSwitches,
                            version,
                            installerSha256,
                            architecture
                        )
                    );
                }
            }

            return Result<List<(string, string, string, string, string, string)>>.Success(
                allInstallers
            );
        }
        catch (Exception ex)
        {
            return Result<List<(string, string, string, string, string, string)>>.Failure(ex);
        }
    }

    private static string ExtractNumericVersionPrefix(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return string.Empty;

        ReadOnlySpan<char> span = s.AsSpan().Trim();
        int space = span.IndexOf(' ');
        return space > 0 ? span[..space].ToString() : span.ToString();
    }
}
