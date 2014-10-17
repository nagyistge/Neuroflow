using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Data.Objects;

namespace Gender.Data
{
    public enum GenderTestSet { Training, Validation, All }
    
    public sealed class GenderComputationTester
    {
        public GenderComputationTester(GenderComputation computation, GenderTestSet testSetType)
        {
            Contract.Requires(computation != null);

            Computation = computation;
            TestSetType = testSetType;
        }
        
        public GenderComputation Computation { get; private set; }

        public GenderTestSet TestSetType { get; private set; }

        public int TestedItems { get; private set; }

        public int PassedItems { get; private set; }

        public int FailedItems { get; private set; }

        public double PassedRatio
        {
            get { return (double)PassedItems / (double)TestedItems; }
        }

        public void Update()
        {
            using (var ctx = new GenderEntities())
            {
                ObjectQuery<Item> q = null;
                if (TestSetType == GenderTestSet.All) q = ctx.Items;
                else if (TestSetType == GenderTestSet.Training) q = (ObjectQuery<Item>)ctx.Items.Where(i => i.IsTrainingPattern);
                else if (TestSetType == GenderTestSet.Validation) q = (ObjectQuery<Item>)ctx.Items.Where(i => !i.IsTrainingPattern);
                var result = q.Execute(MergeOption.NoTracking);

                int count = 0;
                int passed = 0;
                int failed = 0;
                foreach (var item in result)
                {
                    bool gender = Computation.ComputeGender(item.Pixels);
                    if (gender == item.Gender) passed++; else failed++;
                    count++;
                }
                TestedItems = count;
                PassedItems = passed;
                FailedItems = failed;
            }
        }
    }
}
