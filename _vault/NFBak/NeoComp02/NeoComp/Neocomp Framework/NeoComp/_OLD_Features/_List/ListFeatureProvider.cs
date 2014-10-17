using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Globalization;
using System.IO;

namespace NeoComp.Features
{
    public delegate bool ListFeatureFilter(int rowIndex, IDictionary<string, double?> row);
    
    public class ListFeatureProvider : ISubsetDataFeatureProvider
    {
        #region Constructor

        public ListFeatureProvider(IEnumerable<IEnumerable> source, Strings featureProjections, bool firsRowIsID = true, int rowsToSkip = 0, ListFeatureFilter filter = null)
        {
            Contract.Requires(source != null);
            Contract.Requires(!featureProjections.IsNullOrEmpty());
            Contract.Requires(rowsToSkip >= 0);

            Build(source, featureProjections, firsRowIsID, rowsToSkip, filter);
        }

        private ListFeatureProvider(ListFeatureProvider baseProvider, ListFeatureFilter filter)
        {
            Contract.Requires(baseProvider != null);
            Contract.Requires(filter != null);

            Build(baseProvider, filter);
        }

        #endregion

        #region Properties

        Dictionary<int, Tuple<int, Dictionary<string, double?>>> rowData = new Dictionary<int, Tuple<int, Dictionary<string, double?>>>();

        ReadOnlyArray<FeatureSet> items;

        public int ItemCount
        {
            get { return items.ItemArray.Length; }
        }

        public FeatureDescriptionSet FeatureDescriptions { get; private set; }

        public FeatureSet this[int index]
        {
            get { return items.ItemArray[index]; }
        } 

        #endregion

        #region Get Items

        public IList<FeatureSet> GetItems(int index, int count)
        {
            var result = new FeatureSet[count];
            for (int idx = 0; idx < count; idx++) result[idx] = items.ItemArray[idx + index];
            return result;
        }

        public IList<FeatureSet> GetAllItems()
        {
            return items;
        } 

        #endregion

        #region Subset

        IDataFeatureProvider ISubsetDataFeatureProvider.GetDataSubsetProvider(params object[] subsetParameters)
        {
            var filter = subsetParameters[0] as ListFeatureFilter;
            if (filter == null) throw new ArgumentException("ListFeatureFilter parameter expected.");
            return GetListSubsetProvider(filter);
        }

        public ListFeatureProvider GetListSubsetProvider(ListFeatureFilter filter)
        {
            Contract.Requires(filter != null);

            return new ListFeatureProvider(this, filter);
        }

        #endregion

        #region Build

        // TODO: Refactor this shit.
        private void Build(IEnumerable<IEnumerable> source, Strings featureProjections, bool firsRowIsID, int rowsToSkip, ListFeatureFilter filter)
        {
            int? rowSize = null;
            var valueList = new List<List<object>>();
            Dictionary<string, int> idsByName = null;
            Dictionary<int, string> idsByIndex = null;
            if (firsRowIsID && rowsToSkip == 0) rowsToSkip = 1;
            int index = 0;
            foreach (var row in source)
            {
                if (row != null)
                {
                    if (index == 0 && firsRowIsID)
                    {
                        idsByName = row.Cast<object>()
                                 .Select(obj => obj.ToString())
                                 .Select((str, idx) => new { Idx = idx, Str = str })
                                 .ToDictionary(info => info.Str, info => info.Idx);

                        if (idsByName.Count == 0) throw new InvalidDataException("Invalid ID row.");

                        idsByIndex = idsByName.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

                        rowSize = idsByName.Count;
                    }

                    if (index >= rowsToSkip)
                    {
                        var rowValues = row.Cast<object>().ToList();

                        if (rowValues.Count == 0) throw new InvalidDataException("Values not found on: " + index + ".");

                        if (rowSize == null)
                        {
                            rowSize = rowValues.Count;
                        }
                        else if (rowSize != rowValues.Count)
                        {
                            throw new InvalidDataException("Invalid number of source values on: " + index + ".");
                        }

                        if (idsByName == null)
                        {
                            idsByName = Enumerable.Range(0, rowSize.Value).ToDictionary(idx => "Column" + idx);
                            idsByIndex = idsByName.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
                        }

                        var rowDict = new Dictionary<string, double?>();
                        for (int idx = 0; idx < rowValues.Count; idx++)
                        {
                            rowDict.Add(idsByIndex[idx], FeatureHelpers.ToDouble(rowValues[idx]));
                        }

                        bool canAdd = true;
                        if (filter != null) canAdd = filter(index, rowDict);
                        if (canAdd)
                        {
                            int valueIndex = valueList.Count;
                            valueList.Add(rowValues);
                            rowData.Add(valueIndex, Tuple.Create(index, rowDict));
                        }
                    }
                }
                index++;
            }

            if (rowSize == null) throw new InvalidDataException("Source is empty.");

            var descDict = new Dictionary<int, FeatureDescription>();

            foreach (var definition in featureProjections)
            {
                char? postfix;
                string id = FeatureHelpers.GetID(definition, out postfix);
                int indexOfID;
                if (!idsByName.TryGetValue(id, out indexOfID))
                {
                    throw new ArgumentException("Feature '" + id + "' not found", "featureProjections");
                }
                if (postfix == null)
                {
                    descDict.Add(indexOfID, new ValueFeatureDescription(id, GetMinMax(valueList, indexOfID)));
                }
                else
                {
                    bool expand = postfix == '>';
                    var distItems = GetDistinctValues(valueList, indexOfID);
                    descDict.Add(indexOfID, new SetFeatureDescription(id, expand, distItems));
                }
            }

            var itemsArray = new FeatureSet[valueList.Count];
            for (int rowIdx = 0; rowIdx < valueList.Count; rowIdx++)
            {
                var currentSet = new FeatureSet();
                var currentRow = valueList[rowIdx];
                for (int idx = 0; idx < currentRow.Count; idx++)
                {
                    FeatureDescription currentDesc;
                    if (descDict.TryGetValue(idx, out currentDesc))
                    {
                        currentSet.Add(currentDesc.CreateFeature(currentRow[idx]));
                    }
                }
                itemsArray[rowIdx] = currentSet;
            }

            this.items = ReadOnlyArray.Wrap(itemsArray);
            FeatureDescriptions = new FeatureDescriptionSet(descDict.Values);
        }

        private void Build(ListFeatureProvider baseProvider, ListFeatureFilter filter)
        {
            var itemList = new List<FeatureSet>();
            FeatureDescriptions = baseProvider.FeatureDescriptions;
            for (int setIdx = 0; setIdx < baseProvider.items.ItemArray.Length; setIdx++)
            {
                var baseSet = baseProvider.items.ItemArray[setIdx];
                var baseRowDataItem = baseProvider.rowData[setIdx];
                if (filter(baseRowDataItem.Item1, baseRowDataItem.Item2))
                {
                    int itemIdx = itemList.Count;
                    itemList.Add(baseSet);
                    rowData.Add(itemIdx, baseRowDataItem);
                }
            }

            if (itemList.Count == 0) throw new InvalidDataException("Filtered item collection is empty.");

            items = ReadOnlyArray.Wrap(itemList.ToArray());
        }

        static DoubleRange GetMinMax(List<List<object>> values, int index)
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (var row in values)
            {
                double? value = FeatureHelpers.ToDouble(row[index]);
                if (value.HasValue)
                {
                    if (value < min) min = value.Value; else if (value > max) max = value.Value;
                }
            }
            return new DoubleRange(min, max);
        }

        static object[] GetDistinctValues(List<List<object>> values, int index)
        {
            var q = from row in values
                    let value = row[index]
                    where value != null
                    select value;
            return q.Distinct().ToArray();
        }

        #endregion
    }
}
