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
        ExportMetadata("SmallImageBase64", "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAA3QAAAN0BcFOiBwAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAARKSURBVFiFzVdbbFRVFF37vubedmZKBxoYIwFl1NJWIAqmKiH0q0Y/UGqM0TRI2jry5TtiY4JNTCNEIyExQigiSqohBOOPpv40BLQRaWIt9CElKcFQaElfd6ZzX3O3H3XaTju9M21N6vqbc/bZa032OvvsS8yM5YSwrOz/BwHSYg+a16qfYcb7AJLkUqOv+OuWxeShhXmgQUj0Xq8SiOsZ2JKWiKjdBRq1B775Hsg9aW4C2qOyEUy8DOb9AB7KEt0N4GP11t/N2NnqLE1A/17VtJI1DH4XwLrsSmeC+4npkE8s/BKRI+bCBPTWBBKw9xHxWwBWL4x4Dm4z4VPNNo+i5EzMU8B4266ILNAJBpULAU2BJC6R+184SXAsYbrMvyVdrgk+/kNfmoD4pd2bKek2wXYeZZdpalNVQAEVJC/usrDtgHUDbFhTayQQQ5baWRRq8x8710HxtmcvsGlth4cVyCeDAhpIyU0ImzY4ZoBN2yMpQD7logRRaCFR2sKO4/dMaNogRZoU4pMzxxk2OJYAW1nNDxKlGEShZcoDid+r3mbbrmcrGcp6WBZBfg2kKZPECWuS2E5mJYYsDgs+6SNt67nPgNm3oKFCjZdKP6FI2wFOZm/TKZM6ORCT6PJg4ry/y3kaB1qNqWVmRv/hihWrLOMLOPw8uZPtmdao4A0BgNzsyb3AAnBNBwaN1E/HlejssKLuW/9G6yiNHSz/VrD5BeJ5HqaQDygOAqILL6Om/1sASQJ6dGA4cw9iguvKdCa7rYdN4NchIKgAJUFAxvytngiwGOgaB3SPGzDzSKoEIds4KthclSrBvMiTgNICQKVpIQIBCQaujgIT3n6YU4LZJuxcg5/XjhjbZWe6IWWET5wUwgCujgGWN7EtEd8MaRceHuDKOSYEgFd6Xn1nFcfrQ0gUAsC27gk80qdDzZI4GwxFRHskgMsb8wAAw9BG7rLW+FVJ0ycAQHt66j4ocmPvFcLI2Ig29yWwrVdHvpG9ucxETJNw6cEAOiNaxv0RqLEhwX9Q8rlWZRDmvF2wI6KhI6Kh+IaB8i4dBRPe5hrNl9FWGsBfa1XPuCBM/7irVBIz46UrdZsKJKcpDH2rxK5n7e8fMPFEp46VupW2fjeo4GJZEDfCiiexQwIPIHB5zJFqm8uO/5lmwhevRDcUiNaJe6DvkOEt5N4hC0926nCJ8MumAG6tzPw+pGBC5AHynx93lNrvyo5dT61nHEiqu6NhVbCbwm7sKRXOkiZnA5I7QP4f475gbfN9h+/M3vccyZ77Y++KkCYcC7ux3XmwFzQUTEB2BuA/a8pm9HTk9Ph8cTkNpdH2aJ6V5xwJI17th+VZZJ0U8w7yT/Ft9/WTO08aXrE5C0ihAg3Suu6bh1ZT/LUCNtPu1xh8E4Pwf75+Y9P+A+CcX7AFfhdMY09P3YdFmHgTYB5C/qFTxccbF5Nn0QL+Kyz7t+GyC/gHSZP0F2ELf00AAAAASUVORK5CYII="),
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