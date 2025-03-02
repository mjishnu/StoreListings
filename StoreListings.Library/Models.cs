namespace StoreListings.Library;

public sealed class Image
{
    public string Url { get; set; }
    public string BackgroundColor { get; }
    public int Height { get; }
    public int Width { get; }

    public Image(string url, string backgroundColor, int height, int width)
    {
        Url = url;
        BackgroundColor = backgroundColor;
        Height = height;
        Width = width;
    }
}

public sealed class Card
{
    public string ProductId { get; }
    public string Title { get; }
    public string? DisplayPrice { get; }
    public double? AverageRating { get; }
    public Image Image { get; }

    public Card(
        string productId,
        string title,
        string? displayPrice,
        double? averageRating,
        Image image
    )
    {
        ProductId = productId;
        Title = title;
        DisplayPrice = displayPrice;
        AverageRating = averageRating;
        Image = image;
    }
}
