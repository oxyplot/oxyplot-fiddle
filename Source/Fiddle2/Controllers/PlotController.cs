namespace Fiddle2.Controllers
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;

    [ApiController]
    [Route("[controller]")]
    public class PlotController : ControllerBase
    {
        public async Task<IActionResult> Get(string format, string source, int width = 800, int height = 400, string renderer = null)
        {
            var options = ScriptOptions.Default.WithReferences(typeof(OxyPlot.PlotModel).Assembly)
                .WithImports("System", "OxyPlot", "OxyPlot.Axes", "OxyPlot.Series", "OxyPlot.Annotations");

            var result = await CSharpScript.EvaluateAsync(source, options);

            var model = result as OxyPlot.PlotModel;

            var contentType = this.Request.HasFormContentType ? this.Request.ContentType : "image/png";
            if (format != null && format == "svg") contentType = "image/svg+xml";

            switch (contentType)
            {
                case "image/svg+xml":
                    {
                        var e = new OxyPlot.SvgExporter { Width = width, Height = height };
                        var svg = e.ExportToString(model);
                        return this.Content(svg, "image/svg+xml");
                    }

                default:
                    if (renderer == "ImageSharp")
                    {
                        var p = new OxyPlot.ImageSharp.PngExporter { Width = width, Height = height };
                        var stream = new MemoryStream();
                        p.Export(model, stream);
                        stream.Position = 0;
                        return this.File(stream, "image/png");
                    }
                    else
                    {
                        var p = new OxyPlot.Core.Drawing.PngExporter { Width = width, Height = height };
                        var stream = new MemoryStream();
                        p.Export(model, stream);
                        stream.Position = 0;
                        return this.File(stream, "image/png");
                    }
            }
        }
    }
}