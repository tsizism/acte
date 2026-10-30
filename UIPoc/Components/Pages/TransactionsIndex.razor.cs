using Microsoft.AspNetCore.Components;
using Radzen;
using UIPooc.Models;
using UIPooc.Services;

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

    private void GoBack()
    {
        NavigationManager.NavigateTo("/holdings");
    }
}
