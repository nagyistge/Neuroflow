using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using System.Diagnostics;
using NeoComp.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace NeoComp.Features
{
    public class DataFeatureMatrixProvider : SynchronizedObject, IDataFeatureMatrixProvider
    {
        #region Constructors

        // TODO: Configurable Cache!        
        public DataFeatureMatrixProvider(DataFeatureSelectionStrategy selectionStrategy, IDataFeatureProvider dataFeatureProvider, Strings featureIDs = null)
        {
            Contract.Requires(selectionStrategy != null);
            Contract.Requires(dataFeatureProvider != null);
            
            this.selectionStrategy = selectionStrategy;
            DataFeatureProvider = dataFeatureProvider;
            vectorizer = new FeatureSetVectorizer(featureIDs == null ? dataFeatureProvider.FeatureDescriptions : dataFeatureProvider.FeatureDescriptions.GetSubset(featureIDs));
            MatrixWidth = vectorizer.FeatureDescriptions.Sum(d => d.FeatureValueCount);
        }

        #endregion

        #region Fields and Properties

        FeatureMatrix current;

        int vectorizerCount;

        internal readonly FeatureSetVectorizer vectorizer;

        string uid = Guid.NewGuid().ToString();

        private DataFeatureSelectionStrategy selectionStrategy;

        public IDataFeatureProvider DataFeatureProvider { get; private set; }

        public Strings FeatureIDs
        {
            get { return new Strings(vectorizer.FeatureDescriptions.Select(d => d.ID)); }
        }

        bool initialized;

        public bool Initialized
        {
            get { lock (SyncRoot) return initialized; }
        }

        public int MatrixWidth { get; private set; }

        #endregion

        #region Initialize

        public void Initialize()
        {
            if (!initialized)
            {
                lock (SyncRoot)
                {
                    InitializeLocked();
                }
            }
        }

        private void InitializeLocked()
        {
            if (!initialized)
            {
                DoInitialization();
                initialized = true;
                current = null;
                vectorizerCount = GetVectorizers().Count();
            }
        }

        protected virtual void DoInitialization()
        {
            InititalizeStrategy();
        }

        private void InititalizeStrategy()
        {
            selectionStrategy.Initialize(this);
        }

        public void Uninitialize()
        {
            if (initialized)
            {
                lock (SyncRoot)
                {
                    if (initialized)
                    {
                        DoUninitialization();
                        initialized = false;
                        current = null;
                    }
                }
            }
        }

        protected virtual void DoUninitialization()
        {
            UninitializeStrategy();
        }

        private void UninitializeStrategy()
        {
            selectionStrategy.Uninitialize();
        }

        #endregion

        #region Get Next

        public FeatureMatrix GetNext()
        {
            lock (SyncRoot)
            {
                // Ensure initialization
                InitializeLocked();

                var nextIndexes = selectionStrategy.GetNextIndexes();

                if (nextIndexes == null || nextIndexes.Value.IsEmpty)
                {
                    if (current == null)
                    {
                        throw new InvalidOperationException(selectionStrategy + " has returned no indexes.");
                    }
                    return current;
                }
                else
                {
                    return current = GetFeatureMatrixInternal(nextIndexes.Value);
                }
            }
        }

        public FeatureMatrix GetFeatureMatrix(FeatureIndexSet indexes)
        {
            Contract.Requires(!indexes.IsEmpty);
            Contract.Ensures(Contract.Result<FeatureMatrix>() != null);

            lock (SyncRoot)
            {
                // Ensure initialization
                InitializeLocked();

                return GetFeatureMatrixInternal(indexes);
            }
        }

        private FeatureMatrix GetFeatureMatrixInternal(FeatureIndexSet indexes)
        {
            Contract.Requires(!indexes.IsEmpty);

            var vectors = new List<Vector<double>[]>(indexes.Count);
            var cache = VectorCache.Cache;

            if (cache == null)
            {
                GetFeatureMatrixNoCache(indexes, vectors);
            }
            else
            {
                GetFeatureMatrixCache(indexes, vectors, cache);
            }

            if (indexes.Randomize) vectors = vectors.OrderByRandom().ToList();

            return ToFeatureMatrix(vectors);
        }

        // Cache
        private void GetFeatureMatrixCache(FeatureIndexSet indexes, List<Vector<double>[]> vectors, ICache cache)
        {
            //var sl = new SpinLock();
            //Parallel.ForEach(indexes, (index) =>
            //{
            //    string key = index.ToString() + uid;
            //    var cv = cache[key] as Vector<double>[];

            //    if (cv == null)
            //    {
            //        cv = GetFeatureVectorsOf(RetrieveFeatureSet(index));
            //        cache.Add(key, cv);
            //    }

            //    bool taken = false;
            //    try
            //    {
            //        sl.Enter(ref taken);
            //        vectors.Add(cv);
            //    }
            //    finally
            //    {
            //        if (taken) sl.Exit();
            //    }
            //});

            foreach (int index in indexes)
            {
                string key = index.ToString() + uid;
                var cv = cache[key] as Vector<double>[];

                if (cv != null)
                {
                    vectors.Add(cv);
                }
                else
                {
                    cv = GetFeatureVectorsOf(RetrieveFeatureSet(index));
                    cache.Add(key, cv);
                    vectors.Add(cv);
                }
            }
        }

        // No Cache
        private void GetFeatureMatrixNoCache(FeatureIndexSet indexes, List<Vector<double>[]> vectors)
        {
            IList<FeatureSet> list = null;

            if (indexes.Indexes != null)
            {
                list = new List<FeatureSet>(indexes.Indexes.Count);
                foreach (int idx in indexes.Indexes)
                {
                    try
                    {
                        list.Add(DataFeatureProvider[idx]);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Cannot retrive item '" + idx + "' from " + DataFeatureProvider + ". See inner exception for details.", ex);
                    }
                }
            }
            else // if (indexes.IndexRange != null)
            {
                try
                {
                    list = DataFeatureProvider.GetItems(indexes.IndexRange.Value.MinValue, indexes.IndexRange.Value.MaxValue + 1);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Cannot retrive items '" + indexes.IndexRange + "' from " + DataFeatureProvider + ". See inner exception for details.", ex);
                }
            }

            Debug.Assert(list.Count == vectors.Capacity);

            foreach (var set in list) vectors.Add(GetFeatureVectorsOf(set));
        }

        private FeatureSet RetrieveFeatureSet(int index)
        {
            try
            {
                return DataFeatureProvider[index];
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot retrive item '" + index + "' from " + DataFeatureProvider + ". See inner exception for details.", ex);
            }
        }

        private Vector<double>[] GetFeatureVectorsOf(FeatureSet featureSet)
        {
            var vectors = new Vector<double>[vectorizerCount];

            int idx = 0;
            foreach (var vectorizer in GetVectorizers())
            {
                vectors[idx++] = vectorizer.ToVector(featureSet);
            }

            return vectors;
        }

        protected virtual IEnumerable<FeatureSetVectorizer> GetVectorizers()
        {
            //Contract.Ensures(Contract.Result<FeatureSetVectorizer>() != null);

            yield return vectorizer;
        }

        protected virtual FeatureMatrix ToFeatureMatrix(List<Vector<double>[]> vectors)
        {
            Contract.Requires(vectors != null);

            return new FeatureMatrix(GetMatrix(vectors, 0), selectionStrategy);
        }

        protected Matrix<double> GetMatrix(List<Vector<double>[]> vectors, int vectorIndex)
        {
            Contract.Requires(vectors != null);
            Contract.Requires(vectorIndex >= 0);
            Contract.Ensures(Contract.Result<Matrix<double>>() != null);
            Contract.Ensures(Contract.Result<Matrix<double>>().Height == vectors.Count);

            var vectorBuffer = new Vector<double>[vectors.Count];

            for (int idx = 0; idx < vectors.Count; idx++)
            {
                vectorBuffer[idx] = vectors[idx][vectorIndex]; // TODO: Ensure indexes.
            }

            return Matrix.Wrap(vectorBuffer);
        }

        #endregion

        #region IFeatured Impl.

        Strings IFeaturedInput.InputFeatureIDs
        {
            get { return FeatureIDs; }
        }

        FeatureDescriptionSet IFeatured.FeatureDescriptions
        {
            get { return DataFeatureProvider.FeatureDescriptions; }
        }

        #endregion
    }
}
