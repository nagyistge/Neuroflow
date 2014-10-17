using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class LearningConnection
    {
        internal LearningConnection(INeuralConnection connection, ILearningRule rule)
        {
            Contract.Requires(connection != null);
            Contract.Requires(rule != null);

            Connection = connection;
            Rule = rule;
        }
        
        public INeuralConnection Connection { get; private set; }

        public ILearningRule Rule { get; private set; }
    }

    public enum AlgoInitializationMode : byte { Startup, BatchStart }

    public enum BackwardIterationMode : byte { None, Enabled, BackPropagation }
    
    public abstract class LearningAlgorithm
    {
        #region Properties And Fields

        LinkedList<LearningConnection> connectionList;

        ReadOnlyArray<LearningConnection> connectionsArray;

        public ReadOnlyArray<LearningConnection> LearningConnections
        {
            get { return connectionsArray; }
        }

        public abstract bool WantForwardIteration { get; }

        public abstract BackwardIterationMode BackwardIterationMode { get; }

        #endregion

        #region Init

        internal void BeginInit()
        {
            connectionList = new LinkedList<LearningConnection>();
        }

        internal void AddConnection(INeuralConnection connection, ILearningRule rule)
        {
            Contract.Requires(connection != null);
            Contract.Requires(rule != null);
            Contract.Requires(rule.AlgorithmType == GetType());

            connectionList.AddLast(new LearningConnection(connection, rule));
        }

        internal void EndInit()
        {
            connectionsArray = ReadOnlyArray.Wrap(connectionList.ToArray());
            connectionList = null;
        }

        #endregion

        #region Learning Protocol

        protected internal abstract void InitializeNewRun(AlgoInitializationMode mode);

        protected internal abstract void ForwardIteration(bool isNewBatch);

        protected internal abstract void BackwardIteration(bool isBatchIteration, double mse);

        #endregion
    }
}
