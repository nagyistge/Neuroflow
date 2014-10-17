using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LeCun.Features;

namespace LeCunConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AddData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("Press any ...");
            Console.ReadKey();
        }

        private static void AddData()
        {
            string lblFile = @"c:\Downloads\minist\t10k-labels.idx1-ubyte";
            string dataFile = @"c:\Downloads\minist\t10k-images.idx3-ubyte";
            bool isTraining = false;

            using (var lblStream = File.OpenRead(lblFile))
            using (var dataStream = File.OpenRead(dataFile))
            using (var lblReader = new BinaryReader2(lblStream, false))
            using (var dataReader = new BinaryReader2(dataStream, false))
            {
                AddData(lblReader, dataReader, isTraining);
            }
        }

        private static void AddData(BinaryReader lblReader, BinaryReader dataReader, bool isTraining)
        {
            // Skip magic:
            lblReader.ReadInt32();
            dataReader.ReadInt32();

            int count = lblReader.ReadInt32();
            if (dataReader.ReadInt32() != count) throw new InvalidOperationException("Counts not match!");

            int rows = dataReader.ReadInt32();
            int cols = dataReader.ReadInt32();

            if (rows != 28 || cols != 28) throw new InvalidOperationException("Invalid image size!");

            Console.WriteLine("Reading: " + count + (isTraining ? " Training" : " Validation") + " samples.");

            int packSize = 1000;
            for (int idx = 0; idx < count; idx+= packSize)
            {
                Console.WriteLine(idx);
                var pack = ReadSamples(lblReader, dataReader, isTraining, packSize, idx, count);
                using (var ctx = new LeCunDataEntities())
                {
                    foreach (var sample in pack)
                    {
                        ctx.Samples.AddObject(sample);
                    }
                    ctx.SaveChanges();
                }
            }

            Console.WriteLine("Done.");
        }

        private static ICollection<Sample> ReadSamples(BinaryReader lblReader, BinaryReader dataReader, bool isTraining, int packSize, int index, int count)
        {
            var list = new LinkedList<Sample>();

            for (int i = index; i < count && i < (index + packSize); i++)
            {
                list.AddLast(
                    new Sample
                    {
                        IsTraining = isTraining,
                        Number = lblReader.ReadByte(),
                        ImageData = dataReader.ReadBytes(28 * 28)
                    });
            }

            return list;
        }
    }
}
