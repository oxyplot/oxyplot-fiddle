namespace Fiddle
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;

    using OxyPlot;
    using OxyPlot.WindowsForms;

    public class PlotController : ApiController
    {
        public async Task<HttpResponseMessage> Get(string source, int width = 900, int height = 600)
        {
            var options = ScriptOptions.Default.WithReferences(typeof(PlotModel).Assembly)
                .WithImports("System", "OxyPlot", "OxyPlot.Axes", "OxyPlot.Series", "OxyPlot.Annotations");

            var result = await CSharpScript.EvaluateAsync(source, options);

            var model = result as PlotModel;

            var contentType = this.Request.Content.Headers.ContentType;
            var mediaType = "image/png";
            if (contentType != null)
            {
                mediaType = contentType.MediaType;
            }

            switch (mediaType)
            {
                case "image/svg+xml":
                    {
                        var e = new OxyPlot.WindowsForms.SvgExporter { Width = width, Height = height };
                        var svg = e.ExportToString(model);
                        var res = this.Request.CreateResponse(HttpStatusCode.OK);
                        res.Content = new StringContent(svg, Encoding.UTF8);
                        res.Content.Headers.ContentType = new MediaTypeHeaderValue("image/svg+xml");
                        return res;
                    }

                default:
                    {
                        var p = new PngExporter { Width = width, Height = height };
                        var stream = new MemoryStream();
                        p.Export(model, stream);

                        var res = this.Request.CreateResponse(HttpStatusCode.OK);
                        res.Content = new ByteArrayContent(stream.ToArray());
                        res.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                        return res;
                    }
            }
        }
    }
}