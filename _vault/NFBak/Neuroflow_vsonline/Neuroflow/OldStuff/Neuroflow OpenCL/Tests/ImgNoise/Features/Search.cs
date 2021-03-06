﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.IO;
using System.Drawing;
using System.Runtime.Caching;
using Neuroflow.Core;

namespace ImgNoise.Features
{
    internal static class Search
    {
        static readonly string CacheKeyPrefix = Guid.NewGuid().ToString();

        static readonly float maxNoiseLevel = Properties.Settings.Default.MaxNoiseLevel;
        
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
            float noise = 0.0f;
            int sampleIndex = 0;
            foreach (var file in files)
            {
                int size = samplingPars.Size;
                int width = samplingPars.RecurrentSampleLength;
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

                                noise += 0.001f;
                                if (Math.Round(noise, 5) > maxNoiseLevel) noise = 0.0f;
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
            float noise = 0.0f;
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
                                
                                noise += 0.001f;
                                if (Math.Round(noise, 5) > maxNoiseLevel) noise = 0.0f;
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
                string key = CacheKeyPrefix + file.FullName;
                Size? size = MemoryCache.Default[key] as Size?;
                if (size == null)
                {
                    var img = Image.FromFile(file.FullName);
                    size = new Size(img.Width, img.Height);
                    MemoryCache.Default.Add(key, size, new DateTimeOffset(DateTime.UtcNow.AddSeconds(60)));
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

        private static IEnumerable<FileInfo> SearchForFiles(IEnumerable<DirectoryInfo> directories, string[] filters, bool recursive, HashSet<string> visited)
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
