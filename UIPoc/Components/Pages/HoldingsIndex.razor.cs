using Microsoft.AspNetCore.Components;
using Radzen;
using UIPooc.Models;
using UIPooc.Services;
using UIPooc.Components.Dialogs;

namespace UIPooc.Components.Pages;

public partial class HoldingsIndex
{
    [Inject]
    private IModelService ModelService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private DialogService DialogService { get; set; } = default!;

    private List<Holding> _holdings = [];
    private IList<Holding>? _selectedHoldings;
    private bool _isLoading;
    private int _totalEquities;
    private decimal _averageIndex;
    private string _renameInput = string.Empty;

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
        catch (InvalidOperationException ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "InvalidOperationException",
                Detail = $"Failed to load holdings: {ex.Message}",
                Duration = 2000,
                CloseOnClick = true
            });
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

    private void ViewHoldingHistory(int holdingId)
    {
        NavigationManager.NavigateTo($"/holdings/{holdingId}/history");
    }

    private void CreateNewHolding()
    {
        NavigationManager.NavigateTo("/holdings/create");
    }

    private async Task CloneHolding(Holding holding)
    {
        try
        {
            var clonedHoldingName = $"{holding.Name}-Copy";
            var counter = 1;

            // Check if name already exists and increment counter
            while (_holdings.Any(h => h.Name == clonedHoldingName))
            {
                counter++;
                clonedHoldingName = $"{holding.Name} - Copy ({counter})";
            }

            var clonedHolding = await ModelService.CloneHoldingAsync(holding.HoldingId, clonedHoldingName, cloneEquities: true);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Success",
                Detail = $"Holding '{holding.Name}' cloned as '{clonedHoldingName}' with {clonedHolding.Equities.Count} equities.",
                Duration = 4000
            });

            await LoadHoldingsAsync();
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = $"Failed to clone holding: {ex.Message}",
                Duration = 4000
            });
        }
    }

    private async Task RenameHolding(Holding holding)
    {
        _renameInput = holding.Name;

        var parameters = new Dictionary<string, object>
        {
            { "HoldingName", holding.Name }
        };

        var result = await DialogService.OpenAsync<RenameHoldingDialog>(
            "Rename Holding", 
            parameters!, 
            new DialogOptions { Width = "400px", Resizable = false, Draggable = false });

        if (result == null)
        {
            return; // User cancelled
        }

        var newName = result.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(newName))
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "Warning",
                Detail = "Holding name cannot be empty.",
                Duration = 4000
            });
            return;
        }

        // Check if name already exists
        if (_holdings.Any(h => h.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && h.HoldingId != holding.HoldingId))
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "Warning",
                Detail = $"A holding with the name '{newName}' already exists.",
                Duration = 4000
            });
            return;
        }

        try
        {
            var originalHolding = _holdings.First(h => h.HoldingId == holding.HoldingId);
            var oldName = originalHolding.Name;
            originalHolding.Name = newName;
            await ModelService.UpdateHoldingAsync(originalHolding);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Success",
                Detail = $"Holding renamed from '{oldName}' to '{newName}'.",
                Duration = 4000
            });

            await LoadHoldingsAsync();
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = $"Failed to rename holding: {ex.Message}",
                Duration = 4000
            });
        }
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
