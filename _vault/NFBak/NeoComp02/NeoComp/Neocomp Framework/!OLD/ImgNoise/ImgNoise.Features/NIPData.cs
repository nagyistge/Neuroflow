using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Features;
using NeoComp.Core;
using System.Drawing;

namespace ImgNoise.Features
{
    public sealed class NIPData : IInitializableFeaturedObject, IFeatureDescriptionOverride
    {
        #region Constructor

        public NIPData(NoisedImagePartReader reader)
        {
            Contract.Requires(reader != null);

            Reader = reader;
        } 

        #endregion

        #region Properties

        public NoisedImagePartReader Reader { get; private set; }

        public byte Pixel { get; private set; } 

        public byte[] NoisedImageData { get; private set; } 

        #endregion

        #region Init

        void IInitializableFeaturedObject.Initialize(string featureID)
        {
            switch (featureID)
            {
                case "Pixel":
                    Pixel = GetPixel(Reader.Read());
                    break;
                case "NoisedImageData":
                    NoisedImageData = Reader.ReadNoised();
                    break;
                case null:
                case "":
                    {
                        byte[] id, nid;
                        Reader.ReadNoised(out id, out nid);
                        Pixel = GetPixel(id);
                        NoisedImageData = nid;
                    }
                    break;
            }
        }

        void IInitializableFeaturedObject.Uninitialize()
        {
            NoisedImageData = null;
        } 

        #endregion

        #region Stuff

        private byte GetPixel(byte[] bytes)
        {
            int half = Reader.Size / 2;
            return bytes[half * Reader.Size + half];
            //return bytes[bytes.Length - 1];
        }

        #endregion

        #region Override

        System.Collections.IList IFeatureDescriptionOverride.GetDistinctValues(string featureID)
        {
            return null;
        }

        DoubleRange? IFeatureDescriptionOverride.GetRange(string featureID)
        {
            if (featureID == "Reader.NoiseLevel") return new DoubleRange(0.0, Properties.Settings.Default.MaxNoiseLevel);
            if (featureID == "Pixel") return new DoubleRange(0.0, 255.0);
            return null;
        }

        int? IFeatureDescriptionOverride.GetItemCount(string featureID)
        {
            if (featureID == "NoisedImageData") return Reader.Size * Reader.Size;
            return null;
        } 

        #endregion
    }
}
