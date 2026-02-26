using OfficeOpenXml;
using UIPooc.Data;
using UIPooc.Models;

namespace UIPooc.Services
{
    public interface IExcelImportService
    {
        Task<int> ImportEquitiesFromExcelAsync(Stream excelStream, int holdingId);
    }

    public class ExcelImportService : IExcelImportService
    {
        private readonly HoldingsDbContext _context;

        public ExcelImportService(HoldingsDbContext context)
        {
            _context = context;
        }

        public async Task<int> ImportEquitiesFromExcelAsync(Stream excelStream, int holdingId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            int importedCount = 0;

            // Assuming the Excel has headers in row 1 and data starts from row 2
            // Expected columns: Symbol, CompanyName, Quantity, AverageCost, CurrentPrice
            for (int row = 2; row <= rowCount; row++)
            {
                var symbol = worksheet.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrWhiteSpace(symbol))
                    continue;

                var equity = new Equity
                {
                    HoldingId = holdingId,
                    Symbol = symbol,
                    CompanyName = worksheet.Cells[row, 2].Text?.Trim(),
                    Quantity = ParseDecimal(worksheet.Cells[row, 3].Text),
                    AverageCost = ParseDecimal(worksheet.Cells[row, 4].Text),
                    CurrentPrice = ParseDecimal(worksheet.Cells[row, 5].Text),
                    LastTxnType = TransactionType.Buy,
                    LastTxnQuantity = ParseDecimal(worksheet.Cells[row, 3].Text),
                    LastTxnPrice = ParseDecimal(worksheet.Cells[row, 4].Text),
                    LastTxnAt = DateTime.UtcNow
                };

                _context.Equities.Add(equity);
                importedCount++;
            }

            await _context.SaveChangesAsync();
            return importedCount;
        }

        private decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value?.Trim(), out decimal result))
                return result;
            return 0;
        }
    }
}
