using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    [ContractClass(typeof(IVectorizerContract<,>))]
    public interface IVectorizer<T, TSource>
        where T : struct
        where TSource : class
    {
        Vector<T> ToVector(TSource source);

        TSource FromVector(Vector<T> vector);
    }

    [ContractClassFor(typeof(IVectorizer<,>))]
    class IVectorizerContract<T, TSource> : IVectorizer<T, TSource>
        where T : struct
        where TSource : class
    {
        Vector<T> IVectorizer<T, TSource>.ToVector(TSource source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Vector<T>>() != null);
            return null;
        }

        TSource IVectorizer<T, TSource>.FromVector(Vector<T> vector)
        {
            Contract.Requires(vector != null);
            Contract.Ensures(Contract.Result<TSource>() != null);
            return default(TSource);
        }
    }
}
