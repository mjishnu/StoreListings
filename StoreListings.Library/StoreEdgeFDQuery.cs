using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using StoreListings.Library.Internal;

namespace StoreListings.Library;

public class StoreEdgeFDQuery
{
    /// <summary>
    /// list of cards returned by the query.
    /// </summary>
    public required List<Card> Cards { get; set; }

    [SetsRequiredMembers]
    private StoreEdgeFDQuery(List<Card> cards)
    {
        Cards = cards;
    }

    public static async Task<Result<StoreEdgeFDQuery>> GetRecommendations(
        Category category,
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
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/recommendations/collections/{category}?market={market}&locale={language}-{market}&deviceFamily=Windows.{deviceFamily}";
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
                response.EnsureSuccessStatusCode();
            }

            using JsonDocument jsondoc = json!;

            if (!response.IsSuccessStatusCode)
            {
                return Result<StoreEdgeFDQuery>.Failure(
                    new Exception(jsondoc.RootElement.GetProperty("message").GetString())
                );
            }

            JsonElement payloadElement = jsondoc.RootElement.GetProperty("Payload");
            JsonElement cardsElement = payloadElement.GetProperty("Cards");

            if (
                payloadElement.TryGetProperty("Cards", out JsonElement cardElement)
                && cardElement.GetArrayLength() >= 1
            )
            {
                return Result<StoreEdgeFDQuery>.Success(new(GetCards(cardElement)));
            }
            else
            {
                throw new Exception("No recommendations found");
            }
        }
        catch (Exception ex)
        {
            return Result<StoreEdgeFDQuery>.Failure(ex);
        }
    }

    public static async Task<Result<StoreEdgeFDQuery>> GetSearchProduct(
        string query,
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
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/pages/searchResults?query={query}&market={market}&locale={language}-{market}&deviceFamily=Windows.{deviceFamily}";
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
                response.EnsureSuccessStatusCode();
            }

            using JsonDocument jsondoc = json!;

            if (!response.IsSuccessStatusCode)
            {
                return Result<StoreEdgeFDQuery>.Failure(
                    new Exception(jsondoc.RootElement.GetProperty("message").GetString())
                );
            }

            JsonElement payloadElement = jsondoc
                .RootElement.EnumerateArray()
                .Last()
                .GetProperty("Payload");

            if (
                payloadElement.TryGetProperty("SearchResults", out JsonElement searchElement)
                && searchElement.GetArrayLength() >= 1
            )
            {
                return Result<StoreEdgeFDQuery>.Success(new(GetCards(searchElement)));
            }
            else
            {
                throw new Exception("No search results found");
            }
        }
        catch (Exception ex)
        {
            return Result<StoreEdgeFDQuery>.Failure(ex);
        }
    }

    public static async Task<Result<StoreEdgeFDQuery>> GetSearchSuggestion(
        string query,
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
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/autosuggest?prefix={query}&market={market}&locale={language}-{market}&deviceFamily=Windows.{deviceFamily}";
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
                response.EnsureSuccessStatusCode();
            }

            using JsonDocument jsondoc = json!;

            if (!response.IsSuccessStatusCode)
            {
                return Result<StoreEdgeFDQuery>.Failure(
                    new Exception(jsondoc.RootElement.GetProperty("message").GetString())
                );
            }

            JsonElement payloadElement = jsondoc.RootElement.GetProperty("Payload");
            if (
                payloadElement.TryGetProperty("AssetSuggestions", out JsonElement suggestionElement)
                && suggestionElement.GetArrayLength() >= 1
            )
            {
                return Result<StoreEdgeFDQuery>.Success(new(GetCards(suggestionElement)));
            }
            else
            {
                throw new Exception("No search suggestions found");
            }
        }
        catch (Exception ex)
        {
            return Result<StoreEdgeFDQuery>.Failure(ex);
        }
    }

    public static async Task<Result<StoreEdgeFDQuery>> GetBundles(
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
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/products/{productId}/BundleParts?&market={market}&locale={language}-{market}&deviceFamily=Windows.{deviceFamily}";
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
                response.EnsureSuccessStatusCode();
            }

            using JsonDocument jsondoc = json!;

            if (!response.IsSuccessStatusCode)
            {
                return Result<StoreEdgeFDQuery>.Failure(
                    new Exception(jsondoc.RootElement.GetProperty("message").GetString())
                );
            }

            JsonElement payloadElement = jsondoc.RootElement.GetProperty("Payload");
            if (
                payloadElement.TryGetProperty("0017", out JsonElement _0017Element)
                && _0017Element.GetProperty("Products").GetArrayLength() >= 1
            )
            {
                return Result<StoreEdgeFDQuery>.Success(
                    new(GetCards(_0017Element.GetProperty("Products")))
                );
            }
            else if (
                payloadElement.TryGetProperty("0010", out JsonElement _0010Element)
                && _0010Element.GetProperty("Products").GetArrayLength() >= 1
            )
            {
                return Result<StoreEdgeFDQuery>.Success(
                    new(GetCards(_0010Element.GetProperty("Products")))
                );
            }
            else
            {
                throw new Exception("No bundles found");
            }
        }
        catch (Exception ex)
        {
            return Result<StoreEdgeFDQuery>.Failure(ex);
        }
    }

    private static List<Card> GetCards(JsonElement cardsElement)
    {
        return cardsElement
            .EnumerateArray()
            .Select(card =>
            {
                List<JsonElement> images = card.GetProperty("Images").EnumerateArray().ToList();
                IEnumerable<JsonElement> filteredImages = images.Where(img =>
                    img.GetProperty("Height").GetInt32() == 300
                    && img.GetProperty("Width").GetInt32() == 300
                );
                JsonElement image = filteredImages.Any() ? filteredImages.Last() : images[0];
                string imageBackgroundColor = "Transparent";
                if (
                    image.TryGetProperty("BackgroundColor", out JsonElement color)
                    && color.GetString()!.StartsWith('#')
                )
                {
                    imageBackgroundColor = color.GetString()!;
                }
                string? displayPrice;
                if (image.TryGetProperty("DisplayPrice", out JsonElement price))
                {
                    displayPrice = price.GetString()!;
                }
                else
                {
                    displayPrice = null;
                }
                double? averageRating = null;
                if (
                    card.TryGetProperty("AverageRating", out JsonElement rating)
                    && rating.GetDouble() != 0.0
                )
                {
                    averageRating = rating.GetDouble();
                }

                return new Card(
                    card.GetProperty("ProductId").GetString()!,
                    card.GetProperty("Title").GetString()!,
                    displayPrice,
                    averageRating,
                    new Image(
                        image.GetProperty("Url").GetString()!,
                        imageBackgroundColor,
                        image.GetProperty("Height").GetInt32(),
                        image.GetProperty("Width").GetInt32()
                    )
                );
            })
            .ToList();
    }
}
