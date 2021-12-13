namespace ManagedSolutionBulkRemover
{
    partial class MyPluginControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MyPluginControl));
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbGetSolutions = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbRemoveUnamangedLayers = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.managedSolutionsDataGrid = new System.Windows.Forms.DataGridView();
            this.lblOutput = new System.Windows.Forms.Label();
            this.rtbLogs = new System.Windows.Forms.RichTextBox();
            this.toolStripMenu.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.managedSolutionsDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbGetSolutions,
            this.tssSeparator1,
            this.tsbRemoveUnamangedLayers});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStripMenu.Size = new System.Drawing.Size(1920, 42);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tsbGetSolutions
            // 
            this.tsbGetSolutions.Image = ((System.Drawing.Image)(resources.GetObject("tsbGetSolutions.Image")));
            this.tsbGetSolutions.Name = "tsbGetSolutions";
            this.tsbGetSolutions.Size = new System.Drawing.Size(200, 36);
            this.tsbGetSolutions.Text = "Load Solutions";
            this.tsbGetSolutions.Click += new System.EventHandler(this.tsbGetSolutions_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 42);
            // 
            // tsbRemoveUnamangedLayers
            // 
            this.tsbRemoveUnamangedLayers.Image = ((System.Drawing.Image)(resources.GetObject("tsbRemoveUnamangedLayers.Image")));
            this.tsbRemoveUnamangedLayers.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRemoveUnamangedLayers.Name = "tsbRemoveUnamangedLayers";
            this.tsbRemoveUnamangedLayers.Size = new System.Drawing.Size(216, 36);
            this.tsbRemoveUnamangedLayers.Text = "Delete solutions";
            this.tsbRemoveUnamangedLayers.Click += new System.EventHandler(this.tsbDeleteSolutions_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.managedSolutionsDataGrid, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.lblOutput, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.rtbLogs, 1, 2);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(14, 65);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(6);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1906, 1010);
            this.tableLayoutPanel2.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(259, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "Select managed solutions";
            // 
            // managedSolutionsDataGrid
            // 
            this.managedSolutionsDataGrid.AccessibleName = "";
            this.managedSolutionsDataGrid.AllowUserToAddRows = false;
            this.managedSolutionsDataGrid.AllowUserToDeleteRows = false;
            this.managedSolutionsDataGrid.AllowUserToResizeRows = false;
            this.managedSolutionsDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.managedSolutionsDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.managedSolutionsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.managedSolutionsDataGrid.Location = new System.Drawing.Point(6, 31);
            this.managedSolutionsDataGrid.Margin = new System.Windows.Forms.Padding(6);
            this.managedSolutionsDataGrid.Name = "managedSolutionsDataGrid";
            this.managedSolutionsDataGrid.ReadOnly = true;
            this.managedSolutionsDataGrid.RowHeadersVisible = false;
            this.managedSolutionsDataGrid.RowHeadersWidth = 82;
            this.managedSolutionsDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.managedSolutionsDataGrid.ShowEditingIcon = false;
            this.managedSolutionsDataGrid.Size = new System.Drawing.Size(559, 988);
            this.managedSolutionsDataGrid.TabIndex = 1;
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(577, 0);
            this.lblOutput.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(76, 25);
            this.lblOutput.TabIndex = 10;
            this.lblOutput.Text = "Output";
            // 
            // rtbLogs
            // 
            this.rtbLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbLogs.Location = new System.Drawing.Point(577, 31);
            this.rtbLogs.Margin = new System.Windows.Forms.Padding(6);
            this.rtbLogs.Name = "rtbLogs";
            this.rtbLogs.Size = new System.Drawing.Size(1323, 988);
            this.rtbLogs.TabIndex = 2;
            this.rtbLogs.Text = "";
            // 
            // MyPluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.toolStripMenu);
            this.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.Name = "MyPluginControl";
            this.Size = new System.Drawing.Size(1920, 1081);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.managedSolutionsDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbGetSolutions;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView managedSolutionsDataGrid;
        private System.Windows.Forms.ToolStripButton tsbRemoveUnamangedLayers;
        private System.Windows.Forms.RichTextBox rtbLogs;
        private System.Windows.Forms.Label lblOutput;
    }
}
