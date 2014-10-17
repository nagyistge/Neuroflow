using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Features
{
    public enum ValidationSamplingMethod { FromStart, FromEnd, Random }
    
    public sealed class ClientValidatorFactory : IValidatorFactory
    {
        #region Wrapper Class

        class Wrapper : IDataFeatureProvider
        {
            public Wrapper(IDataFeatureProvider baseProvider, IEnumerable<int> indexes)
            {
                Contract.Requires(baseProvider != null);
                Contract.Requires(indexes != null);

                this.BaseProvider = baseProvider;
                this.indexes = indexes.OrderBy(i => i).Select((i, idx) => new { I = i, Idx = idx }).ToDictionary(info => info.Idx, info => info.I);
            }

            public IDataFeatureProvider BaseProvider { get; private set; }

            Dictionary<int, int> indexes;

            int IDataFeatureProvider.ItemCount
            {
                get { return indexes.Count; }
            }

            FeatureDescriptionSet IFeatured.FeatureDescriptions
            {
                get { return BaseProvider.FeatureDescriptions; }
            }

            FeatureSet IDataFeatureProvider.this[int index]
            {
                get { return BaseProvider[indexes[index]]; }
            }

            IList<FeatureSet> IDataFeatureProvider.GetItems(int index, int count)
            {
                var result = new FeatureSet[count];
                for (int idx = 0; idx < count; idx++)
                {
                    result[idx] = BaseProvider[indexes[idx + index]];
                }
                return result;
            }

            IList<FeatureSet> IDataFeatureProvider.GetAllItems()
            {
                var result = new FeatureSet[indexes.Count];
                for (int idx = 0; idx < indexes.Count; idx++)
                {
                    result[idx] = BaseProvider[indexes[idx]];
                }
                return result;
            }
        } 

        #endregion

        #region Constructor

        public ClientValidatorFactory(IDataFeatureProvider baseProvider, int numberOfValidationItems, ValidationSamplingMethod method = ValidationSamplingMethod.FromStart)
        {
            Contract.Requires(baseProvider != null);
            Contract.Requires(numberOfValidationItems > 0);

            BaseProvider = baseProvider;
            NumberOfValidationItems = numberOfValidationItems;
            NumberOfTrainingItems = baseProvider.ItemCount - numberOfValidationItems;
            Method = method;

            if (NumberOfTrainingItems <= 0) throw new ArgumentException("Too many validation item count.", "numberOfValidationItems");
        } 

        #endregion

        #region Properties

        public IDataFeatureProvider BaseProvider { get; private set; }

        public int NumberOfTrainingItems { get; private set; }

        public int NumberOfValidationItems { get; private set; }

        public ValidationSamplingMethod Method { get; private set; }

        #endregion

        #region Providers

        public void GetProviders(out IDataFeatureProvider trainingProvider, out IDataFeatureProvider validationProvider)
        {
            if (Method == ValidationSamplingMethod.FromStart)
            {
                validationProvider = new Wrapper(BaseProvider, Enumerable.Range(0, NumberOfValidationItems));
                trainingProvider = new Wrapper(BaseProvider, Enumerable.Range(NumberOfValidationItems, NumberOfTrainingItems));
            }
            else if (Method == ValidationSamplingMethod.FromEnd)
            {
                trainingProvider = new Wrapper(BaseProvider, Enumerable.Range(0, NumberOfTrainingItems));
                validationProvider = new Wrapper(BaseProvider, Enumerable.Range(NumberOfTrainingItems, NumberOfValidationItems));
            }
            else
            {
                var validationIndexes = new HashSet<int>();
                do { validationIndexes.Add(RandomGenerator.Random.Next(BaseProvider.ItemCount)); } while (validationIndexes.Count != NumberOfValidationItems);
                validationProvider = new Wrapper(BaseProvider, validationIndexes);
                trainingProvider = new Wrapper(BaseProvider, Enumerable.Range(0, BaseProvider.ItemCount).Except(validationIndexes));
            }
        } 

        #endregion
    }
}
