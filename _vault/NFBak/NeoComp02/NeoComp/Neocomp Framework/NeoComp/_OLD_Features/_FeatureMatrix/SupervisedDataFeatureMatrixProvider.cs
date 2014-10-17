using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Computations;

namespace NeoComp.Features
{
    public sealed class SupervisedDataFeatureMatrixProvider : DataFeatureMatrixProvider, ISupervisedFeatureMatrixProvider
    {
        #region Constructors

        public SupervisedDataFeatureMatrixProvider(DataFeatureSelectionStrategy selectionStrategy, IDataFeatureProvider dataFeatureProvider, Strings featureIDs, Strings outputFeatureIDs)
            : base(selectionStrategy, dataFeatureProvider, featureIDs)
        {
            Contract.Requires(selectionStrategy != null);
            Contract.Requires(dataFeatureProvider != null);
            Contract.Requires(featureIDs != null);
            Contract.Requires(outputFeatureIDs != null);

            outputVectorizer = new FeatureSetVectorizer(dataFeatureProvider.FeatureDescriptions.GetSubset(outputFeatureIDs));
            OutputMatrixWidth = outputVectorizer.FeatureDescriptions.Sum(d => d.FeatureValueCount);
        }

        #endregion

        #region Properties

        FeatureSetVectorizer outputVectorizer;

        public int OutputMatrixWidth { get; private set; }

        public Strings OutputFeatureIDs
        {
            get { return new Strings(outputVectorizer.FeatureDescriptions.Select(d => d.ID)); }
        }

        #endregion

        #region Get Next

        public new SupervisedFeatureMatrix GetNext()
        {
            return (SupervisedFeatureMatrix)base.GetNext();
        }

        protected override IEnumerable<FeatureSetVectorizer> GetVectorizers()
        {
            yield return vectorizer;
            yield return outputVectorizer;
        }

        protected override FeatureMatrix ToFeatureMatrix(List<Vector<double>[]> vectors)
        {
            var fm = base.ToFeatureMatrix(vectors);
            var om = GetMatrix(vectors, 1);
            return new SupervisedFeatureMatrix(fm.Matrix, om, fm.Context);
        }

        #endregion
    }
}
