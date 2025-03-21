using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using StoreListings.Library.Internal;

namespace StoreListings.Library;

public class StoreEdgeFDSuggestions
{
    /// <summary>
    /// list of cards and suggestions returned by the query.
    /// </summary>
    public List<string> Suggestions { get; set; }
    public required List<Card> Cards { get; set; }

    [SetsRequiredMembers]
    private StoreEdgeFDSuggestions(List<Card> cards, List<string> suggestions)
    {
        Cards = cards;
        Suggestions = suggestions;
    }

    public static async Task<Result<StoreEdgeFDSuggestions>> GetSearchSuggestion(
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
                return Result<StoreEdgeFDSuggestions>.Failure(
                    new Exception(jsondoc.RootElement.GetProperty("message").GetString())
                );
            }

            JsonElement payloadElement = jsondoc.RootElement.GetProperty("Payload");
            if (
                (
                    payloadElement.TryGetProperty(
                        "AssetSuggestions",
                        out JsonElement suggestionCards
                    )
                    && suggestionCards.GetArrayLength() >= 1
                )
                && (
                    payloadElement.TryGetProperty("SearchSuggestions", out JsonElement suggestions)
                    && suggestions.GetArrayLength() >= 1
                )
            )
            {
                List<string> suggestionsList =
                [
                    .. suggestions
                        .EnumerateArray()
                        .Select(item => item.GetString())
                        .Where(item => !string.IsNullOrEmpty(item)),
                ];
                return Result<StoreEdgeFDSuggestions>.Success(
                    new(Helpers.GetCards(suggestionCards), suggestionsList)
                );
            }
            return Result<StoreEdgeFDSuggestions>.Success(new StoreEdgeFDSuggestions([], []));
        }
        catch (Exception ex)
        {
            return Result<StoreEdgeFDSuggestions>.Failure(ex);
        }
    }
}
