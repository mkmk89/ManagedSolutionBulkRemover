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

        RichTextBox LogBox { get; set; }
        MyPluginControl Plugin { get; set; }

        internal void Log(string text, Color color)
        {
            Plugin.AppendText(text, color);
        }
    }

    public class LogLine
    {
        public LogLine(string text, Color color)
        {
            this.Text = text;
            this.Color = color;
        }
        public string Text { get; set; }
        public Color Color { get; set; }
    }

    public class Solution
    {
        public Solution()
        {
            DependentSolutions = new List<Solution>();
            RequiredSolutions = new List<Solution>();
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

        public List<Solution> DependentSolutions;
        public List<Solution> RequiredSolutions;

        public List<Solution> GetDependentSolutions(Solution solution = null, List<Solution> list = null)
        {
            var currentSolution = solution ?? this;
            var sols = list ?? new List<Solution> { this };

            foreach (var ds in currentSolution.DependentSolutions)
            {
                sols.Add(ds);
                GetDependentSolutions(ds, list);
            }

            return sols;
        }

        public override string ToString()
        {
            return UniqueName;
        }
    }
}
