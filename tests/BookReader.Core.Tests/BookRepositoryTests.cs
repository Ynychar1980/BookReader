using BookReader.Core.Services;

namespace BookReader.Core.Tests;

public class BookRepositoryTests : IDisposable
{
    private readonly string _testDbPath = Path.Combine(Path.GetTempPath(), "test.db");
    private BookRepository _repo;

    public BookRepositoryTests()
    {
        _repo = new BookRepository(_testDbPath);
    }

    [Fact]
    public async Task SaveProgress_SavesCorrectData()
    {
        // Arrange
        var bookId = "test-book";

        // Act
        await _repo.SaveProgressAsync(bookId, "chapter1", 150);
        var result = await _repo.GetMetadataAsync(bookId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(150, result.ScrollPosition);
    }

    public void Dispose()
    {
        if (File.Exists(_testDbPath))
            File.Delete(_testDbPath);
    }
}
