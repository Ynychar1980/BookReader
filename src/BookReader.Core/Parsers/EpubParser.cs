using AngleSharp;
using BookReader.Core.Models;
using BookReader.Core.Services;
using BookReader.Core.Utilities;
using SharpCompress.Archives;
using HtmlAgilityPack;

namespace BookReader.Core.Parsers;

public class EpubParser : IBookParser
{
    public Book ParseBook(Stream bookStream, string filePath)
    {
        using var archive = ArchiveFactory.Open(bookStream);

        try
        {
            // 1. Найти файл метаданных (OPF)
            var opfEntry = archive.Entries.FirstOrDefault(e => e.Key.EndsWith(".opf"));
            if (opfEntry == null) throw new InvalidDataException("Invalid EPUB");

            // 2. Парсим метаданные
            var opfContent = new StreamReader(opfEntry.OpenEntryStream()).ReadToEnd();
            var metadata = ParseOpfMetadata(opfContent);

            // 3. Извлекаем CSS-файлы
            var cssFiles = ExtractCssFiles(archive);

            // 4. Извлекаем главы
            var chapters = new List<Chapter>();
            foreach (var item in metadata.SpineItems)
            {
                var entry = archive.Entries.FirstOrDefault(e => e.Key == item);
                if (entry == null) continue;

                var content = new StreamReader(entry.OpenEntryStream()).ReadToEnd();

                // 5. добавляем CSS-стили
                var processedContent = ProcessChapterContent(content, cssFiles);

                chapters.Add(new Chapter {
                    Title = item,
                    Content = processedContent
                });
            }

            return new Book
            {
                Title = metadata.Title,
                Author = metadata.Author,
                Chapters = chapters,
                CoverImage = LoadCover(archive, metadata.CoverPath),
                FilePath = filePath
            };
        }
        catch (Exception ex)
        {
            LoggerService.LogError("Ошибка парсинга EPUB", ex);
            throw;
        }
    }

    private byte[] LoadCover(IArchive archive, string coverPath)
    {
        var coverEntry = archive.Entries.FirstOrDefault(e => e.Key == coverPath);
        if (coverEntry == null) return Array.Empty<byte>();

        using var ms = new MemoryStream();
        coverEntry.OpenEntryStream().CopyTo(ms);
        return ms.ToArray();
    }

    private OpfMetadata ParseOpfMetadata(string opfContent)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var document = context.OpenAsync(req => req.Content(opfContent)).Result;

        var metadata = new OpfMetadata();

        // Извлечение названия и автора
        metadata.Title = document.QuerySelector("dc|title")?.TextContent ?? "Без названия";
        metadata.Author = document.QuerySelector("dc|creator")?.TextContent ?? "Неизвестный автор";

        // Поиск обложки
        var metaCover = document.QuerySelector("meta[name='cover']");
        if (metaCover != null)
        {
            var coverId = metaCover.GetAttribute("content");
            var coverItem = document.QuerySelector($"item#${coverId}");
            metadata.CoverPath = coverItem?.GetAttribute("href");
        }

        // Порядок глав (spine)
        var spineItems = document.QuerySelectorAll("spine itemref");
        foreach (var item in spineItems)
        {
            var idref = item.GetAttribute("idref");
            var manifestitem = document.QuerySelector($"manifest item#${idref}");
            if (manifestitem != null)
            {
                metadata.SpineItems.Add(manifestitem.GetAttribute("href"));
            }
        }

        return metadata;
    }

    private List<string> ExtractCssFiles(IArchive archive)
    {
        return archive.Entries
            .Where(e => e.Key.EndsWith(".css"))
            .Select(e => e.Key)
            .ToList();
    }

    private string ProcessChapterContent(string htmlContent, List<string> cssFiles)
    {
        if (string.IsNullOrEmpty(htmlContent)) return htmlContent;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var headNode = htmlDoc.DocumentNode.SelectSingleNode("//head");

        // Если нет тега head - создаем его
        if (headNode == null)
        {
            headNode = HtmlNode.CreateNode("<head></head>");
            htmlDoc.DocumentNode.InsertBefore(headNode, htmlDoc.DocumentNode.FirstChild);
        }

        // Добавляем CSS-ссылки
        foreach (var cssPath in cssFiles)
        {
            var linkTag = $"<link rel='stylesheet' href='{cssPath}'>";
            headNode.AppendChild(HtmlNode.CreateNode(linkTag));
        }

        return htmlDoc.DocumentNode.OuterHtml;
    }
}
