using ClosedXML.Excel;
using Concert.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Concert.Services;

public class ConcertImportService : IImportService<Concert.Models.Concert>
{
    private readonly ConcertContext _context;
    public ConcertImportService(ConcertContext context) => _context = context;

    public async Task ImportFromStreamAsync(Stream stream, CancellationToken ct)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1); // Читаємо перший лист

        foreach (var row in worksheet.RowsUsed().Skip(1)) // Пропускаємо заголовок
        {
            var title = row.Cell(1).GetValue<string>();
            if (string.IsNullOrEmpty(title)) continue;

            // 1. Шукаємо або створюємо майданчик (Venue)
            var venueName = row.Cell(3).GetValue<string>();
            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Name == venueName, ct);
            if (venue == null)
            {
                venue = new Venue { Name = venueName, Address = "Адреса з Excel", Capacity = 100 };
                _context.Venues.Add(venue);
            }

            // 2. Створюємо об'єкт концерту
            var concert = new Concert.Models.Concert
            {
                Title = title,
                DateTime = row.Cell(2).GetDateTime(),
                Venue = venue,
                ImageUrl = row.Cell(5).GetValue<string>()
            };

            // 3. Обробка гуртів (розділені комою в 4-й колонці)
            var groupNames = row.Cell(4).GetValue<string>().Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var gName in groupNames)
            {
                var name = gName.Trim();
                var group = await _context.Groups.FirstOrDefaultAsync(g => g.Name == name, ct);
                if (group == null)
                {
                    group = new Group { Name = name };
                    _context.Groups.Add(group);
                }
                concert.Groups.Add(group);
            }

            _context.Concerts.Add(concert);
        }
        await _context.SaveChangesAsync(ct);
    }
}