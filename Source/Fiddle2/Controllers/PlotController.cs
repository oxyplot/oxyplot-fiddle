namespace Fiddle2.Controllers
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
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
            var contentType = this.Request.HasFormContentType ? this.Request.ContentType : "image/png";
            if (format != null && format == "svg") contentType = "image/svg+xml";

            OxyPlot.PlotModel model;

            try
            {
                var options = ScriptOptions.Default.WithReferences(typeof(OxyPlot.PlotModel).Assembly)
                    .WithImports("System", "OxyPlot", "OxyPlot.Axes", "OxyPlot.Series", "OxyPlot.Annotations");
                source = source.Replace("\n", "\r\n");
                var result = await CSharpScript.EvaluateAsync(source, options);
                model = result as OxyPlot.PlotModel;
            }
            catch (Exception e)
            {
                model = new OxyPlot.PlotModel() { Title = e.GetType().ToString() };
                var regex = new Regex(@"\((\d*),(\d*)\): error (.*?):(.*)");
                var match = regex.Match(e.Message);
                var message = e.Message;
                if (match.Success)
                {
                    model.Subtitle = "on line " + match.Groups[1].Value + " character " + match.Groups[2].Value;
                    message = match.Groups[4].Value;
                }
                model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Left, Minimum = -1, Maximum = 1 });
                model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Minimum = -1, Maximum = 1 });
                model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation()
                {
                    Text = message,
                    TextPosition = new OxyPlot.DataPoint(0, 0),
                    TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                    TextVerticalAlignment = OxyPlot.VerticalAlignment.Middle
                });
            }

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
                        var p = new OxyPlot.ImageSharp.PngExporter { Width = width, Height = height, Background = OxyPlot.OxyColors.White };
                        var stream = new MemoryStream();
                        p.Export(model, stream);
                        stream.Position = 0;
                        return this.File(stream, "image/png");
                    }
                    else
                    {
                        var p = new OxyPlot.Core.Drawing.PngExporter { Width = width, Height = height, Background = OxyPlot.OxyColors.White };
                        var stream = new MemoryStream();
                        p.Export(model, stream);
                        stream.Position = 0;
                        return this.File(stream, "image/png");
                    }
            }
        }
    }
}