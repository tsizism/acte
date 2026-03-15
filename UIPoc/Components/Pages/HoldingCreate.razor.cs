using Microsoft.AspNetCore.Components;
using Radzen;
using UIPooc.Models;
using UIPooc.Services;

namespace UIPooc.Components.Pages;

public partial class HoldingCreate
{
    [Inject]
    private IModelService ModelService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    private Holding _holding = new();
    private bool _isSaving;

    private readonly List<object> _holdingTypes = Enum.GetValues<HoldingType>()
        .Select(t => (object)new { Text = t.ToString(), Value = t })
        .ToList();

    protected override async Task OnInitializedAsync()
    {
    }

    private async Task OnSubmit()
    {
        _isSaving = true;
        try
        {
            var user = await ModelService.GetCurrentUserAsync();
            if (user is not null)
            {
                _holding.UserId = user.UserId;
            }

            _holding.CreatedAt = DateTime.UtcNow;
            _holding.LastUpdated = DateTime.UtcNow;

            await ModelService.CreateHoldingAsync(_holding);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Success",
                Detail = $"Holding '{_holding.Name}' created successfully.",
                Duration = 4000
            });

            NavigationManager.NavigateTo("/holdings");
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = $"Failed to create holding: {ex.Message}",
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
        NavigationManager.NavigateTo("/holdings");
    }
}
