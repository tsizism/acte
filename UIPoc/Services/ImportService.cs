using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using UIPooc.Models;

namespace UIPooc.Services
{
    public class ImportService : IImportService
    {
        private readonly IModelService _modelService;

        public ImportService(IModelService modelService)
        {
            _modelService = modelService;
        }

        //public async Task<int> ImportEquitiesFromExcelAsync2(Stream excelStream, int holdingId)
        //{
        //    //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    //ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

        //    using var package = new ExcelPackage(excelStream);
        //    var worksheet = package.Workbook.Worksheets[0];

        //    int rowCount = worksheet.Dimension?.Rows ?? 0;
        //    int importedCount = 0;

        //    // Assuming the Excel has headers in row 1 and data starts from row 2
        //    // Expected columns: Symbol, CompanyName, Quantity, AverageCost, CurrentPrice
        //    for (int row = 2; row <= rowCount; row++)
        //    {
        //        string? assetType = worksheet.Cells[row, 1].Text?.Trim();

        //        if (assetType == null || !assetType.Equals("Equities", StringComparison.OrdinalIgnoreCase))
        //        {
        //            continue; // Skip header or non-equity rows
        //        }

        //        //if (string.IsNullOrWhiteSpace(symbol))
        //        //    continue;

        //        Equity equity = new Equity
        //        {
        //            HoldingId = holdingId,
        //            Currency = worksheet.Cells[row, 2].Text?.Trim() ?? throw new InvalidDataException("Currency is empty"),
        //            Symbol = worksheet.Cells[row, 3].Text?.Trim() ?? throw new InvalidDataException("Symbol is empty"),
        //            Market = worksheet.Cells[row, 4].Text?.Trim() ?? throw new InvalidDataException("Market is empty"),
        //            CompanyName = worksheet.Cells[row, 5].Text?.Trim(),
        //            Quantity = ParseDecimal(worksheet.Cells[row, 6].Text),
        //            AverageCost = ParseDecimal(worksheet.Cells[row, 7].Text),
        //            CurrentPrice = ParseDecimal(worksheet.Cells[row, 8].Text), // Closing price
        //            LastTxnType = TransactionType.Buy,
        //            LastTxnAt = DateTime.UtcNow
        //        };


        //        equity.LastTxnQuantity = equity.Quantity;
        //        equity.LastTxnPrice = equity.CurrentPrice;
        //        equity.HoldingHigh = equity.LastTxnPrice;
        //        equity.HoldingHighAt = equity.LastTxnAt;
        //        equity.HoldingLow = equity.LastTxnPrice;
        //        equity.HoldingLowAt = equity.LastTxnAt;

        //        //_context.Equities.Add(equity);
        //        importedCount++;
        //    }

        //    //await _context.SaveChangesAsync();
        //    return importedCount;
        //}


        public async Task<ImportResult> ImportEquitiesFromExcelAsync(Stream excelStream, int holdingId)
        {
            ImportResult result = new ImportResult();

            try
            {
                // Verify holding exists
                var holding = await _modelService.GetHoldingByIdAsync(holdingId);
                if (holding == null)
                {
                    result.Success = false;
                    result.Message = $"Holding with ID {holdingId} not found.";
                    result.Errors.Add(result.Message);
                    return result;
                }

                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage(excelStream);
                var worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension?.Rows ?? 0;

                // Expected columns: AssetType, Currency, Symbol, Market, CompanyName, Quantity, AverageCost, CurrentPrice
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        string? assetType = worksheet.Cells[row, 1].Text?.Trim();

                        if (string.IsNullOrWhiteSpace(assetType) || 
                            !assetType.Equals("Equities", StringComparison.OrdinalIgnoreCase))
                        {
                            result.SkippedCount++;
                            continue;
                        }

                        var currency = worksheet.Cells[row, 2].Text?.Trim();
                        var symbol = worksheet.Cells[row, 3].Text?.Trim();
                        var market = worksheet.Cells[row, 4].Text?.Trim();

                        if (string.IsNullOrWhiteSpace(currency))
                        {
                            result.Errors.Add($"Row {row}: Currency is required");
                            result.ErrorCount++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(symbol))
                        {
                            result.Errors.Add($"Row {row}: Symbol is required");
                            result.ErrorCount++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(market))
                        {
                            result.Errors.Add($"Row {row}: Market is required");
                            result.ErrorCount++;
                            continue;
                        }

                        // Check if equity already exists for this holding
                        var existingEquities = await _modelService.GetEquitiesByHoldingIdAsync(holdingId);
                        var existingEquity = existingEquities.FirstOrDefault(e => 
                            e.Symbol == symbol && e.Market == market);

                        if (existingEquity != null)
                        {
                            result.Warnings.Add($"Row {row}: Equity {symbol} already exists in holding. Skipping.");
                            result.SkippedCount++;
                            continue;
                        }

                        var equity = new Equity
                        {
                            HoldingId = holdingId,
                            Currency = currency,
                            Symbol = symbol,
                            Market = market,
                            CompanyName = worksheet.Cells[row, 5].Text?.Trim(),
                            Quantity = ParseInt(worksheet.Cells[row, 6].Text),
                            AverageCost = ParseDecimal(worksheet.Cells[row, 7].Text),
                            CurrentPrice = ParseDecimal(worksheet.Cells[row, 8].Text),
                            LastTxnType = TransactionType.Buy,
                            //LastTxnAt = DateTime.UtcNow,
                            FlagMax = 0,
                            FlagMin = 0
                        };

                        equity.LastTxnQuantity = equity.Quantity;
                        equity.LastTxnPrice = equity.CurrentPrice;
                        equity.HoldingHigh = equity.CurrentPrice;
                        //equity.HoldingHighAt = equity.LastTxnAt;
                        equity.HoldingLow = equity.CurrentPrice;
                        //equity.HoldingLowAt = equity.LastTxnAt;

                        await _modelService.CreateEquityAsync(equity);
                        result.ImportedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {row}: {ex.Message}");
                        result.ErrorCount++;
                    }
                }

                result.Success = result.ImportedCount > 0;
                result.Message = $"Successfully imported {result.ImportedCount} equities. " +
                                $"Skipped: {result.SkippedCount}, Errors: {result.ErrorCount}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Import failed: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<ImportResult> ImportEquitiesFromExcelByHoldingNameAsync(Stream excelStream, string holdingName, int userId)
        {
            var result = new ImportResult();

            try
            {
                // Get or create holding
                var holding = await _modelService.GetHoldingByNameAsync(holdingName);
                
                if (holding == null)
                {
                    // Create new holding
                    holding = new Holding
                    {
                        Name = holdingName,
                        CallName = holdingName,
                        UserId = userId,
                        Index = 0,
                        FlagMaxIndex = 0,
                        FlagMinIndex = 0,
                        Currency = "USD"
                    };

                    holding = await _modelService.CreateHoldingAsync(holding);
                    result.Warnings.Add($"Created new holding: {holdingName}");
                }

                // Import equities for this holding
                return await ImportEquitiesFromExcelAsync(excelStream, holding.HoldingId);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Import failed: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<ImportResult> ImportTransactionsFromExcelAsync(Stream excelStream, int holdingId)
        {
            var result = new ImportResult();

            try
            {
                // Verify holding exists
                var holding = await _modelService.GetHoldingByIdAsync(holdingId);
                if (holding == null)
                {
                    result.Success = false;
                    result.Message = $"Holding with ID {holdingId} not found.";
                    result.Errors.Add(result.Message);
                    return result;
                }

                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage(excelStream);
                var worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension?.Rows ?? 0;

                // Expected columns: Date, Symbol, Type, Quantity, Price, TotalAmount, Commission, Notes
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var dateText = worksheet.Cells[row, 1].Text?.Trim();
                        var symbol = worksheet.Cells[row, 2].Text?.Trim();
                        var typeText = worksheet.Cells[row, 3].Text?.Trim();

                        if (string.IsNullOrWhiteSpace(symbol))
                        {
                            result.SkippedCount++;
                            continue;
                        }

                        if (!DateTime.TryParse(dateText, out DateTime transactionDate))
                        {
                            result.Errors.Add($"Row {row}: Invalid date format");
                            result.ErrorCount++;
                            continue;
                        }

                        if (!Enum.TryParse<TransactionType>(typeText, true, out TransactionType transactionType))
                        {
                            result.Errors.Add($"Row {row}: Invalid transaction type. Must be 'Buy' or 'Sell'");
                            result.ErrorCount++;
                            continue;
                        }

                        var transaction = new Transaction
                        {
                            HoldingId = holdingId,
                            UserId = holding.UserId,
                            Symbol = symbol,
                            Type = transactionType,
                            Quantity = ParseInt(worksheet.Cells[row, 4].Text),
                            Price = ParseDecimal(worksheet.Cells[row, 5].Text),
                            TotalAmount = ParseDecimal(worksheet.Cells[row, 6].Text),
                            Commission = ParseDecimal(worksheet.Cells[row, 7].Text),
                            Notes = worksheet.Cells[row, 8].Text?.Trim(),
                            TransactionDate = transactionDate
                        };

                        await _modelService.CreateTransactionAsync(transaction);
                        result.ImportedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {row}: {ex.Message}");
                        result.ErrorCount++;
                    }
                }

                result.Success = result.ImportedCount > 0;
                result.Message = $"Successfully imported {result.ImportedCount} transactions. " +
                                $"Skipped: {result.SkippedCount}, Errors: {result.ErrorCount}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Import failed: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<ImportResult> ImportIndexHistoryFromExcelAsync(Stream excelStream, int holdingId)
        {
            var result = new ImportResult();

            try
            {
                // Verify holding exists
                var holding = await _modelService.GetHoldingByIdAsync(holdingId);
                if (holding == null)
                {
                    result.Success = false;
                    result.Message = $"Holding with ID {holdingId} not found.";
                    result.Errors.Add(result.Message);
                    return result;
                }

                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage(excelStream);
                var worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension?.Rows ?? 0;

                // Expected columns: Date, Index, HoldingSnapshot (optional JSON)
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var dateText = worksheet.Cells[row, 1].Text?.Trim();
                        var indexText = worksheet.Cells[row, 2].Text?.Trim();

                        if (string.IsNullOrWhiteSpace(indexText))
                        {
                            result.SkippedCount++;
                            continue;
                        }

                        if (!DateTime.TryParse(dateText, out DateTime recordedAt))
                        {
                            result.Errors.Add($"Row {row}: Invalid date format");
                            result.ErrorCount++;
                            continue;
                        }

                        if (!double.TryParse(indexText, out double indexValue))
                        {
                            result.Errors.Add($"Row {row}: Invalid index value");
                            result.ErrorCount++;
                            continue;
                        }

                        // Check for duplicates
                        var existingHistories = await _modelService.GetIndexHistoriesByHoldingIdAsync(holdingId);
                        if (existingHistories.Any(h => h.RecordedAt.Date == recordedAt.Date))
                        {
                            result.Warnings.Add($"Row {row}: Index history for {recordedAt:yyyy-MM-dd} already exists. Skipping.");
                            result.SkippedCount++;
                            continue;
                        }

                        var indexHistory = new IndexHistory
                        {
                            HoldingId = holdingId,
                            Index = indexValue,
                            RecordedAt = recordedAt,
                            HoldingSnapshot = worksheet.Cells[row, 3].Text?.Trim()
                        };

                        await _modelService.CreateIndexHistoryAsync(indexHistory);
                        result.ImportedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {row}: {ex.Message}");
                        result.ErrorCount++;
                    }
                }

                result.Success = result.ImportedCount > 0;
                result.Message = $"Successfully imported {result.ImportedCount} index history records. " +
                                $"Skipped: {result.SkippedCount}, Errors: {result.ErrorCount}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Import failed: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value?.Trim(), out decimal result))
                return result;
            return 0;
        }

        private int ParseInt(string value)
        {
            if (int.TryParse(value?.Trim(), out int result))
                return result;
            return 0;
        }
    }
}
