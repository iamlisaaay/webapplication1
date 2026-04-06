using Concert.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Concert.Services;

public interface IImportService<TEntity> where TEntity : class
{
  
    Task<List<ImportError>> ImportFromStreamAsync(Stream stream, CancellationToken ct);
}