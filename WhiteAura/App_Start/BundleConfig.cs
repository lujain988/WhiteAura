using System.Web.Optimization;

namespace WhiteAura
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Bundle for CSS files in assets folder
            bundles.Add(new StyleBundle("~/bundles/assets/css").Include(
                "~/Content/vendors/styles/core.css",               // Adjusted path
                "~/Content/vendors/styles/icon-font.min.css"        // Adjusted path
            ));

            // Bundle for JS files in assets folder
            bundles.Add(new ScriptBundle("~/bundles/assets/js").Include(
                "~/Content/src/plugins/datatables/js/jquery.dataTables.min.js",   // Adjusted path
                "~/Content/src/plugins/datatables/js/dataTables.bootstrap4.min.js",   // Adjusted path
                "~/Content/src/plugins/datatables/js/dataTables.responsive.min.js",   // Adjusted path
                "~/Content/src/plugins/datatables/js/responsive.bootstrap4.min.js"    // Adjusted path
            ));

            // Bundle for CSS files in the main css folder
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                "~/Content/vendors/styles/style.css",                 // Adjusted path
                "~/Content/src/plugins/datatables/css/dataTables.bootstrap4.min.css",   // Adjusted path
                "~/Content/src/plugins/datatables/css/responsive.bootstrap4.min.css"    // Adjusted path
            ));

            // Bundle for JS files in js folder
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/Content/vendors/scripts/core.js",                  // Adjusted path
                "~/Content/vendors/scripts/script.min.js",            // Adjusted path
                "~/Content/vendors/scripts/process.js",               // Adjusted path
                "~/Content/vendors/scripts/layout-settings.js"        // Adjusted path
            ));

            // If you have a combined CSS and JS folder
            bundles.Add(new ScriptBundle("~/bundles/cssjs/js").Include(
                "~/Content/css and js/script.js"
            ));

            bundles.Add(new StyleBundle("~/bundles/cssjs/css").Include(
                "~/Content/css and js/styles.css"
            ));
        }
    }
}
