using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Evolution
{
    public static class SequenceOperators
    {
        #region Crossover

        public static IEnumerable<T> Crossover<T>(this IEnumerable<IEnumerable<T>> sequences, IntRange chunkSize)
        {
            Contract.Requires(sequences != null);
            Contract.Requires(chunkSize.MinValue > 0);

            IEnumerable<T> first = null;
            var enumerators = new List<ObservableEnumerator<T>>();
            foreach (var seq in sequences)
            {
                if (seq != null)
                {
                    enumerators.Add(new ObservableEnumerator<T>(seq.GetEnumerator()));
                    if (first == null) first = seq;
                }
            }

            if (enumerators.Count == 1)
            {
                return first;
            }
            else if (enumerators.Count != 0)
            {
                return YieldCrossover(enumerators, chunkSize);
            }
            return Enumerable.Repeat(default(T), 0);
        }

        private static IEnumerable<T> YieldCrossover<T>(List<ObservableEnumerator<T>> enumerators, IntRange chunkSize)
        {
            int seqInx = RandomGenerator.Random.Next(enumerators.Count);
            int counter = 0;
            int switchOn = chunkSize.PickRandomValue();
            int doneCount = 0;

            while (doneCount != enumerators.Count)
            {
                // Step enumerators:
                for (int enumIdx = 0; enumIdx < enumerators.Count; enumIdx++)
                {
                    var enumerator = enumerators[enumIdx];
                    if (enumerator != null)
                    {
                        if (!enumerator.MoveNext())
                        {
                            enumerators[enumIdx] = null;
                            doneCount++;
                        }
                    }
                }

                // Yield if current enumerator has value:
                var currentEnum = enumerators[seqInx];
                if (currentEnum != null)
                {
                    yield return currentEnum.Current;
                }

                if (++counter == switchOn)
                {
                    counter = 0;
                    switchOn = chunkSize.PickRandomValue();
                    seqInx = RandomGenerator.Random.Next(enumerators.Count);
                }
            }
        } 

        #endregion

        #region Chunks

        public static IEnumerable<IEnumerable<T>> ToChunks<T>(this IEnumerable<T> sequence, IntRange chunkSize)
        {
            Contract.Requires(sequence != null);
            Contract.Requires(!chunkSize.IsZero);
            Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

            return DoToChunks(sequence, chunkSize); // TODO: Contracts iterator support.
        }

        static IEnumerable<IEnumerable<T>> DoToChunks<T>(IEnumerable<T> sequence, IntRange chunkSize)
        {
            var enumerator = new ObservableEnumerator<T>(sequence.GetEnumerator());

            while (!enumerator.OnEnd)
            {
                int nextChunkSize = chunkSize.PickRandomValue();
                if (nextChunkSize == 1)
                {
                    if (enumerator.MoveNext()) yield return new[] { enumerator.Current };
                }
                else if (nextChunkSize == 2)
                {
                    if (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (enumerator.MoveNext())
                        {
                            yield return new[] { current, enumerator.Current };
                        }
                        else
                        {
                            yield return new[] { current };
                        }
                    }
                }
                else
                {
                    var chunk = GetChunk(enumerator, nextChunkSize);
                    if (chunk.Count > 0) yield return chunk;
                }
            }
        }

        public static IEnumerable<T> ToSequence<T>(this IEnumerable<IEnumerable<T>> chunks)
        {
            foreach (var chunk in chunks)
            {
                if (chunk != null)
                {
                    foreach (var entiry in chunk)
                    {
                        yield return entiry;
                    }
                }
            }
        }

        private static List<T> GetChunk<T>(ObservableEnumerator<T> enumerator, int chunkSize)
        {
            var chunk = new List<T>(chunkSize);
            while (enumerator.MoveNext())
            {
                chunk.Add(enumerator.Current);
                if (chunk.Count == chunkSize) break;
            }
            return chunk;
        }

        #endregion

        #region Deletion Operator

        public static IEnumerable<IEnumerable<T>> Deletion<T>(this IEnumerable<IEnumerable<T>> chunks, Probability probability)
        {
            Contract.Requires(chunks != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

            return DoDeletion(chunks, probability); // TODO: Contracts iterator support.
        }

        static IEnumerable<IEnumerable<T>> DoDeletion<T>(IEnumerable<IEnumerable<T>> chunks, Probability probability)
        {
            if (probability.HasChance)
            {
                foreach (var chunk in chunks)
                {
                    if (probability)
                    {
                        continue;
                    }
                    yield return chunk;
                }
            }
            else
            {
                foreach (var chunk in chunks)
                {
                    yield return chunk;
                }
            }
        }

        #endregion

        #region Duplication Operator

        public static IEnumerable<IEnumerable<T>> Duplication<T>(this IEnumerable<IEnumerable<T>> chunks, Probability probability)
        {
            Contract.Requires(chunks != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

            return DoDuplication(chunks, probability); // TODO: Contracts iterator support.
        }

        static IEnumerable<IEnumerable<T>> DoDuplication<T>(IEnumerable<IEnumerable<T>> chunks, Probability probability)
        {
            if (probability.HasChance)
            {
                foreach (var chunk in chunks)
                {
                    if (probability)
                    {
                        yield return chunk;
                    }
                    yield return chunk;
                }
            }
            else
            {
                foreach (var chunk in chunks)
                {
                    yield return chunk;
                }
            }
        }

        #endregion

        #region Inversion Operator

        public static IEnumerable<IEnumerable<T>> Inversion<T>(this IEnumerable<IEnumerable<T>> chunks, Probability probability)
        {
            Contract.Requires(chunks != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

            return DoInversion(chunks, probability); // TODO: Contracts iterator support.
        }

        static IEnumerable<IEnumerable<T>> DoInversion<T>(IEnumerable<IEnumerable<T>> chunks, Probability probability)
        {
            if (probability.HasChance)
            {
                foreach (var chunk in chunks)
                {
                    if (probability)
                    {
                        yield return chunk.Reverse();
                    }
                    else
                    {
                        yield return chunk;
                    }
                }
            }
            else
            {
                foreach (var chunk in chunks)
                {
                    yield return chunk;
                }
            }
        }

        #endregion

        #region Insertion Operator

        public static IEnumerable<IEnumerable<T>> Insertion<T>(this IEnumerable<IEnumerable<T>> chunks, Probability probability)
        {
            Contract.Requires(chunks != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

            if (probability.HasChance)
            {
                //var list = chunks.ToList();
                //var insert = list.Where(c => probability).ToList();

                var list = new List<IEnumerable<T>>();
                var insert = new LinkedList<IEnumerable<T>>();
                foreach (var chunk in chunks)
                {
                    list.Add(chunk);
                    if (probability) insert.AddLast(chunk);
                }

                foreach (var chunk in insert)
                {
                    list.Insert(RandomGenerator.Random.Next(list.Count), chunk);
                }
                return list;
            }
            else
            {
                return chunks;
            }
        }

        #endregion

        #region Translocation Operator

        public static IEnumerable<IEnumerable<T>> Translocation<T>(this IEnumerable<IEnumerable<T>> chunks, Probability probability)
        {
            Contract.Requires(chunks != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

            if (probability.HasChance)
            {
                var list = chunks.ToList();
                if (list.Count > 1)
                {
                    for (int idx = 0; idx < list.Count; idx++)
                    {
                        if (probability)
                        {
                            int otherIdx = RandomGenerator.Random.Next(list.Count);
                            while (otherIdx == idx) otherIdx = RandomGenerator.Random.Next(list.Count);
                            var x = list[idx];
                            list[idx] = list[otherIdx];
                            list[otherIdx] = x;
                        }
                    }
                }
                return list;
            }
            else
            {
                return chunks;
            }
        }

        #endregion

        #region Mutate

        public static IEnumerable<IEnumerable<T>> Mutate<T>(this IEnumerable<IEnumerable<T>> chunks, NaturalMutationParameters parameters)
        {
            Contract.Requires(chunks != null);
            Contract.Requires(parameters != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

            Probability del = default(Probability);
            Probability dup = default(Probability);
            Probability inv = default(Probability);
            Probability ins = default(Probability);
            Probability tra = default(Probability);

            lock (parameters.SyncRoot)
            {
                del = parameters.DeletionMutationChance;
                dup = parameters.DuplicationMutationChance;
                inv = parameters.InversionMutationChance;
                ins = parameters.InsertionMutationChance;
                tra = parameters.TranslocationMutationChance;
            }

            var result = chunks;

            if (del.HasChance) result = result.Deletion(del);
            if (dup.HasChance) result = result.Duplication(dup);
            if (inv.HasChance) result = result.Inversion(inv);
            if (ins.HasChance) result = result.Insertion(ins);
            if (tra.HasChance) result = result.Translocation(tra);

            return result;
        }

        public static IEnumerable<T> Mutate<T>(this IEnumerable<T> sequence, Probability pointMutationChance, Func<T, T> pointMutationMethod)
        {
            Contract.Requires(sequence != null);
            // TODO: Fix this Contract bug.
            // Contract.Requires(pointMutationMethod != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            if (pointMutationChance.HasChance)
            {
                return sequence.Select(v => pointMutationChance ? pointMutationMethod(v) : v);
            }
            return sequence;
        }

        #endregion
    }
}
