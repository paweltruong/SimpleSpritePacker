using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SimpleSpritePacker
{
    public struct GeneratorData
    {
        public string OutputFile;
        public int Width;
        public int Height;
        public GeneratorFileData[] InputFiles;
        public bool GenerateFileList;
        public string ContentListFile;
    }

    public struct GeneratorFileData
    {
        public string Fullpath;
        public int Width;
        public int Height;
        public int X;
        public int Y;
    }
}
