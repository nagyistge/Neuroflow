using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Computations
{
    [ContractClass(typeof(INormalizedVectorizerContract<>))]
    public interface INormalizedVectorizer<T> : IRealVectorizer<T>
        where T : class
    {
        DoubleRange NormalizationRange { get; }
    }

    [ContractClassFor(typeof(INormalizedVectorizer<>))]
    class INormalizedVectorizerContract<T> : INormalizedVectorizer<T>
        where T : class
    {
        DoubleRange INormalizedVectorizer<T>.NormalizationRange
        {
            get
            {
                Contract.Ensures(!Contract.Result<DoubleRange>().IsFixed);
                return default(DoubleRange);
            }
        }

        Vector<double> IVectorizer<double, T>.ToVector(T source)
        {
            throw new NotImplementedException();
        }

        T IVectorizer<double, T>.FromVector(Vector<double> vector)
        {
            throw new NotImplementedException();
        }
    }
}
