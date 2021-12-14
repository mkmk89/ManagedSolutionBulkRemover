using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagedSolutionBulkRemover
{
    public class SolutionItem
    {
        public string UniqueName { get; set; }
        public string FriendlyName { get; set; }
        public string Version { get; set; }
    }

    public class Logger
    {
        public Logger(MyPluginControl plugin)
        {
            this.Plugin = plugin;
        }

        MyPluginControl Plugin { get; set; }

        internal void Log(string text, Color color)
        {
            Plugin.AppendText($"{DateTime.Now}: {text}", color);
        }
    }

    public class Solution
    {
        public Solution()
        {
        }

        public Solution(Guid id, string uniqueName, EntityReference parentSolutionId) : this()
        {
            Id = id;
            UniqueName = uniqueName;
        }

        public Guid Id { get; set; }

        public string UniqueName { get; set; }
        public int NoDependencies { get; set; }

        public string FriendlyName
        {
            get { return Entity.GetAttributeValue<string>("friendlyname"); }
        }

        public Entity Entity { get; internal set; }

        public override string ToString()
        {
            return UniqueName;
        }
    }
}
