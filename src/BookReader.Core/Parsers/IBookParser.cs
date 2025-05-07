using BookReader.Core.Models;

namespace BookReader.Core.Parsers;

internal interface IBookParser
{
    Book ParseBook(Stream bookStream, string filePath);
}
