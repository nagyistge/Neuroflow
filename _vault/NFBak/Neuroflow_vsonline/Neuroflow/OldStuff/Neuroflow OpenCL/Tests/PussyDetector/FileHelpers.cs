using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PussyDetector.Properties;

namespace PussyDetector
{
    internal static class FileHelpers
    {
        internal static IEnumerable<FileInfo> SearchForPussyFiles()
        {
            return SearchForFiles(Settings.Default.PussyImagesPath);
        }

        internal static IEnumerable<FileInfo> SearchForNotPussyFiles()
        {
            return SearchForFiles(Settings.Default.NotPussyImagesPath);
        }

        internal static IEnumerable<FileInfo> SearchForFiles(string dir)
        {
            var di = new DirectoryInfo(dir);

            return di.EnumerateFiles("*.jpg", SearchOption.AllDirectories)
                .Concat(di.EnumerateFiles("*.jpeg", SearchOption.AllDirectories))
                .Concat(di.EnumerateFiles("*.png", SearchOption.AllDirectories))
                .Concat(di.EnumerateFiles("*.bmp", SearchOption.AllDirectories));
        }
    }
}
