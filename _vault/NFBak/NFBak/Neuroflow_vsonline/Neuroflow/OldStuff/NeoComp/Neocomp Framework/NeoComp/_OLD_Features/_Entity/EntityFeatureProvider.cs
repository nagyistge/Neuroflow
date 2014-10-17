using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Diagnostics.Contracts;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using NeoComp.Core;

namespace NeoComp.Features
{
    public class EntityFeatureProvider<T> : ISubsetDataFeatureProvider
        where T : class
    {
        #region Constructor

        public EntityFeatureProvider(string connString,
            string entitySetName,
            string keyColumnNames,
            Strings featureProjections,
            string includePaths = null,
            string whereClause = null)
        {
            Contract.Requires(!string.IsNullOrEmpty(connString));
            Contract.Requires(!string.IsNullOrEmpty(entitySetName));
            Contract.Requires(!string.IsNullOrEmpty(keyColumnNames));
            Contract.Requires(!featureProjections.IsNullOrEmpty());

            this.featureProjections = featureProjections;
            EntitySetName = entitySetName;
            KeyColumnNames = keyColumnNames;
            keyQueryPars = GetKeyQueryParameters(KeyColumnNames);
            this.includePaths = includePaths;
            this.connString = connString;
            WhereClause = whereClause;
            Reinitialize(true);
        }

        private EntityFeatureProvider(string connString,
            string entitySetName,
            string keyColumnNames,
            Strings featureProjections,
            string includePaths,
            string whereClause,
            FeatureDescriptionSet featureDescriptions)
        {
            Contract.Requires(!string.IsNullOrEmpty(connString));
            Contract.Requires(!string.IsNullOrEmpty(entitySetName));
            Contract.Requires(!string.IsNullOrEmpty(keyColumnNames));
            Contract.Requires(!featureProjections.IsNullOrEmpty());
            Contract.Requires(featureDescriptions != null);
            

            this.featureProjections = featureProjections;
            EntitySetName = entitySetName;
            KeyColumnNames = keyColumnNames;
            keyQueryPars = GetKeyQueryParameters(KeyColumnNames);
            this.includePaths = includePaths;
            this.connString = connString;
            WhereClause = whereClause;
            FeatureDescriptions = featureDescriptions;
            Reinitialize(false);
        } 

        #endregion

        #region Fields and Properties

        string connString, keyQueryPars, includePaths;

        Strings featureProjections;

        public FeatureDescriptionSet FeatureDescriptions { get; private set; }

        public string EntitySetName { get; private set; }

        public string KeyColumnNames { get; private set; }

        public int ItemCount { get; private set; }

        public string WhereClause { get; private set; }

        #endregion

        #region Query Generation

        string eSQL_ItemsQuery, eSQL_RecordQueryPars;

        private ObjectQuery<T> CreateItemsQuery(ObjectContext context)
        {
            if (string.IsNullOrEmpty(eSQL_ItemsQuery))
            {
                var keyOrderPars = GetKeyQueryParameters(KeyColumnNames);
                try
                {
                    var oset = context.CreateObjectSet<T>(EntitySetName);
                    var q = oset.OrderBy(keyOrderPars);
                    if (!string.IsNullOrEmpty(includePaths)) q = q.Include(includePaths);
                    if (!string.IsNullOrEmpty(WhereClause)) q = q.Where(WhereClause);
                    eSQL_ItemsQuery = q.CommandText;
                    return q;
                }
                catch (Exception ex)
                {
                    var newEx = new InvalidOperationException("Cannot create object query. See inner exception and provided data 'entitySetName' and 'keyColumnNames' for details.", ex);
                    newEx.Data["entitySetName"] = EntitySetName;
                    newEx.Data["keyColumnNames"] = keyOrderPars;
                    throw newEx;
                }
            }
            else
            {
                return context.CreateQuery<T>(eSQL_ItemsQuery);
            }
        }

        private ObjectQuery<DbDataRecord> CreateRecordQuery(ObjectContext context)
        {
            if (string.IsNullOrEmpty(eSQL_RecordQueryPars))
            {
                var fsb = new StringBuilder();
                foreach (var info in FeatureDescriptions)
                {
                    if (fsb.Length != 0) fsb.Append(',');
                    fsb.Append((string)info.Context);
                }
                fsb.Append(',');
                fsb.Append(keyQueryPars);
                eSQL_RecordQueryPars = fsb.ToString();
            }
            return CreateItemsQuery(context).Select(eSQL_RecordQueryPars).OrderBy(keyQueryPars);
        } 

        #endregion

        #region KeyColumnNames to Key Query Pars

        private static string GetKeyQueryParameters(string keyColumnNames)
        {
            var split = keyColumnNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var part in split)
            {
                if (sb.Length != 0) sb.Append(',');
                sb.Append("it.");
                sb.Append(part);
            }
            return sb.ToString();
        }

        #endregion

        #region Reinit Logic
        
        private void Reinitialize(bool withDescs)
        {
            using (var ctx = new ObjectContext(connString))
            {
                var q = CreateItemsQuery(ctx);
                if (withDescs) FeatureDescriptions = GetFeatureDescriptions(q);
                ItemCount = GetItemCount(q);
            }
        } 

        private FeatureDescriptionSet GetFeatureDescriptions(ObjectQuery<T> itemsQuery)
        {
            var list = new List<FeatureDescription>();

            foreach (var definition in featureProjections)
            {
                char? postfix;
                string id = FeatureHelpers.GetID(definition, out postfix);
                string prj = "it." + id;
                if (postfix == null)
                {
                    double min = itemsQuery.SelectValue<double>(prj).Min();
                    double max = itemsQuery.SelectValue<double>(prj).Max();
                    list.Add(new ValueFeatureDescription(id, new DoubleRange(min, max), prj));
                }
                else
                {
                    bool expand = postfix == '>';
                    var items = itemsQuery.SelectValue<object>(prj).Distinct().ToArray();
                    list.Add(new SetFeatureDescription(id, expand, items, prj));
                }
            }

            return new FeatureDescriptionSet(list);
        }

        private int GetItemCount(ObjectQuery<T> itemsQuery)
        {
            return itemsQuery.Count();
        }

        #endregion

        #region Access

        public FeatureSet this[int index]
        {
            get
            {
                using (var ctx = new ObjectContext(connString))
                {
                    var q = CreateRecordQuery(ctx);
                    var record = q.Skip(index).Take(1).FirstOrDefault();
                    if (record != null) return new FeatureSet(GetFeatures(record));
                    return null;
                }
            }
        }

        public IList<FeatureSet> GetItems(int index, int count)
        {
            using (var ctx = new ObjectContext(connString))
            {
                var q = CreateRecordQuery(ctx);
                return q.Skip(index).Take(count).AsEnumerable().Select((r) => new FeatureSet(GetFeatures(r))).ToList();
            }
        }

        public IList<FeatureSet> GetAllItems()
        {
            using (var ctx = new ObjectContext(connString))
            {
                var q = CreateRecordQuery(ctx);
                return q.AsEnumerable().Select((r) => new FeatureSet(GetFeatures(r))).ToList();
            }
        }

        private IEnumerable<Feature> GetFeatures(DbDataRecord record)
        {
            int idx = 0;
            foreach (var info in FeatureDescriptions)
            {
                if (record.IsDBNull(idx))
                {
                    yield return info.CreateFeature(null);
                }
                else
                {
                    yield return info.CreateFeature(record[idx]);
                }
                idx++;
            }
        }

        #endregion

        #region Subset

        IDataFeatureProvider ISubsetDataFeatureProvider.GetDataSubsetProvider(params object[] subsetParameters)
        {
            string where = subsetParameters[0] as string;
            if (string.IsNullOrEmpty(where)) throw new ArgumentException("Where clause subset parameter expected.");
            return GetEntitySubsetProvider(where);
        }

        public EntityFeatureProvider<T> GetEntitySubsetProvider(string whereClause)
        {
            Contract.Requires(!string.IsNullOrEmpty(whereClause));
            return new EntityFeatureProvider<T>(connString, EntitySetName, KeyColumnNames, featureProjections, includePaths, WhereAND(WhereClause, whereClause), FeatureDescriptions);
        }

        private static string WhereAND(string whereClause1, string whereClause2)
        {
            if (string.IsNullOrEmpty(whereClause1)) return whereClause2;
            return string.Format("({0}) AND ({1})", whereClause1, whereClause2);
        }

        #endregion
    }
}
