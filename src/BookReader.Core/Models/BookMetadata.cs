namespace BookReader.Core.Models;

public class BookMetadata
{
    public string BookId { get; set; }
    public string CurrentChapterId { get; set; }
    public int ScrollPosition { get; set; }
}
