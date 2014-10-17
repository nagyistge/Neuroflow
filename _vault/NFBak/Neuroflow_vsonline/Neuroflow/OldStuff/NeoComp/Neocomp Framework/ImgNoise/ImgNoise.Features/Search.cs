using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.IO;
using System.Drawing;
using NeoComp.Caching;
using System.Web.Caching;

namespace ImgNoise.Features
{
    internal static class Search
    {
        static readonly ICache Cache = new ASPNETCache(TimeSpan.FromMinutes(10), CacheItemPriority.BelowNormal);
        
        #region Entry

        internal static IEnumerable<NoisedImagePartReader> FindNIPReaders(SearchingParams searchingPars, SamplingParams samplingPars)
        {
            Contract.Requires(!searchingPars.IsEmpty);
            Contract.Requires(!samplingPars.IsEmpty);
            Contract.Ensures(Contract.Result<IEnumerable<NoisedImagePartReader>>() != null);

            return SearchForNIPs(samplingPars, SearchForFiles(searchingPars));
        }

        internal static IEnumerable<NoisedImageLineReader> FindNILReaders(SearchingParams searchingPars, SamplingParams samplingPars)
        {
            Contract.Requires(!searchingPars.IsEmpty);
            Contract.Requires(!samplingPars.IsEmpty);
            Contract.Ensures(Contract.Result<IEnumerable<NoisedImageLineReader>>() != null);

            return SearchForNILs(samplingPars, SearchForFiles(searchingPars));
        } 

        #endregion

        #region ToNIL

        private static IEnumerable<NoisedImageLineReader> SearchForNILs(SamplingParams samplingPars, IEnumerable<FileInfo> files)
        {
            double noise = 0.0;
            int sampleIndex = 0;
            foreach (var file in files)
            {
                int size = samplingPars.Size;
                int width = samplingPars.Width;
                var imgSize = GetImageSize(file);
                if (imgSize != null && imgSize.Value.Width >= width && imgSize.Value.Height >= size)
                {
                    // Supported and fits.
                    string fileName = file.FullName;
                    for (int y = 0; y < imgSize.Value.Height - size; y += size)
                    {
                        for (int x = 0; x < imgSize.Value.Width - width; x += size)
                        {
                            if ((sampleIndex++ % samplingPars.Frequency) == 0)
                            {
                                var p = new Point(x, y);
                                int ch = RandomGenerator.Random.Next(3);
                                switch (ch)
                                {
                                    case 0:
                                        yield return new NoisedImageLineReader(fileName, p, size, width, Channel.Red, noise);
                                        break;
                                    case 1:
                                        yield return new NoisedImageLineReader(fileName, p, size, width, Channel.Green, noise);
                                        break;
                                    case 3:
                                        yield return new NoisedImageLineReader(fileName, p, size, width, Channel.Blue, noise);
                                        break;
                                }

                                noise += 0.001;
                                if (Math.Round(noise, 5) > Properties.Settings.Default.MaxNoiseLevel) noise = 0.0;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region To NIP

        private static IEnumerable<NoisedImagePartReader> SearchForNIPs(SamplingParams samplingPars, IEnumerable<FileInfo> files)
        {
            double noise = 0.0;
            int sampleIndex = 0;
            foreach (var file in files)
            {
                int size = samplingPars.Size;
                var imgSize = GetImageSize(file);
                if (imgSize != null && imgSize.Value.Width >= size && imgSize.Value.Height >= size)
                {
                    // Supported and fits.
                    string fileName = file.FullName;
                    for (int y = 0; y < imgSize.Value.Height - size; y += size)
                    {
                        for (int x = 0; x < imgSize.Value.Width - size; x += size)
                        {
                            if ((sampleIndex++ % samplingPars.Frequency) == 0)
                            {
                                var p = new Point(x, y);
                                int ch = RandomGenerator.Random.Next(3);
                                switch (ch)
                                {
                                    case 0:
                                        yield return new NoisedImagePartReader(fileName, p, size, Channel.Red, noise);
                                        break;
                                    case 1:
                                        yield return new NoisedImagePartReader(fileName, p, size, Channel.Green, noise);
                                        break;
                                    case 3:
                                        yield return new NoisedImagePartReader(fileName, p, size, Channel.Blue, noise);
                                        break;
                                }
                                
                                noise += 0.001;
                                if (Math.Round(noise, 5) > Properties.Settings.Default.MaxNoiseLevel) noise = 0.0;
                            }
                        }
                    }
                }
            }
        }

        private static Size? GetImageSize(FileInfo file)
        {
            try
            {
                string key = "IMGSIZE:" + file.FullName;
                Size? size = Cache[key] as Size?;
                if (size == null)
                {
                    var img = Image.FromFile(file.FullName);
                    size = new Size(img.Width, img.Height);
                    Cache.Add(key, size);
                }
                return size;
            }
            catch
            {
                return null; // Not supported format.
            }
        }

        #endregion

        #region File Search

        private static IEnumerable<FileInfo> SearchForFiles(SearchingParams searchingPars)
        {
            var visited = new HashSet<string>();
            return SearchForFiles(searchingPars.Directories.Select(d => new DirectoryInfo(d)).Where(di => di.Exists), searchingPars.Filters, searchingPars.Recursive, visited);
        }

        private static IEnumerable<FileInfo> SearchForFiles(IEnumerable<DirectoryInfo> directories, Strings filters, bool recursive, HashSet<string> visited)
        {
            foreach (var di in directories)
            {
                if (!visited.Contains(di.FullName))
                {
                    foreach (string filter in filters)
                    {
                        foreach (var fi in di.GetFiles(filter))
                        {
                            yield return fi;
                        }
                    }

                    if (recursive)
                    {
                        var sdirs = di.GetDirectories();
                        if (sdirs.Length > 0)
                        {
                            foreach (var sfi in SearchForFiles(sdirs, filters, recursive, visited))
                            {
                                yield return sfi;
                            }
                        }
                    }

                    visited.Add(di.Name);
                }
            }
        } 

        #endregion
    }
}
