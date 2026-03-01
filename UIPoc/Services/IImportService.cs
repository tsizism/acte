using UIPooc.Models;

namespace UIPooc.Services
{
    public interface IImportService
    {
        Task<ImportResult> ImportEquitiesFromExcelAsync(Stream excelStream, int holdingId);
        Task<ImportResult> ImportEquitiesFromExcelByHoldingNameAsync(Stream excelStream, string holdingName, int userId);
        Task<ImportResult> ImportTransactionsFromExcelAsync(Stream excelStream, int holdingId);
        Task<ImportResult> ImportIndexHistoryFromExcelAsync(Stream excelStream, int holdingId);
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? Message { get; set; }
    }
}
