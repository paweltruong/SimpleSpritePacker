
namespace SimpleSpritePacker
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lvInputFiles = new System.Windows.Forms.ListView();
            this.chName = new System.Windows.Forms.ColumnHeader();
            this.chFormat = new System.Windows.Forms.ColumnHeader();
            this.chDimension = new System.Windows.Forms.ColumnHeader();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.lbInputFiles = new System.Windows.Forms.Label();
            this.lbOutput = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnAddFiles = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnOutputFile = new System.Windows.Forms.Button();
            this.backgroundWorkerSpritePacker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // lvInputFiles
            // 
            this.lvInputFiles.AllowDrop = true;
            this.lvInputFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chFormat,
            this.chDimension});
            this.lvInputFiles.HideSelection = false;
            this.lvInputFiles.Location = new System.Drawing.Point(12, 37);
            this.lvInputFiles.Name = "lvInputFiles";
            this.lvInputFiles.Size = new System.Drawing.Size(756, 182);
            this.lvInputFiles.TabIndex = 0;
            this.lvInputFiles.UseCompatibleStateImageBehavior = false;
            this.lvInputFiles.View = System.Windows.Forms.View.Details;
            this.lvInputFiles.VirtualListSize = 10000;
            this.lvInputFiles.VirtualMode = true;
            this.lvInputFiles.CacheVirtualItems += new System.Windows.Forms.CacheVirtualItemsEventHandler(this.lvInputFiles_CacheVirtualItems);
            this.lvInputFiles.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lvInputFiles_RetrieveVirtualItem);
            this.lvInputFiles.SearchForVirtualItem += new System.Windows.Forms.SearchForVirtualItemEventHandler(this.lvInputFiles_SearchForVirtualItem);
            this.lvInputFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.lvInputFiles_DragDrop);
            this.lvInputFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.lvInputFiles_DragEnter);
            // 
            // chName
            // 
            this.chName.Text = "File";
            this.chName.Width = 600;
            // 
            // chFormat
            // 
            this.chFormat.Text = "Format";
            // 
            // chDimension
            // 
            this.chDimension.Text = "Dimension";
            this.chDimension.Width = 80;
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(12, 381);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(675, 23);
            this.txtOutput.TabIndex = 3;
            // 
            // lbInputFiles
            // 
            this.lbInputFiles.AutoSize = true;
            this.lbInputFiles.Location = new System.Drawing.Point(12, 9);
            this.lbInputFiles.Name = "lbInputFiles";
            this.lbInputFiles.Size = new System.Drawing.Size(62, 15);
            this.lbInputFiles.TabIndex = 2;
            this.lbInputFiles.Text = "Input files:";
            // 
            // lbOutput
            // 
            this.lbOutput.AutoSize = true;
            this.lbOutput.Location = new System.Drawing.Point(12, 353);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(66, 15);
            this.lbOutput.TabIndex = 2;
            this.lbOutput.Text = "OutputFile:";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(693, 415);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 4;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnAddFiles
            // 
            this.btnAddFiles.Location = new System.Drawing.Point(693, 225);
            this.btnAddFiles.Name = "btnAddFiles";
            this.btnAddFiles.Size = new System.Drawing.Size(75, 23);
            this.btnAddFiles.TabIndex = 2;
            this.btnAddFiles.Text = "Add files";
            this.btnAddFiles.UseVisualStyleBackColor = true;
            this.btnAddFiles.Click += new System.EventHandler(this.btnAddFiles_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(612, 225);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnOutputFile
            // 
            this.btnOutputFile.Location = new System.Drawing.Point(693, 381);
            this.btnOutputFile.Name = "btnOutputFile";
            this.btnOutputFile.Size = new System.Drawing.Size(75, 23);
            this.btnOutputFile.TabIndex = 2;
            this.btnOutputFile.Text = "Browse";
            this.btnOutputFile.UseVisualStyleBackColor = true;
            this.btnOutputFile.Click += new System.EventHandler(this.btnOutputFile_Click);
            // 
            // backgroundWorkerSpritePacker
            // 
            this.backgroundWorkerSpritePacker.WorkerReportsProgress = true;
            this.backgroundWorkerSpritePacker.WorkerSupportsCancellation = true;
            this.backgroundWorkerSpritePacker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerSpritePacker_DoWork);
            this.backgroundWorkerSpritePacker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerSpritePacker_ProgressChanged);
            this.backgroundWorkerSpritePacker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerSpritePacker_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnOutputFile);
            this.Controls.Add(this.btnAddFiles);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.lbOutput);
            this.Controls.Add(this.lbInputFiles);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.lvInputFiles);
            this.Name = "MainForm";
            this.Text = "SimpleSpritePacker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvInputFiles;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chFormat;
        private System.Windows.Forms.ColumnHeader chDimension;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label lbInputFiles;
        private System.Windows.Forms.Label lbOutput;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnAddFiles;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnOutputFile;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSpritePacker;
    }
}

