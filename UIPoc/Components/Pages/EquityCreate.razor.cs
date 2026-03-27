using Microsoft.AspNetCore.Components;
using Radzen;
using UIPooc.Models;
using UIPooc.Services;

namespace UIPooc.Components.Pages;

public partial class EquityCreate
{
    [Parameter]
    public int HoldingId { get; set; }

    [Inject]
    private IModelService ModelService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    private Equity _equity = new();
    private Holding? _holding;
    private bool _isSaving;

    private readonly List<object> _transactionTypes = Enum.GetValues<TransactionType>()
        .Select(t => (object)new { Text = t.ToString(), Value = t })
        .ToList();

    protected override async Task OnInitializedAsync()
    {
        _equity.HoldingId = HoldingId;
        _equity.LastTxnAt = DateTime.UtcNow;

        try
        {
            _holding = await ModelService.GetHoldingByIdAsync(HoldingId);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = $"Failed to load holding: {ex.Message}",
                Duration = 4000
            });
        }
    }

    private async Task OnSubmit()
    {
        _isSaving = true;
        try
        {
            _equity.LastTxnAt = DateTime.UtcNow;

            await ModelService.CreateEquityAsync(_equity);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Success",
                Detail = $"Equity '{_equity.Symbol}' added successfully.",
                Duration = 4000
            });

            NavigationManager.NavigateTo($"/equities/{HoldingId}");
        }
        catch (InvalidOperationException ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "InvalidOperationException",
                Detail = $"Failed to create equity: {ex.Message}",
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
                Detail = $"Failed to create equity: {ex.Message}",
                Duration = 4000
            });
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void GoBack()
    {
        NavigationManager.NavigateTo($"/equities/{HoldingId}");
    }
}
