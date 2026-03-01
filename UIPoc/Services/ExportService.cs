using OfficeOpenXml;
using OfficeOpenXml.Style;
using UIPooc.Models;

namespace UIPooc.Services
{
    public class ExportService : IExportService
    {
        private readonly IModelService _modelService;

        public ExportService(IModelService modelService)
        {
            _modelService = modelService;
        }

        public async Task<byte[]> ExportHoldingToExcelAsync(int holdingId)
        {
            var holding = await _modelService.GetHoldingByIdAsync(holdingId);
            if (holding == null)
                throw new InvalidOperationException($"Holding with ID {holdingId} not found.");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            
            // Create Summary worksheet
            var summarySheet = package.Workbook.Worksheets.Add("Summary");
            AddHoldingSummary(summarySheet, holding);

            // Create Equities worksheet
            var equitiesSheet = package.Workbook.Worksheets.Add("Equities");
            await AddEquitiesData(equitiesSheet, holdingId);

            // Create Transactions worksheet
            var transactionsSheet = package.Workbook.Worksheets.Add("Transactions");
            await AddTransactionsData(transactionsSheet, holdingId);

            // Create Index History worksheet
            var indexHistorySheet = package.Workbook.Worksheets.Add("Index History");
            await AddIndexHistoryData(indexHistorySheet, holdingId);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportAllHoldingsToExcelAsync(int userId)
        {
            var holdings = await _modelService.GetHoldingsByUserIdAsync(userId);
            if (holdings.Count == 0)
                throw new InvalidOperationException($"No holdings found for user ID {userId}.");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("All Holdings");

            // Add headers
            worksheet.Cells[1, 1].Value = "Holding Name";
            worksheet.Cells[1, 2].Value = "Call Name";
            worksheet.Cells[1, 3].Value = "Index";
            worksheet.Cells[1, 4].Value = "Currency";
            worksheet.Cells[1, 5].Value = "Flag Max Index";
            worksheet.Cells[1, 6].Value = "Flag Min Index";
            worksheet.Cells[1, 7].Value = "Equity Count";
            worksheet.Cells[1, 8].Value = "Total Value";
            worksheet.Cells[1, 9].Value = "Created At";
            worksheet.Cells[1, 10].Value = "Last Updated";

            StyleHeader(worksheet, 1, 10);

            int row = 2;
            foreach (var holding in holdings)
            {
                var totalValue = await _modelService.GetTotalPortfolioValueAsync(holding.HoldingId);
                var equityCount = await _modelService.GetEquityCountAsync(holding.HoldingId);

                worksheet.Cells[row, 1].Value = holding.Name;
                worksheet.Cells[row, 2].Value = holding.CallName;
                worksheet.Cells[row, 3].Value = holding.Index;
                worksheet.Cells[row, 4].Value = holding.Currency;
                worksheet.Cells[row, 5].Value = holding.FlagMaxIndex;
                worksheet.Cells[row, 6].Value = holding.FlagMinIndex;
                worksheet.Cells[row, 7].Value = equityCount;
                worksheet.Cells[row, 8].Value = totalValue;
                worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 9].Value = holding.CreatedAt;
                worksheet.Cells[row, 9].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                worksheet.Cells[row, 10].Value = holding.LastUpdated;
                worksheet.Cells[row, 10].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportEquitiesToExcelAsync(int holdingId)
        {
            var equities = await _modelService.GetEquitiesByHoldingIdAsync(holdingId);
            if (equities.Count == 0)
                throw new InvalidOperationException($"No equities found for holding ID {holdingId}.");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Equities");

            await AddEquitiesData(worksheet, holdingId);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportTransactionsToExcelAsync(int holdingId)
        {
            var transactions = await _modelService.GetTransactionsByHoldingIdAsync(holdingId);
            if (transactions.Count == 0)
                throw new InvalidOperationException($"No transactions found for holding ID {holdingId}.");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Transactions");

            await AddTransactionsData(worksheet, holdingId);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportIndexHistoryToExcelAsync(int holdingId)
        {
            var indexHistories = await _modelService.GetIndexHistoriesByHoldingIdAsync(holdingId);
            if (indexHistories.Count == 0)
                throw new InvalidOperationException($"No index history found for holding ID {holdingId}.");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Index History");

            await AddIndexHistoryData(worksheet, holdingId);

            return package.GetAsByteArray();
        }

        #region Private Helper Methods

        private void AddHoldingSummary(ExcelWorksheet worksheet, Holding holding)
        {
            worksheet.Cells[1, 1].Value = "Holding Information";
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;

            int row = 3;
            worksheet.Cells[row, 1].Value = "Name:";
            worksheet.Cells[row, 2].Value = holding.Name;
            row++;

            worksheet.Cells[row, 1].Value = "Call Name:";
            worksheet.Cells[row, 2].Value = holding.CallName;
            row++;

            worksheet.Cells[row, 1].Value = "Index:";
            worksheet.Cells[row, 2].Value = holding.Index;
            row++;

            worksheet.Cells[row, 1].Value = "Currency:";
            worksheet.Cells[row, 2].Value = holding.Currency;
            row++;

            worksheet.Cells[row, 1].Value = "Flag Max Index:";
            worksheet.Cells[row, 2].Value = holding.FlagMaxIndex;
            row++;

            worksheet.Cells[row, 1].Value = "Flag Min Index:";
            worksheet.Cells[row, 2].Value = holding.FlagMinIndex;
            row++;

            worksheet.Cells[row, 1].Value = "Created At:";
            worksheet.Cells[row, 2].Value = holding.CreatedAt;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
            row++;

            worksheet.Cells[row, 1].Value = "Last Updated:";
            worksheet.Cells[row, 2].Value = holding.LastUpdated;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

            worksheet.Column(1).Style.Font.Bold = true;
            worksheet.Cells[3, 1, row, 2].AutoFitColumns();
        }

        private async Task AddEquitiesData(ExcelWorksheet worksheet, int holdingId)
        {
            var equities = await _modelService.GetEquitiesByHoldingIdAsync(holdingId);

            // Add headers
            worksheet.Cells[1, 1].Value = "Symbol";
            worksheet.Cells[1, 2].Value = "Company Name";
            worksheet.Cells[1, 3].Value = "Market";
            worksheet.Cells[1, 4].Value = "Currency";
            worksheet.Cells[1, 5].Value = "Quantity";
            worksheet.Cells[1, 6].Value = "Average Cost";
            worksheet.Cells[1, 7].Value = "Current Price";
            worksheet.Cells[1, 8].Value = "Total Value";
            worksheet.Cells[1, 9].Value = "Gain/Loss";
            worksheet.Cells[1, 10].Value = "Gain/Loss %";
            worksheet.Cells[1, 11].Value = "Last Txn Type";
            worksheet.Cells[1, 12].Value = "Last Txn Price";
            worksheet.Cells[1, 13].Value = "Last Txn Quantity";
            worksheet.Cells[1, 14].Value = "Last Txn At";
            worksheet.Cells[1, 15].Value = "Holding High";
            worksheet.Cells[1, 16].Value = "Holding High At";
            worksheet.Cells[1, 17].Value = "Holding Low";
            worksheet.Cells[1, 18].Value = "Holding Low At";
            worksheet.Cells[1, 19].Value = "Flag Max";
            worksheet.Cells[1, 20].Value = "Flag Min";

            StyleHeader(worksheet, 1, 20);

            int row = 2;
            foreach (var equity in equities)
            {
                decimal totalValue = equity.Quantity * equity.CurrentPrice;
                decimal gainLoss = totalValue - (equity.Quantity * equity.AverageCost);
                decimal gainLossPercent = equity.AverageCost != 0 
                    ? (gainLoss / (equity.Quantity * equity.AverageCost)) * 100 
                    : 0;

                worksheet.Cells[row, 1].Value = equity.Symbol;
                worksheet.Cells[row, 2].Value = equity.CompanyName;
                worksheet.Cells[row, 3].Value = equity.Market;
                worksheet.Cells[row, 4].Value = equity.Currency;
                worksheet.Cells[row, 5].Value = equity.Quantity;
                worksheet.Cells[row, 6].Value = equity.AverageCost;
                worksheet.Cells[row, 7].Value = equity.CurrentPrice;
                worksheet.Cells[row, 8].Value = totalValue;
                worksheet.Cells[row, 9].Value = gainLoss;
                worksheet.Cells[row, 10].Value = gainLossPercent;
                worksheet.Cells[row, 11].Value = equity.LastTxnType.ToString();
                worksheet.Cells[row, 12].Value = equity.LastTxnPrice;
                worksheet.Cells[row, 13].Value = equity.LastTxnQuantity;
                worksheet.Cells[row, 14].Value = equity.LastTxnAt;
                worksheet.Cells[row, 14].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                worksheet.Cells[row, 15].Value = equity.HoldingHigh;
                worksheet.Cells[row, 16].Value = equity.HoldingHighAt;
                worksheet.Cells[row, 16].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                worksheet.Cells[row, 17].Value = equity.HoldingLow;
                worksheet.Cells[row, 18].Value = equity.HoldingLowAt;
                worksheet.Cells[row, 18].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                worksheet.Cells[row, 19].Value = equity.FlagMax;
                worksheet.Cells[row, 20].Value = equity.FlagMin;

                // Format currency columns
                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 9].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 10].Style.Numberformat.Format = "0.00%";
                worksheet.Cells[row, 12].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 15].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 17].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 19].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 20].Style.Numberformat.Format = "#,##0.00";

                // Color code gain/loss
                if (gainLoss > 0)
                    worksheet.Cells[row, 9].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                else if (gainLoss < 0)
                    worksheet.Cells[row, 9].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private async Task AddTransactionsData(ExcelWorksheet worksheet, int holdingId)
        {
            var transactions = await _modelService.GetTransactionsByHoldingIdAsync(holdingId);

            // Add headers
            worksheet.Cells[1, 1].Value = "Transaction Date";
            worksheet.Cells[1, 2].Value = "Symbol";
            worksheet.Cells[1, 3].Value = "Type";
            worksheet.Cells[1, 4].Value = "Quantity";
            worksheet.Cells[1, 5].Value = "Price";
            worksheet.Cells[1, 6].Value = "Total Amount";
            worksheet.Cells[1, 7].Value = "Commission";
            worksheet.Cells[1, 8].Value = "Notes";

            StyleHeader(worksheet, 1, 8);

            int row = 2;
            foreach (var transaction in transactions)
            {
                worksheet.Cells[row, 1].Value = transaction.TransactionDate;
                worksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                worksheet.Cells[row, 2].Value = transaction.Symbol;
                worksheet.Cells[row, 3].Value = transaction.Type.ToString();
                worksheet.Cells[row, 4].Value = transaction.Quantity;
                worksheet.Cells[row, 5].Value = transaction.Price;
                worksheet.Cells[row, 6].Value = transaction.TotalAmount;
                worksheet.Cells[row, 7].Value = transaction.Commission;
                worksheet.Cells[row, 8].Value = transaction.Notes;

                // Format currency columns
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";

                // Color code transaction type
                if (transaction.Type == TransactionType.Buy)
                    worksheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                else if (transaction.Type == TransactionType.Sell)
                    worksheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private async Task AddIndexHistoryData(ExcelWorksheet worksheet, int holdingId)
        {
            var indexHistories = await _modelService.GetIndexHistoriesByHoldingIdAsync(holdingId);

            // Add headers
            worksheet.Cells[1, 1].Value = "Recorded At";
            worksheet.Cells[1, 2].Value = "Index";
            worksheet.Cells[1, 3].Value = "Change";
            worksheet.Cells[1, 4].Value = "Change %";

            StyleHeader(worksheet, 1, 4);

            int row = 2;
            double? previousIndex = null;

            foreach (var history in indexHistories.OrderBy(h => h.RecordedAt))
            {
                double? change = previousIndex.HasValue ? history.Index - previousIndex.Value : null;
                double? changePercent = previousIndex.HasValue && previousIndex.Value != 0 
                    ? ((history.Index - previousIndex.Value) / previousIndex.Value) * 100 
                    : null;

                worksheet.Cells[row, 1].Value = history.RecordedAt;
                worksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                worksheet.Cells[row, 2].Value = history.Index;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                
                if (change.HasValue)
                {
                    worksheet.Cells[row, 3].Value = change.Value;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                    
                    if (change.Value > 0)
                        worksheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                    else if (change.Value < 0)
                        worksheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                }

                if (changePercent.HasValue)
                {
                    worksheet.Cells[row, 4].Value = changePercent.Value;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "0.00%";
                }

                previousIndex = history.Index;
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void StyleHeader(ExcelWorksheet worksheet, int startRow, int columnCount)
        {
            using var range = worksheet.Cells[startRow, 1, startRow, columnCount];
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
        }

        #endregion
    }
}
