namespace Fiddle
{
    using System.Web.Http;

    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;

    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfiguration = new HttpConfiguration();

            // Configure Web API Routes:
            // - Enable Attribute Mapping
            // - Enable Default routes at /api.
            httpConfiguration.MapHttpAttributeRoutes();
            httpConfiguration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            app.UseWebApi(httpConfiguration);

            app.UseFileServer(new FileServerOptions
            {
                EnableDefaultFiles = true,
                DefaultFilesOptions = { DefaultFileNames = { "index.html" } },
                FileSystem = new PhysicalFileSystem("./public"),
                EnableDirectoryBrowsing = false
            });
        }
    }
}