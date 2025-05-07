namespace BookReader.Core.Models;

public class Book
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string Author { get; set; }
    public List<Chapter> Chapters { get; set; } = new();
    public byte[] CoverImage { get; set; }
    public string FilePath { get; set; }
}
