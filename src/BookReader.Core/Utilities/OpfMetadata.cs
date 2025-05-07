namespace BookReader.Core.Utilities;

public class OpfMetadata
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string CoverPath { get; set; }
    public List<string> SpineItems { get; set; } = new();
}
