using Microsoft.AspNetCore.Components;
using Radzen;
using UIPooc.Models;
using UIPooc.Services;

namespace UIPooc.Components.Pages;

public partial class HoldingIndexHistory
{
    [Parameter]
    public int HoldingId { get; set; }

    [Inject]
    private IModelService ModelService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    private Holding? _holding;
    private List<IndexHistory> _historyEntries = [];
    private bool _isLoading;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            _holding = await ModelService.GetHoldingByIdAsync(HoldingId);
            _historyEntries = await ModelService.GetIndexHistoriesByHoldingIdAsync(HoldingId);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = $"Failed to load holding history: {ex.Message}",
                Duration = 4000
            });
        }
        finally
        {
            _isLoading = false;
        }
    }

    private decimal CalculateChange(IndexHistory current)
    {
        var currentIndex = _historyEntries.IndexOf(current);
        if (currentIndex < _historyEntries.Count - 1)
        {
            var previous = _historyEntries[currentIndex + 1];
            return current.Index - previous.Index;
        }
        return 0;
    }

    private decimal CalculateChangePercent(IndexHistory current)
    {
        var currentIndex = _historyEntries.IndexOf(current);
        if (currentIndex < _historyEntries.Count - 1)
        {
            var previous = _historyEntries[currentIndex + 1];
            if (previous.Index != 0)
            {
                return (current.Index - previous.Index) / previous.Index;
            }
        }
        return 0;
    }

    private void GoBack()
    {
        NavigationManager.NavigateTo("/holdings");
    }

    private static BadgeStyle GetHoldingTypeBadgeStyle(HoldingType type) => type switch
    {
        HoldingType.Active => BadgeStyle.Success,
        HoldingType.WatchList => BadgeStyle.Info,
        HoldingType.Sold => BadgeStyle.Warning,
        HoldingType.BuyPending => BadgeStyle.Primary,
        _ => BadgeStyle.Light
    };
}
