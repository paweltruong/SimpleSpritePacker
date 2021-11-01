using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public MainForm()
        {
            InitializeComponent();
            _progressForm = new ProgressForm();
            _progressForm.Canceled += ProgressForm_Canceled;
            lvInputFiles.VirtualListSize = 0;
        }

        private void lvInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            //Filter elements that are allowed to drop
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false)
                //TODO:dont allow bitmap for now, because we would need to id them with hash and so modify how input file list works
                //|| e.Data.GetDataPresent(DataFormats.Bitmap, false)
                //TODO:check for string file path and is an image
                //|| (e.Data.GetDataPresent(DataFormats.Text, false) && Image.)
                //https://web.archive.org/web/20090302032444/http://www.mikekunz.com/image_file_header.html
                )
                e.Effect = DragDropEffects.Copy;
        }

        private void lvInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                    AddInputFile(file);
            }
            //else if (e.Data.GetDataPresent(DataFormats.Bitmap, false))
            //    filePath = (e.Data.GetData(DataFormats.Bitmap) as Bitmap).;

        }

        void AddInputFile(string filePath)
        {
            if (!_inputFiles.Any(inputFile => inputFile.Fullpath == filePath))
            {
                _inputFiles.Add(new SpriteFileData(filePath));

                lvInputFiles.VirtualListSize = _inputFiles.Count;
            }
        }

        private void lvInputFiles_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            //Caching is not required but improves performance on large sets.
            //To leave out caching, don't connect the CacheVirtualItems event 
            //and make sure myCache is null.

            //check to see if the requested item is currently in the cache
            if (_inputFileListViewCache != null
                && _inputFileListViewCache.Length != 1 && _inputFileListViewCache[0] != null
                && e.ItemIndex >= _firstCacheItemIndex && e.ItemIndex < _firstCacheItemIndex + _inputFileListViewCache.Length)
            {
                //A cache hit, so get the ListViewItem from the cache instead of making a new one.
                e.Item = _inputFileListViewCache[e.ItemIndex - _firstCacheItemIndex];
            }
            else if (e.ItemIndex < _inputFiles.Count)
            {
                //A cache miss, so create a new ListViewItem and pass it back.
                e.Item = CreateInputFileListViewItem(e.ItemIndex);
            }
        }

        private void lvInputFiles_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            //Do we need refreshing the cache
            if (_inputFileListViewCache != null && e.StartIndex >= _firstCacheItemIndex && e.EndIndex <= _firstCacheItemIndex + _inputFileListViewCache.Length)
            {
                //If the newly requested cache is a subset of the old cache, 
                //no need to rebuild everything, so do nothing.
                return;
            }

            //Rebuild the cache.
            _firstCacheItemIndex = e.StartIndex;
            int length = e.EndIndex - e.StartIndex + 1; //indexes are inclusive
            _inputFileListViewCache = new ListViewItem[length];

            //Fill the cache with the appropriate ListViewItems.
            if (_inputFiles.Count != 0)
            {
                for (int cacheIndex = 0; cacheIndex < length; cacheIndex++)
                {
                    var inputFileIndex = cacheIndex + _firstCacheItemIndex;
                    _inputFileListViewCache[cacheIndex] = CreateInputFileListViewItem(_inputFiles.ElementAt(inputFileIndex));
                }
            }
        }

        private void lvInputFiles_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
        {

            var foundSprite = _inputFiles.FirstOrDefault(s => s.Fullpath.Contains(e.Text));
            if (foundSprite != null)
            {
                e.Index = _inputFiles.IndexOf(foundSprite);
            }
        }


        ListViewItem CreateInputFileListViewItem(int index)
        {
            var sprite = _inputFiles.ElementAt(index);

            return CreateInputFileListViewItem(sprite);
        }


        private ListViewItem CreateInputFileListViewItem(SpriteFileData sprite)
        {
            var newItem = new ListViewItem(sprite.Fullpath);
            newItem.SubItems.Add(sprite.Extension);
            newItem.SubItems.Add(sprite.Dimension);
            return newItem;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _inputFiles.Clear();
            _inputFileListViewCache = null;
            lvInputFiles.VirtualListSize = 0;
            lvInputFiles.Invalidate();
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
            //TODO:split inserting to output bitmap to multiple threads?
            int files = 14;

            for (int i = 0; i < files; ++i)
            {
                if (!bw.CancellationPending)
                {
                    bw.ReportProgress(i+1, $"Inserting file{i} ");
                    Thread.Sleep(1000);
                }
                else
                    return false;
            }
            return true;
        }

        private void ProgressForm_Canceled(object sender, EventArgs e)
        {
            if (backgroundWorkerSpritePacker.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorkerSpritePacker.CancelAsync();
            }
        }

    }
}
