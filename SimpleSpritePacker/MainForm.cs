using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Shell;

namespace SimpleSpritePacker
{
    public partial class MainForm : Form
    {
        List<SpriteFileData> _inputFiles = new List<SpriteFileData>();
        /// <summary>
        /// array to cache items for the virtual list
        /// </summary>
        private ListViewItem[] _inputFileListViewCache;
        /// <summary>
        /// stores the index of the first item in the cache
        /// </summary>
        private int _firstCacheItemIndex;
        ProgressForm _progressForm;

        GeneratorData _generatorData;

        public MainForm()
        {
            InitializeComponent();
            _progressForm = new ProgressForm();
            _progressForm.Canceled += ProgressForm_Canceled;
            lvInputFiles.VirtualListSize = 0;
            UpdateButtons();
        }

        void UpdateButtons()
        {
            bool outputValid = false;
            try 
            { 
                var fileName = Path.GetFileName(txtOutput.Text);
                var extension = Path.GetExtension(txtOutput.Text);
                outputValid = !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(extension);
            }
            catch {}

            btnGenerate.Enabled = _inputFiles.Count > 0 && outputValid;
            btnClear.Enabled = _inputFiles.Count > 0;
        }

        void AddInputFile(string filePath)
        {
            if (!_inputFiles.Any(inputFile => inputFile.Fullpath == filePath))
            {
                _inputFiles.Add(new SpriteFileData(filePath));
                lvInputFiles.VirtualListSize = _inputFiles.Count;
                UpdateButtons();
            }
        }


        private void btnClear_Click(object sender, EventArgs e)
        {
            _inputFiles.Clear();
            _inputFileListViewCache = null;
            lvInputFiles.VirtualListSize = 0;
            lvInputFiles.Invalidate();
            UpdateButtons();
        }

        private void btnAddFiles_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    AddInputFile(openFileDialog.FileName);
                }
            }
        }

        private void btnOutputFile_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {

                saveFileDialog.Filter = "PNG files (*.png)|*.png";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutput.Text = saveFileDialog.FileName;
                }
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerSpritePacker.IsBusy != true)
            {
                _progressForm.Init(14);//_inputFiles.Count);
                _progressForm.Show(this);
                _generatorData = new GeneratorData();
                _generatorData.OutputFile = txtOutput.Text;
                this.Enabled = false;
                backgroundWorkerSpritePacker.RunWorkerAsync();
            }
        }

        private void backgroundWorkerSpritePacker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            //Assign work
            e.Result = GenerateSpriteAtlas(bw);

            // If the operation was canceled by the user,
            // set the DoWorkEventArgs.Cancel property to true.
            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void backgroundWorkerSpritePacker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //UpdateTaskBarProgress(e.ProgressPercentage, 14);
            _progressForm.UpdateProgress(e.ProgressPercentage, e.UserState);
        }

        private void backgroundWorkerSpritePacker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Enabled = true;
            _progressForm.Hide();
            if (e.Cancelled)
            {
                // The user canceled the operation.
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                MessageBox.Show(msg);
            }
            else
            {
                // The operation completed normally.
            }
        }

        bool GenerateSpriteAtlas(BackgroundWorker bw)
        {
            CreateOutputSprite();

            //TODO:split inserting to output bitmap to multiple threads?
            int files = 14;

            for (int i = 0; i < files; ++i)
            {
                if (!bw.CancellationPending)
                {
                    bw.ReportProgress(i + 1, $"Inserting file{i} ");
                    Thread.Sleep(1000);
                }
                else
                    return false;
            }
            return true;
        }

        void CreateOutputSprite()
        {
            using (var fileStream = File.Create(_generatorData.OutputFile))
            {
                CalculateSpriteAtlasDimensions(out int width, out int height);
                using (var memoryStream = new MemoryStream())
                {
                    //create new bitmap
                    using (var bmp = new Bitmap(width, height))
                    {
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.Transparent);
                            g.Flush();
                        }
                        bmp.Save(memoryStream, ImageFormat.Png);
                    }

                    //append to atlas file
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                    fileStream.Flush();
                }
            }
        }

        void CalculateSpriteAtlasDimensions(out int width, out int height)
        {
            width = 1;
            height = 1;

            if (_inputFiles.Count > 0)
            {
                var firstFile = _inputFiles[0];

                //if all files have same dimensions
                if (_inputFiles.Count(i => i.Width == firstFile.Width && i.Height == firstFile.Height) == _inputFiles.Count)
                {
                    //simple square atlas
                    var squareValue = (int)Math.Ceiling(Math.Sqrt(_inputFiles.Count));
                    width = squareValue * firstFile.Width;
                    height = squareValue * firstFile.Height;
                }
                else
                {

                    //TODO: calculate more advanced scenarios
                }
            }
        }

        private void ProgressForm_Canceled(object sender, EventArgs e)
        {
            if (backgroundWorkerSpritePacker.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorkerSpritePacker.CancelAsync();
            }
        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }
    }
}
