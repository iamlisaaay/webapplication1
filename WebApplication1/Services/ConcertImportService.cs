using ClosedXML.Excel;
using Concert.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Concert.Services;

public class ConcertImportService : IImportService<Concert.Models.Concert>
{
    private readonly ConcertContext _context;
    public ConcertImportService(ConcertContext context) => _context = context;

    public async Task<List<ImportError>> ImportFromStreamAsync(Stream stream, CancellationToken ct)
    {
        var errors = new List<ImportError>();
        var validConcertsToSave = new List<Concert.Models.Concert>();

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        int rowNumber = 1;
        foreach (var row in worksheet.RowsUsed().Skip(1)) // Пропускаємо заголовок
        {
            rowNumber++;
            var title = row.Cell(1).GetValue<string>();
            if (string.IsNullOrEmpty(title)) continue;

            try
            {
                // 1. Читання та перевірка дати
                if (!row.Cell(2).TryGetValue<DateTime>(out var parsedDate))
                {
                    errors.Add(new ImportError { RowNumber = rowNumber, ConcertTitle = title, ErrorMessage = "Некоректний формат дати." });
                    continue;
                }
                var concertDateUtc = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                var targetDate = concertDateUtc.Date;

                // 2. Шукаємо майданчик (Venue) - ТІЛЬКИ ПЕРЕВІРКА ІСНУВАННЯ
                var venueName = row.Cell(3).GetValue<string>();
                var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Name == venueName, ct);

                if (venue == null)
                {
                    errors.Add(new ImportError { RowNumber = rowNumber, ConcertTitle = title, ErrorMessage = $"Майданчик '{venueName}' не знайдено. Спочатку додайте його в систему." });
                    continue;
                }

                // Перевірка: чи є вже концерт на цьому майданчику в цю дату (в БД або в цьому ж файлі Excel)
                bool isVenueBusyDb = await _context.Concerts.AnyAsync(c => c.VenueId == venue.VenueId && c.DateTime.HasValue && c.DateTime.Value.Date == targetDate, ct);
                bool isVenueBusyExcel = validConcertsToSave.Any(c => c.VenueId == venue.VenueId && c.DateTime.HasValue && c.DateTime.Value.Date == targetDate);
                if (isVenueBusyDb || isVenueBusyExcel)
                {
                    errors.Add(new ImportError { RowNumber = rowNumber, ConcertTitle = title, ErrorMessage = $"На майданчику '{venueName}' вже заплановано концерт на {targetDate:dd.MM.yyyy}." });
                    continue;
                }

                // 3. Обробка та перевірка гуртів
                var groupNamesStr = row.Cell(4).GetValue<string>();
                if (string.IsNullOrWhiteSpace(groupNamesStr))
                {
                    errors.Add(new ImportError { RowNumber = rowNumber, ConcertTitle = title, ErrorMessage = "Не вказано жодної групи." });
                    continue;
                }

                var groupNames = groupNamesStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(g => g.Trim());
                var groupsForConcert = new List<Group>();
                bool hasGroupErrors = false;

                foreach (var gName in groupNames)
                {
                    var group = await _context.Groups.FirstOrDefaultAsync(g => g.Name == gName, ct);
                    if (group == null)
                    {
                        errors.Add(new ImportError { RowNumber = rowNumber, ConcertTitle = title, ErrorMessage = $"Гурт '{gName}' не знайдено. Спочатку додайте його в систему." });
                        hasGroupErrors = true;
                        break; // припиняємо перевірку гуртів для цього рядка
                    }

                    // Перевірка: чи не має цей гурт концерту в цей же день (в БД або в цьому ж файлі)
                    bool isGroupBusyDb = await _context.Concerts.AnyAsync(c => c.DateTime.HasValue && c.DateTime.Value.Date == targetDate && c.Groups.Any(g => g.GroupId == group.GroupId), ct);
                    bool isGroupBusyExcel = validConcertsToSave.Any(c => c.DateTime.HasValue && c.DateTime.Value.Date == targetDate && c.Groups.Any(g => g.GroupId == group.GroupId));
                    if (isGroupBusyDb || isGroupBusyExcel)
                    {
                        errors.Add(new ImportError { RowNumber = rowNumber, ConcertTitle = title, ErrorMessage = $"Гурт '{gName}' вже виступає в іншому місці {targetDate:dd.MM.yyyy}." });
                        hasGroupErrors = true;
                        break;
                    }

                    groupsForConcert.Add(group);
                }

                if (hasGroupErrors) continue; // Пропускаємо рядок, якщо була помилка з гуртами

                // 4. Створюємо об'єкт концерту (додаємо лише правильні)
                var concert = new Concert.Models.Concert
                {
                    Title = title,
                    DateTime = concertDateUtc,
                    Venue = venue,
                    VenueId = venue.VenueId,
                    ImageUrl = row.Cell(5).GetValue<string>()
                };

                foreach (var g in groupsForConcert)
                {
                    concert.Groups.Add(g);
                }

                validConcertsToSave.Add(concert);
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { RowNumber = rowNumber, ConcertTitle = title, ErrorMessage = "Помилка читання даних: " + ex.Message });
            }
        }

        // Зберігаємо ТІЛЬКИ ті рядки, що пройшли всі перевірки
        if (validConcertsToSave.Any())
        {
            _context.Concerts.AddRange(validConcertsToSave);
            await _context.SaveChangesAsync(ct);
        }

        return errors; // Повертаємо список помилок у контролер
    }
}