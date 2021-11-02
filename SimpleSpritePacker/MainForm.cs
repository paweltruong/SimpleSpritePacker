using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        Bitmap _spriteAtlasBitmap;

        public bool AllInputFilesSameDimensions
        {
            get
            {
                var firstFile = _inputFiles.FirstOrDefault();
                if (firstFile == null)
                    return true;
                return _inputFiles.Count(i => i.Width == firstFile.Width && i.Height == firstFile.Height) == _inputFiles.Count;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            _progressForm = new ProgressForm();
            _progressForm.Canceled += ProgressForm_Canceled;
            lvInputFiles.VirtualListSize = 0;
            txtOutput.Text = @"C:\atlas.png";
            lbInputCount.Text = string.Empty;
            lbDimensionVariesWarning.Visible = false;
            UpdateButtons();
        }

        void UpdateButtons()
        {
            lbInputCount.Text = _inputFiles.Count.ToString();
            lbDimensionVariesWarning.Visible = !AllInputFilesSameDimensions;

            bool outputValid = false;
            try
            {
                var fileName = Path.GetFileName(txtOutput.Text);
                var extension = Path.GetExtension(txtOutput.Text);
                outputValid = !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(extension);
            }
            catch { }

            btnGenerate.Enabled = _inputFiles.Count > 0 && outputValid;
            btnClear.Enabled = _inputFiles.Count > 0;
            btnSortAsc.Enabled = btnSortDesc.Enabled = _inputFiles.Count > 0;
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
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in openFileDialog.FileNames)
                        AddInputFile(file);
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
                SetGeneratorData();

                _progressForm.Init(_inputFiles.Count);//_inputFiles.Count);
                _progressForm.Show(this);
                this.Enabled = false;

                backgroundWorkerSpritePacker.RunWorkerAsync();
            }
        }

        void SetGeneratorData()
        {
            _generatorData = new GeneratorData();
            _generatorData.OutputFile = txtOutput.Text;
            _generatorData.GenerateFileList = chGenerateFileList.Checked;

            string dir = Path.GetDirectoryName(txtOutput.Text);
            string fileName = Path.GetFileNameWithoutExtension(txtOutput.Text);
            _generatorData.ContentListFile = Path.GetFullPath(Path.Combine(dir, fileName + ".txt"));

            var placementMode = CalculateSpriteAtlasDimensions(out int atlasWidth, out int atlasHeight);
            _generatorData.Width = atlasWidth;
            _generatorData.Height = atlasHeight;
            //prepare sprite placement

            //For simple square
            switch (placementMode)
            {
                default:
                case SpriteAtlasPlacementMode.SimpleSquare:
                    var squareValue = (int)Math.Ceiling(Math.Sqrt(_inputFiles.Count));
                    var inputFiles = new List<GeneratorFileData>();
                    for (int i = 0; i < _inputFiles.Count; ++i)
                    {
                        var rowIndex = i / squareValue;
                        var colIndex = i % squareValue;

                        var inputFile = _inputFiles[i];
                        inputFiles.Add(new GeneratorFileData
                        {
                            Fullpath = inputFile.Fullpath,
                            Width = inputFile.Width,
                            Height = inputFile.Height,
                            X = colIndex * inputFile.Width,
                            Y = rowIndex * inputFile.Height
                        });
                    }
                    _generatorData.InputFiles = inputFiles.ToArray();
                    break;
            }

            //TODO:more advanced placing

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
                if (e.Error is UnauthorizedAccessException)
                {
                    MessageBox.Show($"{e.Error.Message}\r\nTry runnig application as administrator", e.Error.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    string msg = String.Format("An error occurred: {0}", e.Error.Message);
                    MessageBox.Show(msg);
                }
            }
            else
            {
                // The operation completed normally.
            }
        }

        bool GenerateSpriteAtlas(BackgroundWorker bw)
        {
            bool result = true;
            CreateOutputSprite();

            //TODO:split inserting to output bitmap to multiple threads?
            int fileCount = _generatorData.InputFiles.Length;

            _spriteAtlasBitmap = new Bitmap(_generatorData.OutputFile);


            for (int i = 0; i < fileCount; ++i)
            {
                if (!bw.CancellationPending)
                {
                    var file = _generatorData.InputFiles[i];
                    var fileName = Path.GetFileName(file.Fullpath);
                    bw.ReportProgress(i + 1, $"Inserting file{fileName} {i + 1}/{fileCount}");

                    InsertIntoAtlas(file);
                }
                else
                {
                    result = false;
                    break;
                }
            }

            if (result)
            {
                //Save to memory to dispose spriteAtlas handle
                using (var memoryStream = new MemoryStream())
                {
                    _spriteAtlasBitmap.Save(memoryStream, ImageFormat.Png);
                    _spriteAtlasBitmap.Dispose();

                    //Append to atlas file
                    using (var fileStream = File.OpenWrite(_generatorData.OutputFile))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }

                //Generate file list
                if (_generatorData.GenerateFileList)
                {
                    File.WriteAllLines(_generatorData.ContentListFile, _generatorData.InputFiles.Select(f => f.Fullpath));
                }
            }
            else
            {

                _spriteAtlasBitmap.Dispose();
            }

            return result;
        }

        void InsertIntoAtlas(GeneratorFileData file)
        {
            using (var sprite = new Bitmap(file.Fullpath))
            {
                using (Graphics grD = Graphics.FromImage(_spriteAtlasBitmap))
                {
                    var srcRegion = new Rectangle(0, 0, file.Width, file.Height);
                    var destRegion = new Rectangle(file.X, file.Y, file.Width, file.Height);
                    grD.DrawImage(sprite, destRegion, srcRegion, GraphicsUnit.Pixel);
                }
            }
        }

        void CreateOutputSprite()
        {
            using (var fileStream = File.Create(_generatorData.OutputFile))
            {
                using (var memoryStream = new MemoryStream())
                {
                    //create new bitmap
                    using (var bmp = new Bitmap(_generatorData.Width, _generatorData.Height))
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

        SpriteAtlasPlacementMode CalculateSpriteAtlasDimensions(out int width, out int height)
        {
            width = 1;
            height = 1;

            if (_inputFiles.Count > 0)
            {
                var firstFile = _inputFiles[0];

                //if all files have same dimensions
                if (AllInputFilesSameDimensions)
                {
                    //simple square atlas
                    var squareValue = (int)Math.Ceiling(Math.Sqrt(_inputFiles.Count));
                    width = squareValue * firstFile.Width;
                    height = squareValue * firstFile.Height;
                    return SpriteAtlasPlacementMode.SimpleSquare;
                }
                else
                {
                    //TODO: calculate more advanced scenarios
                }
            }
            return SpriteAtlasPlacementMode.Unknown;
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

        private void lvInputFiles_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lvInputFiles.SelectedIndices.Count > 0)
            {
                var list = lvInputFiles.SelectedIndices.Cast<int>().ToList();
                list.Sort();
                list.Reverse();

                foreach (var indexToDelete in list)
                {
                    _inputFiles.RemoveAt(indexToDelete);
                }

                lvInputFiles.VirtualListSize = _inputFiles.Count;
                _inputFileListViewCache = null;
                lvInputFiles.Invalidate();
                UpdateButtons();
            }

        }

        private void btnSortAsc_Click(object sender, EventArgs e)
        {
            _inputFiles = _inputFiles.OrderBy(f => Path.GetFileName(f.Fullpath)).ToList();
            _inputFileListViewCache = null;
            lvInputFiles.Invalidate();
        }

        private void btnSortDesc_Click(object sender, EventArgs e)
        {
            _inputFiles = _inputFiles.OrderByDescending(f => Path.GetFileName(f.Fullpath)).ToList();
            _inputFileListViewCache = null;
            lvInputFiles.Invalidate();
        }
    }
}
