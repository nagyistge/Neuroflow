using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace NeoComp.Features
{
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name= "featuredComp")]
    [Serializable]
    [KnownType(typeof(ValueFeatureDescription))]
    [KnownType(typeof(SetFeatureDescription))]
    [KnownType(typeof(BinaryFeatureDescription))]
    public class FeaturedComputation : IFeaturedInputOutput, ICloneable
    {
        #region Constructor

        public FeaturedComputation(IComputationalUnit<double?> unit, int numberOfIterations = 1, object featuredObject = null)
        {
            Contract.Requires(unit != null);
            Contract.Requires(numberOfIterations >= 1);

            Strings inIDs, outIDs;
            ExtractFeatureInformation(featuredObject, unit.InputInterface.Length, unit.OutputInterface.Length, out desc, out inIDs, out outIDs);

            Unit = unit;
            InputFeatureIDs = inIDs;
            OutputFeatureIDs = outIDs;
            NumberOfIterations = numberOfIterations;
            Initialize();
        }

        #endregion

        #region Properties and Fields

        [NonSerialized]
        Learningutation<double> computation;

        [NonSerialized]
        FeatureSetVectorizer inputVectorizer;

        [NonSerialized]
        FeatureSetVectorizer outputVectorizer;

        [NonSerialized]
        Dictionary<string, int> inputIDLookup;

        [DataMember(Name = "descriptions")]
        FeatureDescription[] desc;

        public ReadOnlyCollection<FeatureDescription> FeatureDescriptions
        {
            get { return new ReadOnlyCollection<FeatureDescription>(((IList<FeatureDescription>)desc)); }
        }

        [DataMember(Name = "inIDs")]
        public Strings InputFeatureIDs { get; private set; }

        [DataMember(Name = "outIDs")]
        public Strings OutputFeatureIDs { get; private set; }

        [DataMember(Name = "unit")]
        public IComputationalUnit<double?> Unit { get; private set; }

        [DataMember(Name = "numIts")]
        public int NumberOfIterations { get; private set; } 

        #endregion

        #region Info Extract

        private static void ExtractFeatureInformation(object featuredObject, int iiLen, int oiLen, out FeatureDescription[] descriptions, out Strings inIDs, out Strings outIDs)
        {
            var ofo = featuredObject as IFeaturedInputOutput;
            if (ofo != null)
            {
                inIDs = ofo.InputFeatureIDs;
                outIDs = ofo.OutputFeatureIDs;
                descriptions = ofo.FeatureDescriptions.ToArray();
                return;
            }

            var ifo = featuredObject as IFeaturedInput;
            if (ifo != null)
            {
                inIDs = ifo.InputFeatureIDs;
                var set = ifo.FeatureDescriptions; // Mutable.
                outIDs = GenerateFeatureDescriptions(set, oiLen);
                descriptions = set.ToArray();
                return;
            }

            var newSet = new FeatureDescriptionSet();
            inIDs = GenerateFeatureDescriptions(newSet, iiLen);
            outIDs = GenerateFeatureDescriptions(newSet, oiLen);
            descriptions = newSet.ToArray();
        }

        private static Strings GenerateFeatureDescriptions(FeatureDescriptionSet set, int count)
        {
            string[] ids = new string[count];
            for (int idx = 0; idx < count; idx++)
            {
                string fid = "Port_" + idx;
                ids[idx] = fid;
                set.Add(new ValueFeatureDescription(fid, new DoubleRange(0.0, 1.0)));
            }
            return new Strings(ids);
        }

        #endregion

        #region Initalize

        private void EnsureInitialization()
        {
            if (computation == null)
            {
                lock (desc)
                {
                    if (computation == null) Initialize();
                }
            }
        }

        private void Initialize()
        {
            var set = new FeatureDescriptionSet(desc);
            FeatureDescriptionSet inputSet = null;
            FeatureDescriptionSet outputSet = null;

            try
            {
                inputSet = set.GetSubset(InputFeatureIDs);
                outputSet = set.GetSubset(OutputFeatureIDs);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot generate feature subset. See inner exception for details.", ex);
            }
            
            inputVectorizer = new FeatureSetVectorizer(inputSet);
            outputVectorizer = new FeatureSetVectorizer(outputSet);

            if (inputVectorizer.FeatureDescriptions.Sum(d => d.FeatureValueCount) != Unit.InputInterface.Length)
            {
                throw new InvalidOperationException("Invalid number of input features.");
            }

            if (outputVectorizer.FeatureDescriptions.Sum(d => d.FeatureValueCount) != Unit.OutputInterface.Length)
            {
                throw new InvalidOperationException("Invalid number of output features.");
            }

            inputIDLookup = CreateLookup(InputFeatureIDs);

            if (InputFeatureIDs.Count + OutputFeatureIDs.Count < set.Count)
            {
                desc = Cut(set, InputFeatureIDs.Concat(OutputFeatureIDs));
            }

            computation = new Learningutation<double>(NumberOfIterations);
        }

        private FeatureDescription[] Cut(FeatureDescriptionSet set, IEnumerable<string> exceptIDs)
        {
            return exceptIDs.Select(id => set[id]).ToArray();
        }

        private Dictionary<string, int> CreateLookup(Strings ids)
        {
            var result = new Dictionary<string, int>();
            for (int idx = 0; idx < ids.Count; idx++)
            {
                string id = ids[idx];
                result.Add(id, idx);
            }
            return result;
        }

        #endregion

        #region Compute

        public IDictionary<string, object> Compute(IDictionary<string, object> inputParameters)
        {
            Contract.Requires(inputParameters != null);
            Contract.Ensures(Contract.Result<IDictionary<string, object>>() != null);
            Contract.Ensures(Contract.Result<IDictionary<string, object>>().Count == OutputFeatureIDs.Count);

            EnsureInitialization();

            object[] pars = new object[inputIDLookup.Count];
            int idx = 0;
            foreach (var lkvp in inputIDLookup)
            {
                object par;
                if (inputParameters.TryGetValue(lkvp.Key, out par))
                {
                    pars[idx] = par;
                }
                idx++;
            }

            var result = new Dictionary<string, object>();
            
            object[] outputs = ComputeOutput(pars);
            for (idx = 0; idx < outputs.Length; idx++)
            {
                object output = outputs[idx];
                result.Add(OutputFeatureIDs[idx], output);
            }

            return result;
        }

        public object[] Compute(params object[] inputParameters)
        {
            Contract.Requires(inputParameters != null);
            Contract.Requires(inputParameters.Length == InputFeatureIDs.Count);
            Contract.Ensures(Contract.Result<object[]>() != null);
            Contract.Ensures(Contract.Result<object[]>().Length == OutputFeatureIDs.Count);
            
            EnsureInitialization();

            return ComputeOutput(inputParameters);
        }

        private object[] ComputeOutput(object[] inputParameters)
        {
            Contract.Requires(inputParameters != null);
            Contract.Requires(inputParameters.Length == InputFeatureIDs.Count);
            Contract.Ensures(Contract.Result<object[]>() != null);
            Contract.Ensures(Contract.Result<object[]>().Length == OutputFeatureIDs.Count);
            
            var inputFeatures = new FeatureSet(inputVectorizer.FeatureDescriptions.Select((d, idx) => d.CreateFeature(inputParameters[idx])));
            var inputVector = inputVectorizer.ToVector(inputFeatures);
            var outputVector = computation.Compute(Unit, inputVector);
            var outputFeatures = outputVectorizer.FromVector(outputVector);
            var outputValues = outputFeatures.Select(f => f.Data).ToArray();

            return outputValues;
        }

        #endregion

        #region IFeatured

        FeatureDescriptionSet IFeatured.FeatureDescriptions
        {
            get { return new FeatureDescriptionSet(desc); }
        }

        #endregion

        #region Clone

        public FeaturedComputation Clone()
        {
            return CloneHelper.Clone(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        } 

        #endregion
    }
}
