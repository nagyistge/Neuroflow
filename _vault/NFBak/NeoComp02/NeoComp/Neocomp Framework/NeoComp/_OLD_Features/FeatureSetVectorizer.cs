using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using NeoComp.Computations;
using NeoComp.Core;

namespace NeoComp.Features
{
    public sealed class FeatureSetVectorizer : INormalizedVectorizer<FeatureSet>
    {
        #region Constructors

        public FeatureSetVectorizer(params FeatureDescription[] featureDescriptions)
        {
            Contract.Requires(featureDescriptions != null);
            Contract.Requires(featureDescriptions.Length > 0);

            this.featureDescriptions = featureDescriptions.ToArray();
        }

        public FeatureSetVectorizer(IEnumerable<FeatureDescription> featureDescriptions)
        {
            Contract.Requires(featureDescriptions != null);

            this.featureDescriptions = featureDescriptions.ToArray();
            if (this.featureDescriptions.Length == 0) throw new ArgumentException("Feature info collection is empty.", "featureDescriptions");
        } 

        #endregion

        #region Fields And Properties

        FeatureDescription[] featureDescriptions;

        public ReadOnlyCollection<FeatureDescription> FeatureDescriptions
        {
            get { return new ReadOnlyCollection<FeatureDescription>(featureDescriptions); }
        }

        public DoubleRange NormalizationRange
        {
            get { return new DoubleRange(-1.0, 1.0); }
        }

        #endregion

        #region To Vector

        public Vector<double> ToVector(FeatureSet featureSet)
        {
            return new Vector<double>(GetValues(GetFeatures(featureSet)));
        }

        private IEnumerable<double?> GetValues(IEnumerable<Feature> features)
        {
            foreach (var feature in features)
            {
                foreach (var value in feature.GetFeatureValues(NormalizationRange))
                {
                    yield return value;
                }
            }
        }

        private IEnumerable<Feature> GetFeatures(FeatureSet featureCollection)
        {
            foreach (var info in featureDescriptions)
            {
                var feature = featureCollection[info.ID];
                if (feature != null) yield return feature;
            }
        } 

        #endregion

        #region To FeatureSet

        public FeatureSet FromVector(Vector<double> vector)
        {
            return new FeatureSet(GetFeatures(vector));
        }

        private IEnumerable<Feature> GetFeatures(Vector<double> vector)
        {
            var valueEnum = vector.GetEnumerator();
            foreach (var info in featureDescriptions)
            {
                yield return info.CreateFeature(valueEnum, NormalizationRange);
            }
        }

        #endregion

        #region MSE

        public double ComputeMeanSquareError(Vector<double> referenceVector, Vector<double> actualVector)
        {
            return referenceVector.ComputeMeanSquareErrorInternal(actualVector, 0.5);
        }

        public double ComputeMeanSquareError(Matrix<double> referenceMatrix, Matrix<double> actualMatrix)
        {
            return referenceMatrix.ComputeMeanSquareErrorInternal(actualMatrix, 0.5);
        }

        #endregion
    }
}
