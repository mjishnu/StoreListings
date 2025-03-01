namespace StoreListings.Library;

public record struct Image(
    string Url,
    string BackgroundColor,
    int Height,
    int Width
);

public record Card(
    string ProductId,
    string Title,
    string? DisplayPrice,
    double? AverageRating,
    Image Image

);
