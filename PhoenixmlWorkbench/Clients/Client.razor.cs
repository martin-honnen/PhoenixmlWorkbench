using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

[SupportedOSPlatform("browser")]
public partial class Client
{
    private static bool _workerStarted;

    public static async Task InitClient()
    {
        if (_workerStarted)
        {
            return;
        }

        _workerStarted = true;

        await JSHost.ImportAsync(
            moduleName: nameof(Client),
            moduleUrl: "../Clients/Client.razor.js");
    }

    [JSImport("transform", nameof(Client))]
    public static partial Task<string> Transform(string xslt, string xml);

    [JSImport("executeXQuery", nameof(Client))]
    public static partial Task<string> ExecuteXQuery(string xquery, string xml);
}