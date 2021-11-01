using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace SimpleSpritePacker
{
    public class SpriteFileData
    {
        public string Fullpath { get; private set; }
        public string Extension { get; private set; }
        public string Dimension => $"{Width}x{Height}";
        public int Width { get; private set; }
        public int Height { get; private set; }


        public SpriteFileData(string path)
        {
            Fullpath = Path.GetFullPath(path);
            Extension = Path.GetExtension(Fullpath);
            SetDimension(Fullpath);
        }

        void SetDimension(string path)
        {
            //TODO:improve with reading file headers
            //https://en.wikipedia.org/wiki/Portable_Network_Graphics#File_header
            //https://github.com/drewnoakes/metadata-extractor-dotnet/blob/master/MetadataExtractor/Formats/Png/PngChunkReader.cs
            //https://www.codeproject.com/Articles/35978/Reading-Image-Headers-to-Get-Width-and-Height

            //Simple not effective way
            using (Stream stream = File.OpenRead(path))
            {
                using (var sourceImage = Image.FromStream(stream, false, false))
                {
                    Width = sourceImage.Width;
                    Height = sourceImage.Height;
                }
            }
        }
    }
}
