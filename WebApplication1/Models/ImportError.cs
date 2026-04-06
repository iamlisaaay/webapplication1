namespace Concert.Models;

public class ImportError
{
    public int RowNumber { get; set; }
    public string ConcertTitle { get; set; }
    public string ErrorMessage { get; set; }
}