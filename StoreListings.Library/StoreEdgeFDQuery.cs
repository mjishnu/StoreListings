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
        MediaTypeRecommendation mediaType = MediaTypeRecommendation.Apps,
        int skipItems = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    )
    {
        HttpClient client = Helpers.GetStoreHttpClient();

        var mediaTypeString = mediaType.ToString();

        try
        {
            string url =
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/recommendations/collections/{category}?market={market}&locale={language}-{market}&deviceFamily=Windows.{deviceFamily}&mediaType={mediaTypeString}&pageSize={pageSize}&skipItems={skipItems}";
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
                return Result<StoreEdgeFDQuery>.Success(new(Helpers.GetCards(cardElement)));
            }
            return Result<StoreEdgeFDQuery>.Success(new StoreEdgeFDQuery(new List<Card>()));
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
        int skipItems = 0,
        MediaTypeSearch mediaType = MediaTypeSearch.All,
        PriceType priceType = PriceType.All,
        CancellationToken cancellationToken = default
    )
    {
        HttpClient client = Helpers.GetStoreHttpClient();

        string filters = $"PriceType%3d{priceType}";
        if (priceType == PriceType.All)
        {
            filters = "";
        }

        // cursor is a base64 url encoded string of length 35
        var randomString = Helpers.GenerateRandomString(21 - skipItems.ToString().Length);
        var base64string = Helpers.ToBase64Url($"o={skipItems}&s={randomString}");

        try
        {
            string url =
                $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/search?query={query}&market={market}&locale={language}-{market}&deviceFamily=Windows.{deviceFamily}&mediaType={mediaType}&filters={filters}&cursor={base64string}%3d";
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
                payloadElement.TryGetProperty("SearchResults", out JsonElement searchElement)
                && searchElement.GetArrayLength() >= 1
            )
            {
                return Result<StoreEdgeFDQuery>.Success(new(Helpers.GetCards(searchElement)));
            }
            return Result<StoreEdgeFDQuery>.Success(new StoreEdgeFDQuery(new List<Card>()));
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
                    new(Helpers.GetCards(_0017Element.GetProperty("Products")))
                );
            }
            else if (
                payloadElement.TryGetProperty("0010", out JsonElement _0010Element)
                && _0010Element.GetProperty("Products").GetArrayLength() >= 1
            )
            {
                return Result<StoreEdgeFDQuery>.Success(
                    new(Helpers.GetCards(_0010Element.GetProperty("Products")))
                );
            }
            return Result<StoreEdgeFDQuery>.Success(new StoreEdgeFDQuery(new List<Card>()));
        }
        catch (Exception ex)
        {
            return Result<StoreEdgeFDQuery>.Failure(ex);
        }
    }
}
