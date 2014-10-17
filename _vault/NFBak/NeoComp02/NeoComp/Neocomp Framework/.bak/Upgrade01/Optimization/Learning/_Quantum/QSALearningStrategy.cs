using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Neural;
using NeoComp.Optimization.Quantum;
using NeoComp.Networks;

namespace NeoComp.Optimization.Learning
{
    public class QSALearningStrategy : LearningStrategy, IReinforcementLearningStrategy
    {
        #region Constructor

        public QSALearningStrategy(NeuralNetwork network, double strength = 1.0)
            : base(network)
        {
            Contract.Requires(network != null);
            Contract.Requires(strength > 0.0);

            this.strength = strength;
        }

        #endregion

        #region Fields

        QuantumStabilizerAlgorithm qsa;

        #endregion

        #region Properties

        double strength;

        public double Strength
        {
            get { lock (SyncRoot) return strength; }
            set
            {
                Contract.Requires(value > 0.0); 
                
                lock (SyncRoot)
                {
                    strength = value;
                    if (qsa != null) qsa.Strength = value;
                }
            }
        } 

        #endregion

        #region Init

        protected internal override void InitializeNewRun()
        {
            lock (Network.SyncRoot) lock (SyncRoot)
            {
                if (qsa == null) CreateQSA(); else qsa.Reset();
                qsa.SetupItems();
            }
        }

        private void CreateQSA()
        {
            try
            {
                //int endIdx = Network.Size - Network.OutputInterface.Length;
                //var q = from ai in Network.GetAdjustableItems()
                //        let s = ai as Synapse
                //        where s == null || (s.Index.LowerNodeIndex <= endIdx)
                //        select ai;

                //qsa = new QuantumStabilizerAlgorithm(
                //    q.Select(i => new QSAItem(i)),
                //    Strength);

                qsa = new QuantumStabilizerAlgorithm(
                    Network.GetAdjustableItems().Select(i => new QSAItem(i)),
                    Strength);
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException("Adjustable items not found in network.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot create QSA. See inner exception for details.", ex);
            }
        }

        #endregion

        #region Reinforcement

        protected void Reward(double reward, bool andSetup)
        {
            Contract.Requires(reward >= 0.0 && reward <= 1.0);

            lock (Network.SyncRoot)
            {
                qsa.Stabilize(reward, andSetup);
            }
        }

        protected void Punishment(double punishment, bool andSetup)
        {
            Contract.Requires(punishment >= 0.0 && punishment <= 1.0);

            lock (Network.SyncRoot)
            {
                qsa.Dissolve(punishment, andSetup);
            }
        }

        protected void SetupItems()
        {
            lock (Network.SyncRoot)
            {
                qsa.SetupItems();
            }
        }

        #endregion

        #region IReinforcementAdjusterAlgorithm Members

        void IReinforcementLearningStrategy.Reward(double reward)
        {
            Reward(reward, true);
        }

        void IReinforcementLearningStrategy.Punishment(double punishment)
        {
            Punishment(punishment, true);
        }

        #endregion
    }
}
