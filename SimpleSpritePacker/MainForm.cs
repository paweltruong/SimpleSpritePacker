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

        public MainForm()
        {
            InitializeComponent();
            _progressForm = new ProgressForm();
            _progressForm.Canceled += ProgressForm_Canceled;
            lvInputFiles.VirtualListSize = 0;
            txtOutput.Text = @"D:\Unity\Assets\Xelu_Free_Controller&Key_Prompts\Others\Xbox 360\atlas.png";
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
            catch { }

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

                    PasteSpriteIntoAtlas2(file);
                    //if(i%2 == 1)
                    //InsertSpriteToAtlas(i);
                    //Thread.Sleep(1000);
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
            }
            else
            {

                _spriteAtlasBitmap.Dispose();
            }

            return result;
        }

        private void InsertSpriteToAtlas(int index)
        {
            var file = _generatorData.InputFiles[index];


            byte[] spritePixelData;
            using (var sprite = new Bitmap(file.Fullpath))
            {
                // Lock the bitmap's bits.  
                var rect = new Rectangle(0, 0, sprite.Width, sprite.Height);
                var spriteData = sprite.LockBits(rect, ImageLockMode.ReadOnly, sprite.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = spriteData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = Math.Abs(spriteData.Stride) * sprite.Height;
                spritePixelData = new byte[bytes];

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, spritePixelData, 0, bytes);

                // Unlock the bits.
                sprite.UnlockBits(spriteData);
            }

            PasteSpriteIntoAtlas(file, spritePixelData);



            //{
            //    Bitmap bmp = new Bitmap(file.Fullpath);

            //    // Lock the bitmap's bits.  
            //    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            //    System.Drawing.Imaging.BitmapData bmpData =
            //        bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
            //        bmp.PixelFormat);

            //    // Get the address of the first line.
            //    IntPtr ptr = bmpData.Scan0;

            //    // Declare an array to hold the bytes of the bitmap.
            //    int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            //    byte[] rgbValues = new byte[bytes];

            //    // Copy the RGB values into the array.
            //    System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            //    // Set every third value to 255. A 24bpp bitmap will look red.  
            //    for (int counter = 2; counter < rgbValues.Length; counter += 3)
            //        rgbValues[counter] = 255;

            //    // Copy the RGB values back to the bitmap
            //    System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            //    // Unlock the bits.
            //    bmp.UnlockBits(bmpData);

            //    // Draw the modified image.
            //    e.Graphics.DrawImage(bmp, 0, 150);
            //}
        }

        void PasteSpriteIntoAtlas2(GeneratorFileData file)
        {
                //Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Rectangle destRegion)

            using (var sprite = new Bitmap(file.Fullpath))
            {
                using (Graphics grD = Graphics.FromImage(_spriteAtlasBitmap))
                {
                    var srcRegion = new Rectangle(0,0, file.Width, file.Height);
                    var destRegion = new Rectangle(file.X,file.Y, file.Width, file.Height);
                    grD.DrawImage(sprite, destRegion, srcRegion, GraphicsUnit.Pixel);
                }
            }
        }

        void PasteSpriteIntoAtlas(GeneratorFileData file, byte[] spritePixelData)
        {
            
            Debug.WriteLine($"File:{Path.GetFileName(file.Fullpath)} Dim:{file.Width}x{file.Height} to ({file.X},{file.Y})");
            // Lock the bitmap's bits.  
            var rect = new Rectangle(file.X, file.Y, file.Width, file.Height);
            var atlasSlotData = _spriteAtlasBitmap.LockBits(rect, ImageLockMode.WriteOnly, _spriteAtlasBitmap.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = atlasSlotData.Scan0;

            //TODO: there is bug that inserts from the 0 position
            //var ptr = (byte*)atlasSlotData.Scan0;
            //var bpp = Image.GetPixelFormatSize(_spriteAtlasBitmap.PixelFormat);

            //for (var y = 0; y < atlasSlotData.Height; y++)
            //{
            //    // This is why real scan-width is important to have!
            //    var row = ptr + (y * atlasSlotData.Stride);

            //    for (var x = 0; x < atlasSlotData.Width; x++)
            //    {
            //        var pixel = row + x * bpp;

            //        for (var bit = 0; bit < bpp; bit++)
            //        {
            //            var pixelComponent = pixel[bit];
            //        }
            //    }
            //}


            //Insert sprite data into atlas
            System.Runtime.InteropServices.Marshal.Copy(spritePixelData, 0, ptr, spritePixelData.Length);

            // Unlock the bits.
            _spriteAtlasBitmap.UnlockBits(atlasSlotData);
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
                if (_inputFiles.Count(i => i.Width == firstFile.Width && i.Height == firstFile.Height) == _inputFiles.Count)
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
    }
}
