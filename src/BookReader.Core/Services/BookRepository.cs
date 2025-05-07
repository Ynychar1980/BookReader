using BookReader.Core.Helpers;
using BookReader.Core.Models;
using SQLite;

namespace BookReader.Core.Services;

public class BookRepository
{
    private SQLiteAsyncConnection _db;

    public BookRepository(string dbPath)
    {
        _db = new SQLiteAsyncConnection(dbPath);
        InitializeAsync().SafeFireAndForget();
    }

    private async Task InitializeAsync()
    {
        await _db.CreateTableAsync<BookMetadata>();
    }

    public async Task<BookMetadata> GetMetadataAsync(string bookId)
    {
        return await _db.Table<BookMetadata>()
            .FirstOrDefaultAsync(b  => b.BookId == bookId);
    }

    public async Task SaveProgressAsync(string bookId, string chapterId, int scrollPosition)
    {
        var meta = new BookMetadata
        {
            BookId = bookId,
            CurrentChapterId = chapterId,
            ScrollPosition = scrollPosition
        };
        await _db.InsertOrReplaceAsync(meta);
    }
}
