using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NeoComp.DEBUG;
using System.Diagnostics;

namespace ComputationAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            var buff = new ComputationValueBuffer<double>();

            var sw = new Stopwatch();

            int iterations = 1000000;
            int count = 100;
            var f = new SigmoidFunction();
            var inputs = buff.Declare(count);
            var weights = buff.Declare(count);
            var output = buff.Declare();

            Randomize(inputs);
            Randomize(weights);

            Expression sumExpr = null;
            for (int i = 0; i < count; i++)
            {
                if (sumExpr == null)
                {
                    sumExpr = Expression.Multiply(inputs[i].ValueExpression, weights[i].ValueExpression);
                }
                else
                {
                    sumExpr = Expression.Add(sumExpr, Expression.Multiply(inputs[i].ValueExpression, weights[i].ValueExpression));
                }
            }
            var compOutExpr = output.Assign(f.Function(sumExpr));
            var compOutSafe = buff.Compile(compOutExpr);

            sw.Start();
            for (int it = 0; it < iterations; it++)
            {
                double sum = 0.0;
                for (int i = 0; i < count; i++)
                {
                    sum += inputs[i].Value * weights[i].Value;
                }
                output.Value = f.Function(sum);
            }
            sw.Stop();

            Console.WriteLine(output.Value);
            Console.WriteLine("Standard: {0}", sw.ElapsedMilliseconds);

            Console.WriteLine();
            sw.Reset();
            output.Value = 0.0;

            sw.Start();
            for (int it = 0; it < iterations; it++)
            {
                compOutSafe();
            }
            sw.Stop();

            Console.WriteLine(output.Value2); 
            Console.WriteLine("Compiled Safe: {0}", sw.ElapsedMilliseconds);

            Console.ReadKey();
        }

        private static void Randomize(ComputationValue<double>[] values)
        {
            var rnd = new Random();
            foreach (var v in values) v.Value = v.Value2 = rnd.NextDouble() * 2.0 - 1.0;
        }
    }
}
