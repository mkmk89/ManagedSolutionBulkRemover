using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using McTools.Xrm.Connection;
using System.Activities.Expressions;
using XrmToolBox.Extensibility.Args;
using XrmToolBox.Extensibility.Interfaces;

namespace ManagedSolutionBulkRemover
{
    public partial class MyPluginControl : PluginControlBase, IStatusBarMessenger, IGitHubPlugin
    {
        private Settings mySettings;
        public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;


        #region IGitHubPlugin implementation

        public string RepositoryName => "UnmanagedLayerBulkRemover";

        public string UserName => "mkmk89";

        #endregion IGitHubPlugin implementation

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }
        }

        private void tsbGetSolutions_Click(object sender, EventArgs e)
        {
            ExecuteMethod(GetSoltuionsMethod);
        }



        private void GetSoltuionsMethod()
        {
            Logic logic = new Logic(Service);

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting solutions",
                Work = (worker, args) =>
                {
                    args.Result = logic.GetSolutions();
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    var result = args.Result as EntityCollection;
                    managedSolutionsDataGrid.DataSource = result.Entities.Select(
                        x => new SolutionItem()
                        {
                            UniqueName = x.Contains("uniquename") ? (string)x.Attributes["uniquename"] : string.Empty,
                            FriendlyName = x.Contains("friendlyname") ? (string)x.Attributes["friendlyname"] : string.Empty,
                            Version = (string)x.Attributes["version"],
                        }).OrderBy(x => x.UniqueName).ToList();
                }
            });
        }

        private void RemoveSolutions()
        {
            Logic logic = new Logic(Service);

            if (managedSolutionsDataGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select at least one solution");
                return;
            }
            List<SolutionItem> selectedRows = new List<SolutionItem>();
            foreach (DataGridViewRow row in managedSolutionsDataGrid.SelectedRows)
                selectedRows.Add(row.DataBoundItem as SolutionItem);
            var dialogResult = MessageBox.Show($"Are you sure you want to remove all selected solutions?", "Warining", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Cancel)
                return;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Removing solutions...",
                Work = (worker, args) =>
                {
                    Logger logger = new Logger(this);

                    logic.RemoveSolutions(worker, selectedRows.Select(x=>x.UniqueName).ToList(), true, logger);//TODO: make it variable
                },
                ProgressChanged = e =>
                {
                    // If progress has to be notified to user, use the following method:
                    //SetWorkingMessage("Message to display");
                    SetWorkingMessage(e.UserState.ToString());

                    // If progress has to be notified to user, through the
                    // status bar, use the following method
                    //SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(e.ProgressPercentage, e.UserState.ToString()));
                },
                PostWorkCallBack = (args) =>
                {
                    SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(string.Empty));

                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                },
                AsyncArgument = null,
                IsCancelable = true,
                MessageWidth = 340,
                MessageHeight = 150
            });
        }

       
        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            managedSolutionsDataGrid.DataSource = new List<SolutionItem>();
            managedSolutionsDataGrid.Columns[0].FillWeight = 200;
            managedSolutionsDataGrid.Columns[1].FillWeight = 300;
            managedSolutionsDataGrid.Columns[2].FillWeight = 100;

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }

        internal void AppendText(string text, Color color)
        {
            RichTextBox box = rtbLogs;
            text = text + Environment.NewLine;
            if (InvokeRequired)
            {
                this.Invoke(new Action<string, Color>(AppendText), new object[] { text, color});
                return;
            }
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }


        private void tsbDeleteSolutions_Click(object sender, EventArgs e)
        {
            ExecuteMethod(RemoveSolutions);
        }
    }
}