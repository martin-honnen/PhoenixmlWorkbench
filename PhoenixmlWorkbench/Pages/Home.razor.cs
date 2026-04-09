using Microsoft.AspNetCore.Components;
using System.Runtime.Versioning;

namespace Pages;

[SupportedOSPlatform("browser")]
public partial class Home : ComponentBase
{
    private string result = string.Empty;
    private string? code;
    private string? xml;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await Client.InitClient();
    }

    private async Task ExecuteXQuery()
    {
        try
        {
            if (code is not null && xml is not null)
            {
                result = await Client.ExecuteXQuery(code, xml);
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
        }

        await InvokeAsync(StateHasChanged);
    }
}