using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using System.Xml;
using System.Runtime.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using ImgNoise.Features;
using System.Threading;
using System.Threading.Tasks;
using NeoComp.Imaging;
using NeoComp.DEBUG;
using System.Diagnostics;
using NeoComp.Networks.Computational;

namespace ImgNoise.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CodeBench.Named("1").Do = () => RemoveNoise(LoadComputation("comp.xml"), @"C:\Users\unbornchikken\Pictures\NN\Noised\croft.png", 0.01);
                Console.WriteLine("All: {0}ms", CodeBench.Named("1").LastMS);
                Console.WriteLine("Neural: {0}ms", TEMP.msTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.GetType().Name);
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }

        private static NeuralComputation LoadComputation(string path)
        {
            using (var xml = XmlReader.Create(path))
            {
                var s = new DataContractSerializer(typeof(NeuralComputation));
                return (NeuralComputation)s.ReadObject(xml);
            }
        }

        private static void RemoveNoise(NeuralComputation computation, string path, double noiseLevel)
        {
            var comps = Enumerable.Range(0, 15).Select(idx => new NoiseRemovalComputation(computation.Clone())).ToList();
            comps.Add(new NoiseRemovalComputation(computation));
            int compIdx = 0;
            
            ImageBuffer source, dest;
            using (var sourceBmp = new Bitmap(path))
            {
                source = new ImageBuffer(sourceBmp);
                AddNoise(source, noiseLevel);
                dest = new ImageBuffer(source.Width, source.Height);
            }

            int size = 7, blockSize = 16;
            int w = source.Width, h = source.Height, soFar = 0, msTime = 0;
            SpinLock infoLock = new SpinLock(), compLock = new SpinLock();

            Parallel.For(0, h / blockSize + 1, (idx, state) =>
            {
                NoiseRemovalComputation comp;
                bool compTaken = false;
                try
                {
                    compLock.Enter(ref compTaken);
                    comp = comps[compIdx++ % comps.Count];
                }
                finally
                {
                    if (compTaken) compLock.Exit();
                }

                //Console.WriteLine(compIdx);

                int pos = idx * blockSize;
                var rBytes = new byte[size * size];
                var bBytes = new byte[size * size];
                var gBytes = new byte[size * size];
                Color[] sourceColors = new Color[size * size];
                for (int y = pos; y < pos + blockSize && y < h; y++)
                {
                    var sw = new Stopwatch();
                    for (int x = 0; x < w; x++)
                    {
                        ReadBytes(source, rBytes, gBytes, bBytes, size, x, y);

                        sw.Start();
                        byte destByteR = comp.Compute(rBytes);
                        byte destByteG = comp.Compute(gBytes);
                        byte destByteB = comp.Compute(bBytes);
                        sw.Stop();

                        WriteByte(dest, x, y, destByteR, destByteG, destByteB);
                    }

                    bool infoTaken = false;
                    try
                    {
                        infoLock.Enter(ref infoTaken);
                        msTime += (int)sw.ElapsedMilliseconds;
                        Console.WriteLine("Processing line: {0} / {1}", ++soFar, h);
                    }
                    finally
                    {
                        if (infoTaken) infoLock.Exit();
                    }
                }
            });

            Console.WriteLine("Comp: {0}ms", msTime);

            dest.Save("result.png", ImageFormat.Png);  
        }

        private static void AddNoise(ImageBuffer source, double noiseLevel)
        {
            Parallel.For(0, source.Height, (y) =>
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var px = source.SafeGetPixel(x, y);
                    byte r = Noise.Add(px.R, noiseLevel), g = Noise.Add(px.G, noiseLevel), b = Noise.Add(px.B, noiseLevel);
                    source.SafeSetPixel(x, y, Color.FromArgb(r, g, b));
                }
            });
            source.Save("noised.png", ImageFormat.Png); 
        }

        private static void GetChannel(Color[] colors, byte[] bytes, Channel channel)
        {
            for (int idx = 0; idx < bytes.Length; idx++) bytes[idx] = GetChannel(colors[idx], channel);
        }

        private static byte GetChannel(Color color, Channel channel)
        {
            switch (channel)
            {
                case Channel.Blue:
                    return color.B;
                case Channel.Green:
                    return color.G;
                default:
                    return color.R;
            }
        }

        private static void WriteByte(ImageBuffer dest, int x, int y, byte r, byte g, byte b)
        {
            if (x < dest.Width && y < dest.Height)
            {
                dest.SafeSetPixel(x, y, Color.FromArgb(r, g, b));
            }
        }

        private static void ReadBytes(ImageBuffer source, byte[] rBytes, byte[] gBytes, byte[] bBytes, int size, int x, int y)
        {
            int size2 = size / 2;
            int xs = x - size2, xe = x + size2 + 1;
            int ys = y - size2, ye = y + size2 + 1;
            int dx = 0, dy = 0;
            for (int ry = ys; ry < ye; ry++, dy++)
            {
                for (int rx = xs; rx < xe; rx++, dx++)
                {
                    int addr = dy * size + dx;
                    var color = ReadColor(source, rx, ry);
                    rBytes[addr] = color.R;
                    gBytes[addr] = color.G;
                    bBytes[addr] = color.B;
                }
                dx = 0;
            }
        }

        private static Color ReadColor(ImageBuffer source, int x, int y)
        {
            x = CutCoord(source.Width, x);
            y = CutCoord(source.Height, y);
            return source.SafeGetPixel(x, y);
        }

        private static int CutCoord(int size, int pos)
        {
            if (pos < 0) return -pos;
            if (pos >= size) return (2 * size) - pos - 1;
            return pos;
        }
    }
}
