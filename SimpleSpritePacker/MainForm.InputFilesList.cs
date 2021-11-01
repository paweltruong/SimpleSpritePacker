using System.Linq;
using System.Windows.Forms;

namespace SimpleSpritePacker
{
    partial class MainForm
    {

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
    }
}
