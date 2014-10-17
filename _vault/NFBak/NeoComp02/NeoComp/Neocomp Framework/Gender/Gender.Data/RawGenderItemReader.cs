using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.Contracts;

namespace Gender.Data
{
    public sealed class RawGenderItemReader
    {
        public RawGenderItemReader(Stream baseStream, bool isTrainingPatterns)
        {
            Contract.Requires(baseStream != null);
            
            reader = new BinaryReader(baseStream);
            IsTrainingPatterns = isTrainingPatterns;
            Reset();
        }

        BinaryReader reader;

        public bool IsTrainingPatterns { get; private set; }

        public int ItemCount { get; private set; }

        private void Initialize()
        {
            int count = reader.ReadInt32();
            int itemSize = reader.ReadInt32();
            ItemCount = count;
        }

        public void Reset()
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            Initialize();
        }

        public IEnumerable<Item> ReadItems()
        {
            Contract.Ensures(Contract.Result<IEnumerable<Item>>() != null);

            for (int idx = 0; idx < ItemCount; idx++)
            {
                byte[] pixels = reader.ReadBytes(20 * 24);
                float mean = reader.ReadSingle();
                float belowMean = reader.ReadSingle();
                float aboveMean = reader.ReadSingle();
                float gender = reader.ReadSingle();
                byte[] comment = reader.ReadBytes(400);
                yield return new Item
                {
                    Pixels = pixels,
                    Mean = mean,
                    AboveMean = aboveMean,
                    BelowMean = belowMean,
                    Gender = gender == 1.0,
                    IsTrainingPattern = this.IsTrainingPatterns
                };
            }
        }
    }
}
