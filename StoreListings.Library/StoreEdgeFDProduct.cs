using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using StoreListings.Library.Internal;

namespace StoreListings.Library;

/// <summary>
/// Represents a product from the StoreEdgeFD API.
/// </summary>
public class StoreEdgeFDProduct
{
    /// <summary>
    /// The Store product ID.
    /// </summary>
    public required string ProductId { get; set; }

    /// <summary>
    /// The listing title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The short listing description, if available.
    /// </summary>
    public string? ShortDescription { get; set; }

    /// <summary>
    /// The full listing description, if available.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The publisher name.
    /// </summary>
    public required string PublisherName { get; set; }

    /// <summary>
    /// The list of screenshots.
    /// </summary>
    public required List<Image> Screenshots { get; set; }

    /// <summary>
    /// The logo image.
    /// </summary>
    public required Image Logo { get; set; }

    /// <summary>
    /// last updated date.
    /// </summary>
    public required string? RevisionId { get; set; }

    /// <summary>
    /// Store version (Packaged apps: best-effort, Unpackaged apps: from package manifest).
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// The product rating.
    /// </summary>
    public required double? Rating { get; set; }

    /// <summary>
    /// The number of ratings.
    /// </summary>
    public required long? RatingCount { get; set; }

    /// <summary>
    /// The size of the product.
    /// </summary>
    public required long? Size { get; set; }

    /// <summary>
    /// Indicates if the product is a bundle.
    /// </summary>
    public required bool IsBundle { get; set; }

    /// <summary>
    /// The installer type.
    /// </summary>
    public required InstallerType InstallerType { get; set; }

    /// <summary>
    /// The package family name, when available.
    /// </summary>
    public string? PackageFamilyName { get; set; }

    [SetsRequiredMembers]
    private StoreEdgeFDProduct(
        string productId,
        string title,
        Image logo,
        List<Image> screenshots,
        string? shortDescription,
        string? description,
        string publisherName,
        string? revisionId,
        double? rating,
        long? ratingCount,
        long? size,
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
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/products/{productId}?market={market}&locale={language}-{market}&deviceFamily=Windows.{deviceFamily}";
            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            JsonDocument? json = null;
            try
            {
                json = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(),
                    cancellationToken: cancellationToken
                );
            }
            catch
            {
                // Definitely an error.
                response.EnsureSuccessStatusCode();
            }

            // We don't know yet if the response is an error, but we do know the response is in JSON.
            using JsonDocument jsondoc = json!;

            if (!response.IsSuccessStatusCode)
            {
                // Error.
                return Result<StoreEdgeFDProduct>.Failure(
                    new Exception(jsondoc.RootElement.GetProperty("message").GetString())
                );
            }

            JsonElement payloadElement = jsondoc.RootElement.GetProperty("Payload");

            string title = payloadElement.GetProperty("Title").GetString()!;
            string publisherName = payloadElement.GetProperty("PublisherName").GetString()!;
            // Future-proofing.
            string prodId = payloadElement.GetProperty("ProductId").GetString()!;
            List<Image> logos = new List<Image>();
            List<Image> screenshots = new List<Image>();

            if (payloadElement.TryGetProperty("Images", out JsonElement imagesJson))
            {
                foreach (JsonElement image in imagesJson.EnumerateArray())
                {
                    string type = image.GetProperty("ImageType").GetString()!;
                    string bgColor = "Transparent";
                    if (
                        image.TryGetProperty("BackgroundColor", out JsonElement color)
                        && color.GetString()!.StartsWith('#')
                    )
                    {
                        bgColor = color.GetString()!;
                    }
                    if (type == "logo" | type == "Poster" | type == "BoxArt")
                    {
                        logos.Add(
                            new Image(
                                image.GetProperty("Url").GetString()!,
                                bgColor,
                                image.GetProperty("Height").GetInt32(),
                                image.GetProperty("Width").GetInt32()
                            )
                        );
                    }
                    else if (type == "screenshot")
                    {
                        screenshots.Add(
                            new Image(
                                image.GetProperty("Url").GetString()!,
                                bgColor,
                                image.GetProperty("Height").GetInt32(),
                                image.GetProperty("Width").GetInt32()
                            )
                        );
                    }
                }
            }
            Image logo = logos.LastOrDefault(
                img => img.Height == 100 && img.Width == 100,
                logos[0]
            );

            double? rating = payloadElement.GetProperty("AverageRating").GetDouble()!;
            rating = rating != 0.0 ? rating : null;

            long? ratingCount = payloadElement.GetProperty("RatingCount").GetInt64()!;
            ratingCount = ratingCount != 0 ? ratingCount : null;

            string? revisonId = null;
            if (payloadElement.TryGetProperty("RevisionId", out JsonElement revisionIdJson))
            {
                revisonId = revisionIdJson.GetString();
            }

            long? size = null;
            if (payloadElement.TryGetProperty("ApproximateSizeInBytes", out JsonElement sizeJson))
            {
                size = sizeJson.GetInt64();
            }

            string? shortDescription = null;
            if (
                payloadElement.TryGetProperty("ShortDescription", out JsonElement shortDesJson)
                && shortDesJson.GetString() is string shortDes
                && !string.IsNullOrEmpty(shortDes)
            )
            {
                shortDescription = shortDes;
            }

            string? description = null;
            if (
                payloadElement.TryGetProperty("Description", out JsonElement desJson)
                && desJson.GetString() is string des
                && !string.IsNullOrEmpty(des)
            )
            {
                if (shortDescription is null)
                {
                    int newlineIndex = des.IndexOf("\r\n");
                    if (newlineIndex == -1)
                        newlineIndex = des.IndexOf('\n');

                    int periodIndex = des.IndexOf('.');

                    if (periodIndex != -1)
                    {
                        // period found
                        shortDescription = des[..(periodIndex + 1)];
                    }
                    else if (newlineIndex != -1)
                    {
                        // Newline found, truncate at newline
                        shortDescription = des[..newlineIndex];
                    }
                }
                description = des;
            }

            InstallerType installerType = payloadElement
                .GetProperty("Installer")
                .GetProperty("Type")
                .GetString()! switch
            {
                "WindowsUpdate" => InstallerType.Packaged,
                "WPM" or "DirectInstall" => InstallerType.Unpackaged,
                // GamingServices?
                _ => InstallerType.Unknown,
            };

            string? storeVersion = null;
            try
            {
                if (
                    payloadElement.TryGetProperty("Installer", out var installer)
                    && installer.TryGetProperty("Architectures", out var arch)
                    && arch.ValueKind == JsonValueKind.Object
                )
                {
                    JsonElement selected = default;
                    if (arch.TryGetProperty("x64", out var x64))
                        selected = x64;
                    else if (arch.TryGetProperty("x86", out var x86))
                        selected = x86;

                    if (
                        selected.ValueKind != JsonValueKind.Undefined
                        && selected.TryGetProperty("Version", out var v)
                        && v.ValueKind == JsonValueKind.String
                    )
                    {
                        storeVersion = v.GetString();
                    }
                }
            }
            catch
            {
                // ignore version parsing failures
            }

            bool isBundle = false;
            JsonElement skuElement = payloadElement.GetProperty("Skus").EnumerateArray().First();
            if (skuElement.TryGetProperty("BundledSkus", out JsonElement bundleElement))
            {
                isBundle = true;
            }

            string? packageFamilyName = null;
            if (
                payloadElement.TryGetProperty("PackageFamilyNames", out var pfnArray)
                && pfnArray.ValueKind == JsonValueKind.Array
            )
            {
                packageFamilyName = pfnArray
                    .EnumerateArray()
                    .Select(e => e.GetString())
                    .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
            }

            return Result<StoreEdgeFDProduct>.Success(
                new(
                    prodId,
                    title,
                    logo,
                    screenshots,
                    shortDescription,
                    description,
                    publisherName,
                    revisonId,
                    rating,
                    ratingCount,
                    size,
                    installerType,
                    isBundle,
                    packageFamilyName
                )
                {
                    Version = storeVersion,
                }
            );
        }
        catch (Exception ex)
        {
            return Result<StoreEdgeFDProduct>.Failure(ex);
        }
    }

    public async Task<
        Result<(
            string InstallerUrl,
            string FileName,
            string InstallerSwitches,
            string Version,
            string InstallerSha256
        )>
    > GetUnpackagedInstall(
        Market market,
        Lang language,
        CancellationToken cancellationToken = default
    )
    {
        HttpClient client = Helpers.GetStoreHttpClient();

        try
        {
            string url =
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/packageManifests/{ProductId}";
            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            JsonDocument? json = null;
            try
            {
                json = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(),
                    cancellationToken: cancellationToken
                );
            }
            catch
            {
                // Definitely an error.
                response.EnsureSuccessStatusCode();
            }
            if (!response.IsSuccessStatusCode)
            {
                return Result<(
                    string InstallerUrl,
                    string FileName,
                    string InstallerSwitches,
                    string Version,
                    string InstallerSha256
                )>.Failure(new Exception(json!.RootElement.GetProperty("message").GetString()));
            }

            JsonElement dataElement = json!.RootElement.GetProperty("Data");
            JsonElement versionsArray = dataElement.GetProperty("Versions");

            if (
                versionsArray.ValueKind != JsonValueKind.Array
                || versionsArray.GetArrayLength() == 0
            )
            {
                return Result<(
                    string InstallerUrl,
                    string FileName,
                    string InstallerSwitches,
                    string Version,
                    string InstallerSha256
                )>.Failure(new Exception("Package manifest contains no versions."));
            }

            // Select version: prefer highest parsable `System.Version`, otherwise fall back to [0].
            JsonElement selectedVersion = versionsArray[0];
            System.Version? bestVersion = null;
            foreach (JsonElement v in versionsArray.EnumerateArray())
            {
                string pv = ExtractNumericVersionPrefix(GetString(v, "PackageVersion"));
                if (!System.Version.TryParse(pv, out var parsed))
                    continue;

                if (bestVersion is null || parsed > bestVersion)
                {
                    bestVersion = parsed;
                    selectedVersion = v;
                }
            }

            string version = GetString(selectedVersion, "PackageVersion") ?? "Unknown";

            JsonElement? installersArray = GetArray(selectedVersion, "Installers");
            if (installersArray is null || installersArray.Value.GetArrayLength() == 0)
            {
                return Result<(
                    string InstallerUrl,
                    string FileName,
                    string InstallerSwitches,
                    string Version,
                    string InstallerSha256
                )>.Failure(new Exception("No installers found in the manifest."));
            }

            string packageName = GetObject(selectedVersion, "DefaultLocale")
                is JsonElement localeObj
                ? (GetString(localeObj, "PackageName") ?? "package")
                : "package";

            // Select installer: prefer language match, otherwise fall back to [0].
            JsonElement selectedInstaller = installersArray.Value[0];
            foreach (JsonElement installer in installersArray.Value.EnumerateArray())
            {
                string installerLocale = GetString(installer, "InstallerLocale") ?? string.Empty;
                if (
                    installerLocale.StartsWith(
                        language.ToString(),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    selectedInstaller = installer;
                    break;
                }
            }

            string installerUrl = GetString(selectedInstaller, "InstallerUrl") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(installerUrl))
            {
                return Result<(
                    string InstallerUrl,
                    string FileName,
                    string InstallerSwitches,
                    string Version,
                    string InstallerSha256
                )>.Failure(new Exception("Selected installer has no InstallerUrl."));
            }

            string installerType = (
                GetString(selectedInstaller, "InstallerType") ?? "exe"
            ).ToLowerInvariant();
            string fileName = $"{packageName}.{installerType}";

            string installerSwitches = string.Empty;
            if (GetObject(selectedInstaller, "InstallerSwitches") is JsonElement switches)
                installerSwitches = GetString(switches, "Silent") ?? string.Empty;

            string installerSha256 =
                GetString(selectedInstaller, "InstallerSha256") ?? string.Empty;

            // Store the version on the product instance for later update detection.
            Version = version;

            return Result<(
                string InstallerUrl,
                string FileName,
                string InstallerSwitches,
                string Version,
                string InstallerSha256
            )>.Success((installerUrl, fileName, installerSwitches, version, installerSha256));
        }
        catch (Exception ex)
        {
            return Result<(
                string InstallerUrl,
                string FileName,
                string InstallerSwitches,
                string Version,
                string InstallerSha256
            )>.Failure(ex);
        }
    }

    private static string? GetString(JsonElement obj, string name) =>
        obj.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString()
            : null;

    private static JsonElement? GetObject(JsonElement obj, string name) =>
        obj.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Object ? p : null;

    private static JsonElement? GetArray(JsonElement obj, string name) =>
        obj.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Array ? p : null;

    private static string ExtractNumericVersionPrefix(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return string.Empty;

        // e.g. "6.6.11 (23272)" => "6.6.11"
        ReadOnlySpan<char> span = s.AsSpan().Trim();
        int space = span.IndexOf(' ');
        if (space > 0)
            span = span[..space];
        return span.ToString();
    }
}
