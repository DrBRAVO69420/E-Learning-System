using System.Web;
using System.Web.Optimization;

namespace OnlineLearning
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                        "~/Scripts/jquery.signalR-2.4.1.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css",
                       "~/Content/w3css.css",
                       "~/Content/customstyle.css"
                       ));

            bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
                "~/Scripts/kendo/kendo.all.min.js",
                "~/Scripts/kendo/kendo.aspnetmvc.min.js",
                "~/Scripts/kendo/kendo.datepicker.min.js",
                "~/Scripts/kendo/jszip.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/custom").Include(
                        "~/Scripts/CustomGeneral.js"));

            bundles.Add(new ScriptBundle("~/bundles/pako").Include(
                "~/Scripts/pako_deflate.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/waypoint").Include(
                "~/Scripts/jquery.waypoints.min.js"));

            bundles.Add(new StyleBundle("~/Content/kendo/css").Include(
                 "~/Content/kendo/kendo.common-bootstrap.min.css",
                "~/Content/kendo/kendo.bootstrap.min.css",
                "~/Content/kendo/kendo.bootstrap.mobile.min.css"
            ));

            // Clear all items from the default ignore list to allow minified CSS and JavaScript files to be included in debug mode
            bundles.IgnoreList.Clear();

            // Add back some of the default ignore list rules
            bundles.IgnoreList.Ignore("*.intellisense.js");
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);

        }
    }
}
