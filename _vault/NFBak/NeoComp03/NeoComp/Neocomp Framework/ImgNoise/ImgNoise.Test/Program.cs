using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.DEBUG;
using NeoComp.Networks.Computational.Neural;
using System.Xml;
using System.Runtime.Serialization;
using NeoComp.Imaging;
using System.Threading.Tasks;
using ImgNoise.Features;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImgNoise.Test
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RemoveNoise(LoadNetwork("comp.xml"), @"C:\Users\unbornchikken\Pictures\NN\Noised\croft.png", 0.1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.GetType().Name);
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }

        private static NeuralNetwork LoadNetwork(string path)
        {
            using (var xml = XmlReader.Create(path))
            {
                var s = new DataContractSerializer(typeof(NeuralNetwork));
                return (NeuralNetwork)s.ReadObject(xml);
            }
        }

        private static void RemoveNoise(NeuralNetwork neuralNetwork, string imagePath, double noiseLevel)
        {
            ImageBuffer source, dest;
            using (var sourceBmp = new Bitmap(imagePath))
            {
                source = new ImageBuffer(sourceBmp);
                AddNoise(source, noiseLevel);
                dest = new ImageBuffer(source.Width, source.Height);
            }

            var rem = new RecurrentNoiseRemoval(neuralNetwork, source, noiseLevel);
            rem.RemoveNoise((row) => Console.WriteLine("Processing: {0}/{1}", row, source.Height));

            Console.WriteLine("Remove Time: {0}ms, Computation Time: {1}ms", rem.LastRemoveTime.TotalMilliseconds, rem.LastComputationTime.TotalMilliseconds);

            rem.Dest.Save("result.png", ImageFormat.Png); 
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
    }
}
