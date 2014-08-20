using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution
{
    public sealed class NaturalMutationParameters : SynchronizedObject
    {
        #region Defaults

        static readonly IntRange defaultRange = IntRange.CreateFixed(10);

        static readonly Probability defaultChance = 0;

        #endregion

        #region Constructor

        public NaturalMutationParameters()
        {
            mutationChunkSize = defaultRange;
            Contract.Assume(!mutationChunkSize.IsZero);
        }

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!mutationChunkSize.IsZero);
        }


        #endregion

        #region Properties

        IntRange mutationChunkSize;

        public IntRange MutationChunkSize
        {
            get { lock (SyncRoot) return mutationChunkSize; }
            set
            {
                Contract.Requires(!value.IsZero);

                lock (SyncRoot) mutationChunkSize = value;
            }
        }

        Probability deletionMutationChance = defaultChance;

        public Probability DeletionMutationChance
        {
            get { lock (SyncRoot) return deletionMutationChance; }
            set { lock (SyncRoot) deletionMutationChance = value; }
        }

        Probability duplicationMutationChance = defaultChance;

        public Probability DuplicationMutationChance
        {
            get { lock (SyncRoot) return duplicationMutationChance; }
            set { lock (SyncRoot) duplicationMutationChance = value; }
        }

        Probability inversionMutationChance = defaultChance;

        public Probability InversionMutationChance
        {
            get { lock (SyncRoot) return inversionMutationChance; }
            set { lock (SyncRoot) inversionMutationChance = value; }
        }

        Probability insertionMutationChance = defaultChance;

        public Probability InsertionMutationChance
        {
            get { lock (SyncRoot) return insertionMutationChance; }
            set { lock (SyncRoot) insertionMutationChance = value; }
        }

        Probability translocationMutationChance = defaultChance;

        public Probability TranslocationMutationChance
        {
            get { lock (SyncRoot) return translocationMutationChance; }
            set { lock (SyncRoot) translocationMutationChance = value; }
        }

        Probability pointMutationChance = defaultChance;

        public Probability PointMutationChance
        {
            get { lock (SyncRoot) return pointMutationChance; }
            set { lock (SyncRoot) pointMutationChance = value; }
        }

        public bool HasReorderMutationChance
        {
            get
            {
                var result = false;
                lock (this.SyncRoot)
                {
                    result =
                           deletionMutationChance.HasChance ||
                           duplicationMutationChance.HasChance ||
                           inversionMutationChance.HasChance ||
                           insertionMutationChance.HasChance ||
                           translocationMutationChance.HasChance;
                }
                return result;
            }
        }

        #endregion
    }
}
