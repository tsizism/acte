using Microsoft.AspNetCore.Components;
using Radzen;
using UIPooc.Models;
using UIPooc.Services;

namespace UIPooc.Components.Pages;

public partial class HoldingsIndex
{
    [Inject]
    private IModelService ModelService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    private List<Holding> _holdings = [];
    private IList<Holding>? _selectedHoldings;
    private bool _isLoading;
    private int _totalEquities;
    private double _averageIndex;

    protected override async Task OnInitializedAsync()
    {
        await LoadHoldingsAsync();
    }

    private async Task LoadHoldingsAsync()
    {
        _isLoading = true;
        try
        {
            _holdings = await ModelService.GetAllHoldingsAsync();
            CalculateSummary();
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = $"Failed to load holdings: {ex.Message}",
                Duration = 4000
            });
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void CalculateSummary()
    {
        _totalEquities = _holdings.Sum(h => h.Equities.Count);
        _averageIndex = _holdings.Count > 0 ? _holdings.Average(h => h.Index) : 0;
    }

    private void ViewEquities(int holdingId)
    {
        NavigationManager.NavigateTo($"/equities/{holdingId}");
    }

    private static BadgeStyle GetHoldingTypeBadgeStyle(HoldingType type) => type switch
    {
        HoldingType.Active => BadgeStyle.Success,
        HoldingType.WatchList => BadgeStyle.Info,
        HoldingType.Listless => BadgeStyle.Warning,
        HoldingType.CustomIndex => BadgeStyle.Primary,
        _ => BadgeStyle.Light
    };
}
