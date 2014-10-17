using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.IO;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Features
{
    public enum BinaryItemType { Boolean, Byte, Int16, Int32, Int64, UInt16, UInt32, UInt64, Float, Double }

    // TODO: Error checking.
    // TODO: Remove switches w/ handler classes.
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "binFeatDesc")]
    [Serializable]
    public class BinaryFeatureDescription : FeatureDescription
    {
        #region Constructor

        public BinaryFeatureDescription(string id, BinaryItemType itemType, int itemCount, DoubleRange? itemValueRange = null, object context = null)
            : base(id, EnsureItemValueRange(itemValueRange, itemType).Value, context)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(itemValueRange == null || !itemValueRange.Value.IsFixed);
            Contract.Requires(itemCount > 0);

            ItemType = itemType;
            ItemCount = itemCount;
        }

        public BinaryFeatureDescription(string id, Type arrayType, int arrayLength, DoubleRange? itemValueRange = null, object context = null)
            : this(id, GetItemType(arrayType), arrayLength, itemValueRange, context)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(itemValueRange == null || !itemValueRange.Value.IsFixed);
            Contract.Requires(arrayLength > 0);
            Contract.Requires(arrayType != null);
        }

        private static BinaryItemType GetItemType(Type arrayType)
        {
            if (arrayType.IsArray && arrayType.HasElementType)
            {
                Type etype = arrayType.GetElementType();
                if (etype == typeof(bool)) return BinaryItemType.Boolean;
                if (etype == typeof(byte)) return BinaryItemType.Byte;
                if (etype == typeof(double)) return BinaryItemType.Double;
                if (etype == typeof(float)) return BinaryItemType.Float;
                if (etype == typeof(short)) return BinaryItemType.Int16;
                if (etype == typeof(int)) return BinaryItemType.Int32;
                if (etype == typeof(long)) return BinaryItemType.Int64;
                if (etype == typeof(ushort)) return BinaryItemType.UInt16;
                if (etype == typeof(uint)) return BinaryItemType.UInt32;
                if (etype == typeof(ulong)) return BinaryItemType.UInt64;
            }
            throw new ArgumentException("Array type '" + arrayType + "' is invalid or unknown.", "arrayType");
        }

        private static DoubleRange? EnsureItemValueRange(DoubleRange? itemValueRange, BinaryItemType itemType)
        {
            Contract.Ensures(Contract.Result<DoubleRange?>() != null);

            if (itemValueRange == null)
            {
                switch (itemType)
                {
                    case BinaryItemType.Boolean:
                        return new DoubleRange(0.0, 1.0);
                    case BinaryItemType.Byte:
                        return new DoubleRange(0.0, byte.MaxValue);
                    case BinaryItemType.Int16:
                        return new DoubleRange(short.MinValue, short.MaxValue);
                    case BinaryItemType.Int32:
                        return new DoubleRange(int.MinValue, int.MaxValue);
                    case BinaryItemType.Int64:
                        return new DoubleRange(long.MinValue, long.MaxValue);
                    case BinaryItemType.UInt16:
                        return new DoubleRange(ushort.MinValue, ushort.MaxValue);
                    case BinaryItemType.UInt32:
                        return new DoubleRange(uint.MinValue, uint.MaxValue);
                    case BinaryItemType.UInt64:
                        return new DoubleRange(ulong.MinValue, ulong.MaxValue);
                    case BinaryItemType.Float:
                        return new DoubleRange(float.MinValue, float.MaxValue);
                    case BinaryItemType.Double:
                        return new DoubleRange(double.MinValue, double.MaxValue);
                }
            }
            else
            {
                switch (itemType)
                {
                    case BinaryItemType.Boolean:
                        return Bordered(itemValueRange.Value, new DoubleRange(0.0, 1.0));
                    case BinaryItemType.Byte:
                        return Bordered(itemValueRange.Value, new DoubleRange(0.0, byte.MaxValue));
                    case BinaryItemType.Int16:
                        return Bordered(itemValueRange.Value, new DoubleRange(short.MinValue, short.MaxValue));
                    case BinaryItemType.Int32:
                        return Bordered(itemValueRange.Value, new DoubleRange(int.MinValue, int.MaxValue));
                    case BinaryItemType.Int64:
                        return Bordered(itemValueRange.Value, new DoubleRange(long.MinValue, long.MaxValue));
                    case BinaryItemType.UInt16:
                        return Bordered(itemValueRange.Value, new DoubleRange(ushort.MinValue, ushort.MaxValue));
                    case BinaryItemType.UInt32:
                        return Bordered(itemValueRange.Value, new DoubleRange(uint.MinValue, uint.MaxValue));
                    case BinaryItemType.UInt64:
                        return Bordered(itemValueRange.Value, new DoubleRange(ulong.MinValue, ulong.MaxValue));
                    case BinaryItemType.Float:
                        return Bordered(itemValueRange.Value, new DoubleRange(float.MinValue, float.MaxValue));
                    case BinaryItemType.Double:
                        return Bordered(itemValueRange.Value, new DoubleRange(double.MinValue, double.MaxValue));
                }
            }
            throw new ArgumentException(itemType + " is unknown.", "itemType"); 
        }

        private static DoubleRange Bordered(DoubleRange range, DoubleRange borderRange)
        {
            double min = range.MinValue, max = range.MaxValue;
            if (min < borderRange.MinValue) min = borderRange.MinValue;
            if (max > borderRange.MaxValue) max = borderRange.MaxValue;
            return new DoubleRange(min, max);
        }

        #endregion

        #region Properties and Fields

        [DataMember(Name = "type")]
        public BinaryItemType ItemType { get; private set; }

        public DoubleRange ItemValueRange
        {
            get { return OriginalValueRange; }
        }

        [DataMember(Name = "count")]
        public int ItemCount { get; private set; }

        public override int OriginalValueCount
        {
            get { return ItemCount; }
        }

        public override int FeatureValueCount
        {
            get { return ItemCount; }
        } 

        #endregion

        #region Get Values

        protected internal override IEnumerable<double?> GetOriginalValues(Feature feature)
        {
            return GetValues(feature);
        }

        private IEnumerable<double?> GetValues(Feature feature)
        {
            return GetValues(feature);
        }

        protected internal override IEnumerable<double?> GetFeatureValues(Feature feature)
        {
            var bytes = ((BinaryFeature)feature).Bytes;
            if (bytes == null)
            {
                for (int idx = 0; idx < ItemCount; idx++) yield return null;
            }
            else
            {
                using (var reader = new BinaryReader(new MemoryStream(bytes)))
                {
                    for (int idx = 0; idx < ItemCount; idx++) yield return ReadItem(reader);
                }
            }
        }

        private double ReadItem(BinaryReader reader)
        {
            double value = 0.0;
            switch (ItemType)
            {
                case BinaryItemType.Boolean:
                    value = reader.ReadBoolean() ? 1.0 : 0.0;
                    break;
                case BinaryItemType.Byte:
                    value = reader.ReadByte();
                    break;
                case BinaryItemType.Int16:
                    value = reader.ReadInt16();
                    break;
                case BinaryItemType.Int32:
                    value = reader.ReadInt32();
                    break;
                case BinaryItemType.Int64:
                    value = reader.ReadInt64();
                    break;
                case BinaryItemType.UInt16:
                    value = reader.ReadUInt16();
                    break;
                case BinaryItemType.UInt32:
                    value = reader.ReadUInt32();
                    break;
                case BinaryItemType.UInt64:
                    value = reader.ReadUInt64();
                    break;
                case BinaryItemType.Float:
                    value = reader.ReadSingle();
                    break;
                case BinaryItemType.Double:
                    value = reader.ReadDouble();
                    break;
            }
            return OriginalValueRange.Cut(value);
        } 

        #endregion

        #region Create

        public override Feature CreateFeature(object value)
        {
            if (value != null)
            {
                var bytes = value as byte[];
                if (bytes != null) return new BinaryFeature(this, bytes);

                var array = value as Array;
                if (array != null) return CreateFromArray(array);
            }
            return new BinaryFeature(this, null);
        }

        private Feature CreateFromArray(Array array)
        {
            int count = Buffer.ByteLength(array);
            byte[] bytes = new byte[count];
            Buffer.BlockCopy(array, 0, bytes, 0, count);
            return new BinaryFeature(this, bytes);
        }

        protected internal override Feature CreateFeature(IEnumerator<double?> valueEnumerator, DoubleRange valueNormalizationRange)
        {
            byte[] bytes;
            MemoryStream ms;
            using (var writer = new BinaryWriter(ms = new MemoryStream()))
            {
                for (int idx = 0; idx < ItemCount; idx++)
                {
                    double? value = GetNext(valueEnumerator);
                    Write(writer, value, valueNormalizationRange);
                }
                bytes = ms.ToArray();
            }
            return new BinaryFeature(this, bytes);
        }

        private void Write(BinaryWriter writer, double? value, DoubleRange valueNormalizationRange)
        {
            if (value == null)
            {
                WriteDefault(writer);
            }
            else
            {
                double currentValue = OriginalValueRange.Denormalize(valueNormalizationRange.Cut(value.Value), valueNormalizationRange);
                switch (ItemType)
                {
                    case BinaryItemType.Boolean:
                        writer.Write(currentValue >= 0.5);
                        return;
                    case BinaryItemType.Byte:
                        writer.Write((byte)Math.Round(currentValue));
                        return;
                    case BinaryItemType.Int16:
                        writer.Write((short)Math.Round(currentValue));
                        return;
                    case BinaryItemType.Int32:
                        writer.Write((int)Math.Round(currentValue));
                        return;
                    case BinaryItemType.Int64:
                        writer.Write((long)Math.Round(currentValue));
                        return;
                    case BinaryItemType.UInt16:
                        writer.Write((ushort)Math.Round(currentValue));
                        return;
                    case BinaryItemType.UInt32:
                        writer.Write((uint)Math.Round(currentValue));
                        return;
                    case BinaryItemType.UInt64:
                        writer.Write((ulong)Math.Round(currentValue));
                        return;
                    case BinaryItemType.Float:
                        writer.Write((float)currentValue);
                        return;
                    case BinaryItemType.Double:
                        writer.Write(currentValue);
                        return;
                }
            }
        }

        private void WriteDefault(BinaryWriter writer)
        {
            switch (ItemType)
            {
                case BinaryItemType.Boolean:
                    writer.Write(default(bool));
                    return;
                case BinaryItemType.Byte:
                    writer.Write(default(byte));
                    return;
                case BinaryItemType.Int16:
                    writer.Write(default(short));
                    return;
                case BinaryItemType.Int32:
                   writer.Write(default(int));
                    return;
                case BinaryItemType.Int64:
                    writer.Write(default(long));
                    return;
                case BinaryItemType.UInt16:
                    writer.Write(default(ushort));
                    return;
                case BinaryItemType.UInt32:
                    writer.Write(default(uint));
                    return;
                case BinaryItemType.UInt64:
                    writer.Write(default(ulong));
                    return;
                case BinaryItemType.Float:
                    writer.Write(default(float));
                    return;
                case BinaryItemType.Double:
                    writer.Write(default(double));
                    return;
            }
        } 

        #endregion
    }
    
    public class BinaryFeature : Feature
    {
        internal BinaryFeature(BinaryFeatureDescription description, byte[] bytes)
            : base(description)
        {
            Contract.Requires(description != null);

            Bytes = bytes;
        }

        public byte[] Bytes { get; private set; }

        new public BinaryFeatureDescription Description
        {
            get { return (BinaryFeatureDescription)base.Description; }
        }

        public override object Data
        {
            get { return Bytes; }
        }
    }
}
