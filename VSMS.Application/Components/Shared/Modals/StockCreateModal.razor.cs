using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Shared.Modals;

public partial class StockCreateModal : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; }

    [Inject] private ILogger<StockCreateModal> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private StocksHttpService StocksHttpService { get; set; }

    [Parameter] public Guid CompanyId { get; set; }
    
    private StockViewModel CreateModel { get; set; } = new();
    private EditContext _editContext;

    protected override void OnInitialized()
    {
        try
        {
            CreateModel.CompanyId = CompanyId;
            _editContext = new(CreateModel);
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            MudDialog.Cancel();
        }
    }

    private async Task HandleAddStock()
    {
        try
        {
            var res = await StocksHttpService.CreateStock(CreateModel);
            if (res is not null)
                MudDialog.Close(DialogResult.Ok(res));
            else
                Logger.LogError($"Stock {CreateModel.Symbol} not added");
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
}