using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Neural;
using NeoComp.Core;

namespace NeoComp.Optimization.Learning
{
    public sealed class SupervisedQSALearningStrategy :
        QSALearningStrategy,
        IBatchLearningStrategy
    {
        #region Constructor

        public SupervisedQSALearningStrategy(
            NeuralNetwork network, 
            double strength = 1.0,
            double learningReward = 1.0,
            double forgettingPunishment = 0.1,
            double ruttingPunishment = 0.0001)
            : base(network, strength)
        {
            Contract.Requires(network != null);
            Contract.Requires(strength > 0.0);
            Contract.Requires(learningReward > 0.0 && learningReward <= 1.0);
            Contract.Requires(forgettingPunishment > 0.0 && forgettingPunishment <= 1.0);
            Contract.Requires(ruttingPunishment >= 0.0 && ruttingPunishment <= 1.0);

            this.learningReward = learningReward;
            this.forgettingPunishment = forgettingPunishment;
            this.ruttingPunishment = ruttingPunishment;
        }

        #endregion

        #region Fields

        double? prevMSE;

        #endregion

        #region Properties

        double learningReward;

        public double LearningReward
        {
            get { lock (SyncRoot) return learningReward; }
            set 
            {
                Contract.Requires(value > 0.0 && value <= 1.0);

                lock (SyncRoot) learningReward = value;
            }
        }

        double forgettingPunishment;

        public double ForgettingPunishment
        {
            get { lock (SyncRoot) return forgettingPunishment; }
            set 
            {
                Contract.Requires(value > 0.0 && value <= 1.0); 

                lock (SyncRoot) forgettingPunishment = value;
            }
        }

        double ruttingPunishment;

        public double RuttingPunishment
        {
            get { lock (SyncRoot) return ruttingPunishment; }
            set
            {
                Contract.Requires(value >= 0.0 && value <= 1.0);

                lock (SyncRoot) ruttingPunishment = value; 
            }
        }

        #endregion

        #region Init

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();
            lock (SyncRoot) prevMSE = null;
        }

        #endregion

        #region Adjust

        private void Adjust(double mse)
        {
            double? prevMSE;
            lock (SyncRoot)
            {
                prevMSE = this.prevMSE;
                if (!prevMSE.HasValue)
                {
                    this.prevMSE = mse;
                    SetupItems();
                    return;
                }
                else
                {
                    this.prevMSE = mse;
                }
            }

            if (mse < prevMSE.Value)
            {
                // Learnt.

                double learningReward;
                lock (SyncRoot)
                {
                    learningReward = this.learningReward;
                }

                Reward(learningReward, true);
            }
            else if (mse > prevMSE.Value)
            {
                // Forgot.

                double forgettingPunishment;
                lock (SyncRoot)
                {
                    forgettingPunishment = this.forgettingPunishment;
                }

                Punishment(mse * forgettingPunishment, true);
            }
            else 
            {
                // Rutted.

                double ruttingPunishment;
                lock (SyncRoot)
                {
                    ruttingPunishment = this.ruttingPunishment;
                }

                if (ruttingPunishment != 0.0)
                {
                    Punishment(mse * ruttingPunishment, true);
                }
                else
                {
                    SetupItems();
                }
            }
        }

        #endregion

        #region IBatchAdjusterAlgorithm Members

        void IBatchLearningStrategy.Adjust(double mse, IEnumerable<IEnumerable<double>> errors)
        {
            Adjust(mse);
        }

        #endregion
    }
}
