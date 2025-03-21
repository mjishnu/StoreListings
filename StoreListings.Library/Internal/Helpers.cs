using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace StoreListings.Library.Internal;

internal static class Helpers
{
    private static HttpClientHandler? _handler;
    private static HttpClient? _storeHttpClient;
    private static HttpClient? _fe3HttpClient;

    public static HttpClient GetStoreHttpClient()
    {
        if (_storeHttpClient is not null)
            return _storeHttpClient;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _handler = new HttpClientHandler();
            _handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            _storeHttpClient = new HttpClient(_handler);
        }
        else
        {
            _storeHttpClient = new HttpClient();
        }

        _storeHttpClient.DefaultRequestHeaders.Accept.Clear();
        _storeHttpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("*/*")
        );
        _storeHttpClient.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue("en-US")
        );
        _storeHttpClient.DefaultRequestHeaders.Add("User-Agent", "WindowsStore/22106.1401.2.0");

        _storeHttpClient.DefaultRequestHeaders.Add("MS-CV", CorrelationVector.Increment());
        _storeHttpClient.DefaultRequestHeaders.Add("OSIsGenuine", "True");
        _storeHttpClient.DefaultRequestHeaders.Add("OSIsSMode", "False");
        return _storeHttpClient;
    }

    public static HttpClient GetFE3StoreHttpClient()
    {
        if (_fe3HttpClient is not null)
            return _fe3HttpClient;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _handler = new HttpClientHandler();
            _handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            _fe3HttpClient = new HttpClient(_handler);
        }
        else
        {
            _fe3HttpClient = new HttpClient();
        }

        _fe3HttpClient.DefaultRequestHeaders.Add(
            "User-Agent",
            "Windows-Update-Agent/10.0.10011.16384 Client-Protocol/2.1"
        );
        _fe3HttpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        return _fe3HttpClient;
    }

    public static string ToBase64Url(string input)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
    }

    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];

        var randomBytes = new byte[length];
        RandomNumberGenerator.Fill(randomBytes);

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[randomBytes[i] % chars.Length];
        }

        return new string(stringChars);
    }

    public static List<Card> GetCards(JsonElement cardsElement)
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
