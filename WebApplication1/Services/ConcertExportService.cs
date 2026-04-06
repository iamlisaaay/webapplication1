using ClosedXML.Excel;
using Concert.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Concert.Services;

public class ConcertExportService : IExportService<Concert.Models.Concert>
{
    private readonly ConcertContext _context;
    public ConcertExportService(ConcertContext context) => _context = context;

    public async Task WriteToAsync(Stream stream, CancellationToken ct)
    {
        var concerts = await _context.Concerts
            .Include(c => c.Venue)
            .Include(c => c.Groups)
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Концерти");

        // Заголовки таблиці
        ws.Cell(1, 1).Value = "Назва";
        ws.Cell(1, 2).Value = "Дата";
        ws.Cell(1, 3).Value = "Майданчик";
        ws.Cell(1, 4).Value = "Гурти";
        ws.Cell(1, 5).Value = "Посилання на афішу";
        ws.Row(1).Style.Font.Bold = true;

        int rowIdx = 2;
        foreach (var c in concerts)
        {
            ws.Cell(rowIdx, 1).Value = c.Title;
            ws.Cell(rowIdx, 2).Value = c.DateTime;
            ws.Cell(rowIdx, 3).Value = c.Venue?.Name;
            ws.Cell(rowIdx, 4).Value = string.Join(", ", c.Groups.Select(g => g.Name));
            ws.Cell(rowIdx, 5).Value = c.ImageUrl;
            rowIdx++;
        }

        ws.Columns().AdjustToContents(); // Автопідбір ширини колонок
        workbook.SaveAs(stream);
    }
}