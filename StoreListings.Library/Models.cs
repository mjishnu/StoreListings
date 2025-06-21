namespace StoreListings.Library;

public sealed class Image(string url, string backgroundColor, int height, int width)
{
    public string Url { get; set; } = url;
    public string BackgroundColor { get; } = backgroundColor;
    public int Height { get; } = height;
    public int Width { get; } = width;
}

public sealed class Card(
    string productId,
    string title,
    string? displayPrice,
    double? averageRating,
    Image image
)
{
    public string ProductId { get; } = productId;
    public string Title { get; } = title;
    public string? DisplayPrice { get; } = displayPrice;
    public double? AverageRating { get; } = averageRating;
    public Image Image { get; } = image;
}
