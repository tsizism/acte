using Microsoft.AspNetCore.Components;
using Radzen;
using UIPooc.Components.Dialogs;
using UIPooc.Models;
using UIPooc.Services;
using UIPooc.Utils;

namespace UIPooc.Components.Pages;

public partial class TransactionsIndex
{
    [Parameter]
    public int HoldingId { get; set; }

    [SupplyParameterFromQuery(Name = "symbol")]
    public string? Symbol { get; set; }

    [Inject]
    private IModelService ModelService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private DialogService DialogService { get; set; } = default!;

    private Holding? _holding;
    private List<Transaction> _transactions = [];
    private bool _isLoading;

    protected override async Task OnParametersSetAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            _holding = await ModelService.GetHoldingByIdAsync(HoldingId);
            _transactions = await ModelService.GetTransactionsByHoldingIdAsync(HoldingId);
            if (!string.IsNullOrWhiteSpace(Symbol))
            {
                _transactions = _transactions
                    .Where(t => string.Equals(t.Symbol, Symbol, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = $"Failed to load transactions: {ex.Message}",
                Duration = 4000
            });
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task NewTransactionAsync()
    {
        var parameters = new Dictionary<string, object?>
        {
            { "HoldingId", HoldingId },
            { "Symbol", Symbol },
            { "HoldingCurrency", _holding?.Currency }
        };

        var result = await DialogService.OpenAsync<TransactionDialog>(
            "New Transaction",
            parameters!,
            new DialogOptions { Width = "450px", Resizable = false, Draggable = false });

        if (result is not Transaction transaction)
        {
            return;
        }

        try
        {
            await ModelService.CreateTransactionAsync(transaction);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Success",
                Detail = $"Transaction for '{transaction.Symbol}' added.",
                Duration = 5000
            });


            Equity? equity = await ModelService.GetEquity(HoldingId, transaction.Symbol, transaction.Market);

            if (equity != null)
            {
                // $170.00 USD / $237.93 CAD
                //6.95    1.4411
                //$23,932.28 = (16600 + 6.95) * 1.4411 = 23,932.275645/
                //16600
                //$16,606.95
                // Avg $238.625 CAD	  $47,725.00 CAD Book 
                // $137.72 USD / $191.29 CAD

                equity.Quantity += transaction.Quantity;
                equity.AverageCost = EquityUtils.CalculateNewAverageCost(equity.AverageCost, equity.Quantity, transaction.Price, transaction.Quantity);
                equity.AverageCostMarket = EquityUtils.CalculateNewAverageCost(equity.AverageCostMarket, equity.Quantity, transaction.MarketPrice, transaction.Quantity);
                await ModelService.UpdateEquityAsync(equity);
                var marketSymbol = EquityUtils.GetSymbolAdjustedToMarket(equity);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = $"Equity for '{marketSymbol}' updated.",
                    Duration = 3000
                });
            }
            else
            {
                Equity newEquity = EquityUtils.CreateEquityFromTransaction(transaction);
                await ModelService.CreateEquityAsync(newEquity);

                var marketSymbol = EquityUtils.GetSymbolAdjustedToMarket(newEquity);

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = $"Equity for '{marketSymbol}' added.",
                    Duration = 3000
                });

            }


            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = $"Failed to add transaction: {ex.Message}",
                Duration = 4000
            });
        }
    }

    private void GoBack()
    {
        NavigationManager.NavigateTo("/holdings");
    }
}
