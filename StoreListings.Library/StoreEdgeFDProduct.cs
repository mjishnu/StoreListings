using System.Diagnostics.CodeAnalysis;
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
        bool isBundle
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
                "WPM" => InstallerType.Unpackaged,
                // GamingServices?
                _ => InstallerType.Unknown,
            };

            bool isBundle = false;
            JsonElement skuElement = payloadElement.GetProperty("Skus").EnumerateArray().First();
            if (skuElement.TryGetProperty("BundledSkus", out JsonElement bundleElement))
            {
                isBundle = true;
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
                    isBundle
                )
            );
        }
        catch (Exception ex)
        {
            return Result<StoreEdgeFDProduct>.Failure(ex);
        }
    }

    public async Task<Result<(string InstallerUrl, string InstallerSwitches)>> GetUnpackagedInstall(
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
                return Result<(string InstallerUrl, string InstallerSwitches)>.Failure(
                    new Exception(json!.RootElement.GetProperty("message").GetString())
                );
            }
            var installers = json!
                .RootElement.GetProperty("Data")
                .GetProperty("Versions")[0]
                .GetProperty("Installers");
            List<(string InstallerUrl, string InstallerSwitches, uint Priority)> installersList =
                new(2);
            for (int i = 0; i < installers.GetArrayLength(); i++)
            {
                JsonElement installer = installers[i];
                string locale = installer.GetProperty("InstallerLocale").GetString()!;
                if (!locale.StartsWith(language.ToString(), StringComparison.OrdinalIgnoreCase))
                    continue;
                int priority = locale.Equals(
                    $"{language}-{market}",
                    StringComparison.OrdinalIgnoreCase
                )
                    ? 1
                    : 0;
                installersList.Add(
                    (
                        installer.GetProperty("InstallerUrl").GetString()!,
                        installer
                            .GetProperty("InstallerSwitches")
                            .GetProperty("Silent")
                            .GetString()!,
                        (uint)priority
                    )
                );
            }
            if (installersList.Count == 0)
            {
                return Result<(string InstallerUrl, string InstallerSwitches)>.Failure(
                    new Exception("No installer found for the specified language and market.")
                );
            }
            (string InstallerUrl, string InstallerSwitches, uint Priority) highestPriority =
                installersList.OrderByDescending(f => f.Priority).ElementAt(0);
            return Result<(string InstallerUrl, string InstallerSwitches)>.Success(
                (highestPriority.InstallerUrl, highestPriority.InstallerSwitches)
            );
        }
        catch (Exception ex)
        {
            return Result<(string InstallerUrl, string InstallerSwitches)>.Failure(ex);
        }
    }
}
