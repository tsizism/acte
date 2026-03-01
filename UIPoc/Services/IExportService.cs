namespace UIPooc.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportHoldingToExcelAsync(int holdingId);
        Task<byte[]> ExportAllHoldingsToExcelAsync(int userId);
        Task<byte[]> ExportEquitiesToExcelAsync(int holdingId);
        Task<byte[]> ExportTransactionsToExcelAsync(int holdingId);
        Task<byte[]> ExportIndexHistoryToExcelAsync(int holdingId);
    }
}
