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
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            int importedCount = 0;

            // Assuming the Excel has headers in row 1 and data starts from row 2
            // Expected columns: Symbol, CompanyName, Quantity, AverageCost, CurrentPrice
            for (int row = 2; row <= rowCount; row++)
            {
                string? assetType = worksheet.Cells[row, 1].Text?.Trim();

                if( assetType == null || !assetType.Equals("Equities", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Skip header or non-equity rows
                }

                //if (string.IsNullOrWhiteSpace(symbol))
                //    continue;

                Equity equity = new Equity
                {
                    HoldingId = holdingId,
                    Currency = worksheet.Cells[row, 2].Text?.Trim() ?? throw new InvalidDataException("Currency is empty"),
                    Symbol = worksheet.Cells[row, 3].Text?.Trim() ?? throw new InvalidDataException("Symbol is empty"),
                    Market = worksheet.Cells[row, 4].Text?.Trim() ?? throw new InvalidDataException("Market is empty"),
                    CompanyName = worksheet.Cells[row, 5].Text?.Trim(),
                    Quantity = ParseDecimal(worksheet.Cells[row, 6].Text),
                    AverageCost = ParseDecimal(worksheet.Cells[row, 7].Text), 
                    CurrentPrice = ParseDecimal(worksheet.Cells[row, 8].Text), // Closing price
                    LastTxnType = TransactionType.Buy,
                    //LastTxnAt = DateTime.UtcNow
                };


                equity.LastTxnQuantity = equity.Quantity;
                equity.LastTxnPrice = equity.CurrentPrice;
                equity.HoldingHigh = equity.LastTxnPrice;
                equity.HoldingHighAt = equity.LastTxnAt;
                equity.HoldingLow = equity.LastTxnPrice;
                equity.HoldingLowAt = equity.LastTxnAt;

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
