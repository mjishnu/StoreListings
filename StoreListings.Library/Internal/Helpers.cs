using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace StoreListings.Library.Internal;

public static class JsonExtensions
{
    public static string GetStringSafe(this JsonElement element, string propName)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propName, out var prop)
            && prop.ValueKind == JsonValueKind.String
        )
        {
            var value = prop.GetString();
            if (value is not null)
            {
                return value;
            }
        }
        return string.Empty;
    }

    public static long GetLongSafe(this JsonElement element, string propName)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propName, out var prop)
            && prop.ValueKind == JsonValueKind.Number
        )
        {
            return prop.GetInt64();
        }
        return 0;
    }

    public static int GetIntSafe(this JsonElement element, string propName)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propName, out var prop)
            && prop.ValueKind == JsonValueKind.Number
        )
        {
            return prop.GetInt32();
        }
        return 0;
    }

    public static double GetDoubleSafe(this JsonElement element, string propName)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propName, out var prop)
            && prop.ValueKind == JsonValueKind.Number
        )
        {
            return prop.GetDouble();
        }
        return 0;
    }

    public static bool GetBoolSafe(this JsonElement element, string propName)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propName, out var prop)
        )
        {
            if (prop.ValueKind == JsonValueKind.True)
                return true;
            if (prop.ValueKind == JsonValueKind.False)
                return false;
        }
        return false;
    }

    public static JsonElement GetPropertySafe(this JsonElement element, string propName)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propName, out var prop)
        )
        {
            return prop;
        }
        return default;
    }

    public static JsonElement? GetFirstArrayElementOrNull(this JsonElement element, string propName)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propName, out var prop)
            && prop.ValueKind == JsonValueKind.Array
            && prop.GetArrayLength() > 0
        )
        {
            return prop[0];
        }
        return null;
    }

    public static JsonElement GetArraySafe(this JsonElement element, string propName)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propName, out var prop)
            && prop.ValueKind == JsonValueKind.Array
        )
        {
            return prop;
        }
        return JsonDocument.Parse("[]").RootElement;
    }
}

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
        _storeHttpClient.DefaultRequestHeaders.Add("User-Agent", "WindowsStore/22512.1401.1101.0");

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
                // 1. Image Selection Logic
                var imagesElement = card.GetPropertySafe("Images");
                var images =
                    imagesElement.ValueKind == JsonValueKind.Array
                        ? imagesElement.EnumerateArray()
                        : Enumerable.Empty<JsonElement>();

                // Try to find the last 300x300 image, fallback to the first image available
                var targetImage = images
                    .Where(img => img.GetIntSafe("Height") == 300 && img.GetIntSafe("Width") == 300)
                    .LastOrDefault();

                // If LastOrDefault returns default (Undefined), fallback to the first image
                if (targetImage.ValueKind == JsonValueKind.Undefined)
                {
                    targetImage = images.FirstOrDefault();
                }

                // 2. Background Color Logic
                string bgColor = targetImage.GetStringSafe("BackgroundColor");
                if (!bgColor.StartsWith('#'))
                {
                    bgColor = "Transparent";
                }

                // 3. Installer Type Logic
                string installerTypeStr = card.GetPropertySafe("Installer").GetStringSafe("Type");
                if (string.IsNullOrEmpty(installerTypeStr))
                {
                    installerTypeStr = card.GetStringSafe("InstallerType");
                }
                var installerType = installerTypeStr switch
                {
                    "WindowsUpdate" => InstallerType.Packaged,
                    "WPM" or "DirectInstall" => InstallerType.Unpackaged,
                    _ => InstallerType.Unknown,
                };

                // 4. Construct Card
                return new Card(
                    card.GetStringSafe("ProductId"),
                    card.GetStringSafe("Title"),
                    card.GetStringSafe("DisplayPrice"),
                    card.GetDoubleSafe("AverageRating"),
                    installerType,
                    new Image(
                        targetImage.GetStringSafe("Url"),
                        bgColor,
                        targetImage.GetIntSafe("Height"),
                        targetImage.GetIntSafe("Width")
                    )
                );
            })
            .ToList();
    }
}
