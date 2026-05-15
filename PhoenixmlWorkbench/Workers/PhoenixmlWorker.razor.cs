using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using PhoenixmlDb.Xslt;
using PhoenixmlDb.XQuery;
using System.Text.Json;

[SupportedOSPlatform("browser")]
public partial class PhoenixmlWorker
{
    static string transpilerURI = "/examples/xslt/schematron/schxslt2/transpile-indent-report.xsl";

    static string? transpilerCode = null;
    static string? _baseUrl = null;

    static string[] docbookFilesToBePreloaded = new string[] { 
            "/examples/xslt/docbook/xslt/modules/templates.xml",
            "/examples/xslt/docbook/xslt/transforms/00-logstruct.xsl",
            "/examples/xslt/docbook/xslt/transforms/50-normalize.xsl",
            "/examples/xslt/docbook/xslt/transforms/52-calstables.xsl",
            "/examples/xslt/docbook/xslt/transforms/70-xlinkbase.xsl",
            "/examples/xslt/docbook/xslt/locale/en.xml",
            "/examples/xslt/docbook/xslt/environment.xsl"
    };

    static PreloadedResources preloadedResources = new PreloadedResources();

    [JSExport]
    public static void Initialize(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
    }

    static bool docbookCompiled = false;

    static XsltTransformer docbookTransformer = new XsltTransformer();

    private static Uri ToAbsoluteUri(string relativeOrAbsolute)
    {
        if (Uri.IsWellFormedUriString(relativeOrAbsolute, UriKind.Absolute))
            return new Uri(relativeOrAbsolute);

        if (_baseUrl is null)
            throw new InvalidOperationException("Initialize() must be called before use.");

        return new Uri(_baseUrl + "/" + relativeOrAbsolute.TrimStart('/'));
    }

    private static async Task<string> GetTranspilerCodeAsync()
    {
        if (transpilerCode == null)
        {
            using var http = new HttpClient();
            transpilerCode = await http.GetStringAsync(ToAbsoluteUri(transpilerURI));
        }
        return transpilerCode;
    }

    [JSExport]
    public static async Task<string> Transform(string xslt, string xml, string codeBaseURI, string inputBaseURI)
    {
        var transformer = new XsltTransformer();

        await transformer.LoadStylesheetAsync(xslt, new Uri(codeBaseURI));

        transformer.SetSourceDocumentUri(new Uri(inputBaseURI));

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
    public static async Task<string> Docbook(string xslt, string xml, string codeBaseURI, string inputBaseURI)
    {
        var transformer = docbookTransformer;

        if (!docbookCompiled)
        {

            using var http = new HttpClient();

            transformer.PreloadedResources = preloadedResources;

            foreach (var docbookFile in docbookFilesToBePreloaded)
            {
                var absoluteUri = ToAbsoluteUri(docbookFile);

                if (!transformer.PreloadedResources.Contains(absoluteUri))
                {
                    var content = await http.GetStringAsync(absoluteUri);
                    transformer.PreloadedResources.Add(absoluteUri, content);
                }
            }

            await transformer.LoadStylesheetAsync(xslt, new Uri(codeBaseURI));

            docbookCompiled = true;
        }

        transformer.SetSourceDocumentUri(new Uri(inputBaseURI));

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
    public static async Task<string> Schematron(string schematron, string xml, string schematronBaseURI, string inputBaseURI)
    {
        var transformer = new XsltTransformer();

        await transformer.LoadStylesheetAsync(await GetTranspilerCodeAsync(), new Uri(transpilerURI));

        transformer.SetSourceDocumentUri(new Uri(schematronBaseURI));

        var transpiledSchematron = await transformer.TransformAsync(schematron);

        transformer = new XsltTransformer();

        await transformer.LoadStylesheetAsync(transpiledSchematron, new Uri(schematronBaseURI));

        transformer.SetSourceDocumentUri(new Uri(inputBaseURI));

        var svrlReport = await transformer.TransformAsync(xml);

        return svrlReport;
    }
    [JSExport]
    public static async Task<string> ExecuteXQuery(string xquery, string xml)
    {
        var xqueryFacade = new XQueryFacade();

        return await xqueryFacade.EvaluateAsync(xquery, xml);
    }

}
