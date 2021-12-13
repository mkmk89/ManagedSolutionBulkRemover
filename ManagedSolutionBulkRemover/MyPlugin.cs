using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace ManagedSolutionBulkRemover
{
    // Do not forget to update version number and author (company attribute) in AssemblyInfo.cs class
    // To generate Base64 string for Images below, you can use https://www.base64-image.de/
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Managed Solution Bulk Remover"),
        ExportMetadata("Description", "This plugin can remove unmanaged (active) layers from solution components in bulk. Just add components to solution, run the plugin and remove Active layers automatically."),
        // Please specify the base64 content of a 32x32 pixels image
        ExportMetadata("SmallImageBase64", "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAOQSURBVFhHxVe9axVBEJ+99zQSSGFlEcFAxEIxCIKmErEw+Qc0YKuQxkobsRBtBDslYPMQiZ2dnYWIJBFB4kejlR9VPiEKmkL03btzZnbndnZv3+V1+cFkZue387Efd3kHuw3jdDPeLOyDVnkPZQaychha6GuVgLbTegw/wJTzcHD6NsfugMEaeL3QwSJXqoL1osp2XFbeGaSJnRt4ubQXk/5GGdqxqOay8jscmhpHqxEU1ozCDEPPDEGOvfZQRGtb+1gwrmf22wTNMPBkuY2dL2HHx3gFtvt4VSN1zo21Xee2Ay7EcxidnsmA/D1zAGWEJdcarC0ra9wFzhNKlcvlKVFDJaMo7g7ML2doDdsVF7bbNrGkUdiPkiHHOyG+BKdXzDlQ2i4n8TTmquYPjE7RYe0u7A4IHr17hp2eq1bAq1W2rC5eaZIj7cbazsoOjJ+/jiMGuT165ieflRY5v8AnOuZwLJwWuU+W++WqMeIG1lH8hQofrxxlEgq4qHxW7DzLl45nH+YJc9B4jWs5RA3Aei3ABpFsw4XJt6hfVD7WFMf2Npw9Y/l+Oay9aYtZxDuwVgtgzZzM0cm8kI+guSAHCcev2IkWcQMbYYASKcBjp+MCFa98OhfZOR6zQthAblZ9AI/DJASdTGvhaax9YbNdlC070SJsoHCX0AcocXNqBRxHPoL2yzw/fxNOn6VbUyFs4MbEP5y0FRTQSQjijznSFY9a5xDJwwtICBsgyKNoA8IElnecskUIcQyPK/+qneSRaADURaSx2CiEZAHSlsZjVD6n/dzgHUBI7cBqFORtAhXqV4CgxyJ+3oad5FFvIM/WaoklCSHwu7HM17wcUZhrgCMo8G1YJSGNvmSBSAtP4xRnd26AS0jnpIMHLoBCkF3R4ucNsgPYZeqG03+yW5/H8IiOJjjSIzD3cQy6jmcf5tFN0qs+QmoHVqoACba6jf/pvmHSxYDzK27j8SFvFqOiIrW3IKHfHSjDJOgnnZsMdSvgxLYaefxZkuJyfAIunZKHtUK9gYdH6G34KZlEfL6hOpecz+MPtkCIegOEwsxxkTiJTRTqFBf4ME/PFCgPOHeEdAOdwx0MeOyTkEZ/36IRJ7b33YXZk684dwRkG3D5K34PwlX8ITmBYvr+IA1/dDofdNF+j/Z9uHbiKaVLobkBweyXPfh3yBdCoe8A/mbgYlbLdwB/W8BfuHm8i6MGAPwHeoS4YyWHHIgAAAAASUVORK5CYII="),
        // Please specify the base64 content of a 80x80 pixels image
        ExportMetadata("BigImageBase64", "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"),
        ExportMetadata("BackgroundColor", "Orange"),
        ExportMetadata("PrimaryFontColor", "Black"),
        ExportMetadata("SecondaryFontColor", "Gray")]
    public class MyPlugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new MyPluginControl();
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public MyPlugin()
        {
            // If you have external assemblies that you need to load, uncomment the following to 
            // hook into the event that will fire when an Assembly fails to resolve
            // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
        }

        /// <summary>
        /// Event fired by CLR when an assembly reference fails to load
        /// Assumes that related assemblies will be loaded from a subfolder named the same as the Plugin
        /// For example, a folder named Sample.XrmToolBox.MyPlugin 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly loadAssembly = null;
            Assembly currAssembly = Assembly.GetExecutingAssembly();

            // base name of the assembly that failed to resolve
            var argName = args.Name.Substring(0, args.Name.IndexOf(","));

            // check to see if the failing assembly is one that we reference.
            List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
            var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

            // if the current unresolved assembly is referenced by our plugin, attempt to load
            if (refAssembly != null)
            {
                // load from the path to this plugin assembly, not host executable
                string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
                string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
                dir = Path.Combine(dir, folder);

                var assmbPath = Path.Combine(dir, $"{argName}.dll");

                if (File.Exists(assmbPath))
                {
                    loadAssembly = Assembly.LoadFrom(assmbPath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
                }
            }

            return loadAssembly;
        }
    }
}