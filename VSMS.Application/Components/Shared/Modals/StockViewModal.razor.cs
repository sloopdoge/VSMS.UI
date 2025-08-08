using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Domain;
using VSMS.Domain.Enums;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Shared.Modals;

public partial class StockViewModal : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; }

    [Inject] private ILogger<StockViewModal> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private StocksHttpService StocksHttpService { get; set; }

    [Parameter] public Guid? CompanyId { get; set; }
    [Parameter] public Guid? StockId { get; set; }
    [Parameter] public ModalModeEnum ModalMode { get; set; } = ModalModeEnum.View;
    
    private StockViewModel Model { get; set; } = new();
    private EditContext _editContext = new(new StockViewModel());

    protected override async Task OnInitializedAsync()
    {
        try
        {
            switch (ModalMode)
            {
                case ModalModeEnum.Edit:
                    if (StockId.HasValue)
                    {
                        var res = await StocksHttpService.GetStockById(StockId.Value);
                        if (res is not null)
                            Model = res;
                    }
                    break;
                case ModalModeEnum.Create:
                    Model.CompanyId = CompanyId;
                    break;
                default:
                    if (StockId.HasValue)
                    {
                        var res = await StocksHttpService.GetStockById(StockId.Value);
                        if (res is not null)
                            Model = res;
                    }
                    break;
            }
            
            _editContext = new EditContext(Model);
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            MudDialog.Cancel();
        }
    }

    private async Task HandleSubmit()
    {
        try
        {
            StockViewModel? resultedModel = null;
            
            switch (ModalMode)
            {
                case ModalModeEnum.Edit:
                    resultedModel = await StocksHttpService.UpdateStock(Model);
                    break;
                case ModalModeEnum.Create:
                    resultedModel = await StocksHttpService.CreateStock(Model);
                    break;
            }
            
            if (resultedModel is not null)
                MudDialog.Close(DialogResult.Ok(resultedModel));
            else
            {
                var errorLog = $"Stock {Model.Symbol}";
                switch (ModalMode)
                {
                    case ModalModeEnum.Create:
                        errorLog += "was not created";
                        break;
                    case ModalModeEnum.Edit:
                        errorLog += "was not updated";
                        break;
                }
                
                Logger.LogError(errorLog);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
}