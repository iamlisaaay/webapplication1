using Concert.Models;
namespace Concert.Services;

public class ConcertDataPortServiceFactory : IDataPortServiceFactory<Concert.Models.Concert>
{
    private readonly ConcertContext _context;
    public ConcertDataPortServiceFactory(ConcertContext context) => _context = context;

    public IImportService<Concert.Models.Concert> GetImportService(string type) => new ConcertImportService(_context);
    public IExportService<Concert.Models.Concert> GetExportService(string type) => new ConcertExportService(_context);
}