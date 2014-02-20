using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WatColor
{
    class Program
    {
        static void Main(string[] args)
        {
            var dir = args.Length != 0 ? new DirectoryInfo(args[0]) : new DirectoryInfo(Environment.CurrentDirectory);

            if (!dir.Exists)
            {
                Console.WriteLine("Directory '{0}' is not exists.", dir);
                return;
            }

            var doc = new XDocument(new XElement("files"));
            foreach (var pngFile in dir.EnumerateFiles("*.png"))
            {
                using (var image = (Bitmap)Image.FromFile(pngFile.FullName))
                {
                    byte r, g, b;
                    GetColor(image, out r, out g, out b);
                    string info = string.Format("{0}\tR:{1} G:{2} B:{3}", pngFile.Name, r, g, b);
                    Console.WriteLine(info);
                    doc.Root.Add(new XElement("file", new XAttribute("name", pngFile.Name), new XAttribute("color", r + "," + g + "," + b)));
                }
            }
            doc.Save(Path.Combine(dir.FullName, "colors.xml"));

            Console.WriteLine("\nPress any...");
            Console.ReadKey();
        }

        private static void GetColor(Bitmap bitmap, out byte r, out byte g, out byte b)
        {
            double rs = 0.0;
            double gs = 0.0;
            double bs = 0.0;
            double count = 0.0;
            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    var px = bitmap.GetPixel(j, i);
                    rs += px.R;
                    gs += px.G;
                    bs += px.B;
                    count++;
                }
            }
            r = ToByte(rs / count);
            g = ToByte(gs / count);
            b = ToByte(bs / count);
        }

        private static byte ToByte(double value)
        {
            value = ToByteRange(value);
            return (byte)Math.Round(value);
        }

        private static double ToByteRange(double value)
        {
            if (value < 0.0) value = 0.0; else if (value > 255.0) value = 255.0;
            return value;
        }
    }
}
