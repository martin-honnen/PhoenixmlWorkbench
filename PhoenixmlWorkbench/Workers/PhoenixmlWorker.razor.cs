using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using PhoenixmlDb.Xslt;
using PhoenixmlDb.XQuery;
using System.Text.Json;

[SupportedOSPlatform("browser")]
public partial class PhoenixmlWorker
{
    [JSExport]
    public static async Task<string> Transform(string xslt, string xml)
    {
        var transformer = new XsltTransformer();
        await transformer.LoadStylesheetAsync(xslt, new Uri("urn:from-string"));
        var primaryResult = await transformer.TransformAsync(xml);

        var results = new Dictionary<string, string>();
        results.Add("*** principal result ***", primaryResult);

        foreach (var secondaryResult in transformer.SecondaryResultDocuments)
        {
            results.Add(secondaryResult.Key, secondaryResult.Value);
        }

        return JsonSerializer.Serialize(results);
    }
    [JSExport]
    public static async Task<string> ExecuteXQuery(string xquery, string xml)
    {
        var xqueryFacade = new XQueryFacade();

        return await xqueryFacade.EvaluateAsync(xquery, xml);
    }

}
