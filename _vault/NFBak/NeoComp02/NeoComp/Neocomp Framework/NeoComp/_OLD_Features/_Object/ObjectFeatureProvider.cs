using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace NeoComp.Features
{
    public delegate bool ObjectFeatureFilter(int index, IDictionary<string, object> values);
    
    public class ObjectFeatureProvider : ISubsetDataFeatureProvider
    {
        #region Entry Class

        sealed class Entry
        {
            #region Constructor

            internal Entry(ObjectFeatureProvider owner, string projection, Type type, IList sourceList)
            {
                Contract.Requires(!String.IsNullOrEmpty(projection));
                Contract.Requires(type != null);
                Contract.Requires(owner != null);

                Owner = owner;

                // Build:
                Build(projection, type, sourceList);
            }

            #endregion

            #region Fields and Properties

            IPropertyOrFieldAccessor[] accessors;

            internal ObjectFeatureProvider Owner { get; private set; }

            internal FeatureDescription Description { get; private set; }

            internal Type FeatureType
            {
                get { return accessors[accessors.Length - 1].PropertyOrFieldType; }
            }

            #endregion

            #region Build

            private void Build(string projection, Type type, IList sourceList)
            {
                // TODO: Static registry!
                // TODO: Ovveride value ranges.

                string[] parts = projection.Split(new[] { '.' });
                var accessorList = new List<IPropertyOrFieldAccessor>(parts.Length);
                Type currentType = type;
                char? postfix = null;
                for (int idx = 0; idx < parts.Length; idx++)
                {
                    string part = parts[idx];

                    if (string.IsNullOrEmpty(part))
                    {
                        throw GetPartNotFoundEx(projection);
                    }

                    if (idx == parts.Length - 1)
                    {
                        part = FeatureHelpers.GetID(part, out postfix);
                    }

                    if (string.IsNullOrEmpty(part))
                    {
                        throw GetPartNotFoundEx(projection);
                    }

                    IPropertyOrFieldAccessor cacc;
                    try
                    {
                        MemberInfo cmi = currentType.GetProperty(part);
                        cacc = AccessorFactory.CreatePropertyOrFieldAccessor(cmi);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            string.Format("Property '{0}' not found or cannot be accessed on type '{1}'. See inner exception for details.", part, currentType.Name), ex);
                    }

                    accessorList.Add(cacc);
                    currentType = cacc.PropertyOrFieldType;
                }

                // Ok. Accessors found.
                accessors = accessorList.ToArray();
                Debug.Assert(accessors.Length > 0);

                // Create Description:
                char? foo;
                string id = FeatureHelpers.GetID(projection, out foo);
                try
                {
                    switch (postfix)
                    {
                        case '<':
                        case '>':
                            Description = new SetFeatureDescription(id, postfix == '>', GetDistinctValues(sourceList, id));
                            break;
                        case '#':
                            Description = CreateBinaryFeatureDescription(id, sourceList);
                            break;
                        default:
                            Description = new ValueFeatureDescription(id, GetMinMaxRange(sourceList, id)); 
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                            string.Format("Cannot initialize feature description by id '{0}'. See inner exception for details.", id), ex);
                }

                // Done.
            }

            private IFeatureDescriptionOverride GetDescOverride(object firstObj)
            {
                var dov = Owner as IFeatureDescriptionOverride;
                if (dov != null) return dov;
                return firstObj as IFeatureDescriptionOverride;
            }

            private BinaryFeatureDescription CreateBinaryFeatureDescription(string featureID, IList sourceList)
            {
                if (FeatureType.IsArray)
                {
                    int length = DetermineArrayLength(featureID, sourceList);
                    return new BinaryFeatureDescription(featureID, FeatureType, length, GetItemValueRange(featureID, sourceList[0]));
                }
                throw new InvalidOperationException("Cannot initialize Binary feature description '" + featureID + "'.");
            }

            private int DetermineArrayLength(string featureID, IList sourceList)
            {
                var dov = GetDescOverride(sourceList[0]);
                if (dov != null)
                {
                    int? count = dov.GetItemCount(featureID);
                    if (count != null) return count.Value;
                }

                for (int idx = 0; idx < sourceList.Count; idx++)
                {
                    var array = AccessValue(sourceList[idx], featureID) as Array;
                    if (array != null) return array.Length;
                }

                throw new InvalidOperationException("Array feature '" + featureID + "' cannot be determined.");
            }

            private DoubleRange? GetItemValueRange(string featureID, object firstObj)
            {
                var dov = GetDescOverride(firstObj);
                if (dov != null)
                {
                    var rng = dov.GetRange(featureID);
                    return rng;
                }
                return null;
            }

            private IList GetDistinctValues(IList sourceList, string featureID)
            {
                var dov = GetDescOverride(sourceList[0]);
                if (dov != null)
                {
                    var values = dov.GetDistinctValues(featureID);
                    if (values != null) return values;
                }
                
                var q = from obj in sourceList.Cast<object>()
                        let value = AccessValue(obj, featureID)
                        where value != null
                        select value;
                return q.Distinct().ToList();
            }

            private DoubleRange GetMinMaxRange(IList sourceList, string featureID)
            {
                var dov = GetDescOverride(sourceList[0]);
                if (dov != null)
                {
                    var rng = dov.GetRange(featureID);
                    if (rng != null) return rng.Value;
                }
                
                double min = double.MaxValue;
                double max = double.MinValue;
                foreach (var obj in sourceList)
                {
                    double? value = FeatureHelpers.ToDouble(AccessValue(obj, featureID));
                    if (value.HasValue)
                    {
                        if (value < min) min = value.Value; else if (value > max) max = value.Value;
                    }
                }
                return new DoubleRange(min, max);
            } 

            #endregion

            #region Value Access

            internal object GetValue(object rootObject)
            {
                if (rootObject == null) return null;

                try
                {
                    object currentObj = rootObject;
                    foreach (var acc in accessors)
                    {
                        if ((currentObj = acc.FastGet(currentObj)) == null) return null;
                    }
                    return currentObj;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Cannot get feature value '" + Description.ID + "' on object '" + rootObject + "'. See inner exception for details.", ex);
                }
            }

            internal Feature GetFeature(object rootObject)
            {
                try
                {
                    return Description.CreateFeature(GetValue(rootObject));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Cannot get feature '" + Description.ID + "' on object '" + rootObject + "'. See inner exception for details.", ex);
                }
            }

            private object AccessValue(object rootObject, string featureID)
            {
                if (rootObject == null) return null;
                using (new FeaturedObjectAccess(rootObject, featureID))
                {
                    return GetValue(rootObject);
                }
            }

            #endregion

            #region Errors

            static Exception GetPartNotFoundEx(string projection)
            {
                return new InvalidOperationException("Empty feature projection part found in '" + projection + "'.");
            } 

            #endregion
        }

        #endregion

        #region Constructor

        public ObjectFeatureProvider(IList sourceList, Strings featureProjections, ObjectFeatureFilter filter = null)
        {
            Contract.Requires(sourceList != null);
            Contract.Requires(featureProjections != null);
            Contract.Requires(sourceList.Count > 0);
            Contract.Requires(featureProjections.Count > 0);

            entries = featureProjections.Select(p => new Entry(this, p, GetObjectType(sourceList), sourceList)).ToArray();
            SourceList = ApplyFilter(sourceList, filter);
        }

        private ObjectFeatureProvider(ObjectFeatureProvider baseProvider, ObjectFeatureFilter filter)
        {
            Contract.Requires(baseProvider != null);
            Contract.Requires(filter != null);
            
            entries = baseProvider.entries;
            SourceList = ApplyFilter(baseProvider.SourceList, filter);
        }

        #endregion

        #region Fields and Properties

        Entry[] entries;

        public IList SourceList { get; private set; }

        public FeatureDescriptionSet FeatureDescriptions
        {
            get { return new FeatureDescriptionSet(entries.Select(e => e.Description)); }
        }

        #endregion

        #region Build

        private Type GetObjectType(IList sourceList)
        {
            Type listType = sourceList.GetType();
            if (listType.IsArray)
            {
                if (listType.HasElementType) return listType.GetElementType();
            }
            else if (listType.IsGenericType)
            {
                var args = listType.GetGenericArguments();
                if (args.Length == 1) return args[0];
            }

            return sourceList[0].GetType();
        }

        private IList ApplyFilter(IList sourceList, ObjectFeatureFilter filter)
        {
            Contract.Requires(entries != null);
            Contract.Requires(entries.Length > 0);

            if (filter == null) return sourceList;

            var filteredList = new ArrayList();
            for (int idx = 0; idx < sourceList.Count; idx++)
            {
                object obj = sourceList[idx];
                var values = entries.ToDictionary(e => e.Description.ID, e => e.GetValue(obj));
                if (filter(idx, values)) filteredList.Add(obj);
            }
            return filteredList.ToArray();
        }

        #endregion

        #region Provider Impl

        public virtual ObjectFeatureProvider GetObjectSubsetProvider(ObjectFeatureFilter filter)
        {
            Contract.Requires(filter != null);
            Contract.Ensures(Contract.Result<ObjectFeatureProvider>() != null);

            return new ObjectFeatureProvider(this, filter);
        }

        IDataFeatureProvider ISubsetDataFeatureProvider.GetDataSubsetProvider(params object[] subsetParameters)
        {
            var filter = subsetParameters[0] as ObjectFeatureFilter;
            if (filter == null) throw new ArgumentException("ObjectFeatureFilter parameter expected.");
            return GetObjectSubsetProvider(filter);
        }

        public int ItemCount
        {
            get { return SourceList.Count; }
        }

        public FeatureSet this[int index]
        {
            get 
            {
                object obj = SourceList[index];
                using (new FeaturedObjectAccess(obj))
                {
                    return new FeatureSet(entries.Select(e => e.GetFeature(obj)));
                }
            }
        }

        public IList<FeatureSet> GetItems(int index, int count)
        {
            var items = new List<FeatureSet>(count);
            for (int idx = index; idx < index + count; idx++)
            {
                object obj = SourceList[index];
                using (new FeaturedObjectAccess(obj))
                {
                    items.Add(new FeatureSet(entries.Select(e => e.GetFeature(obj))));
                }
            }
            return items;
        }

        public IList<FeatureSet> GetAllItems()
        {
            var all = new List<FeatureSet>(ItemCount);
            foreach (var obj in SourceList)
            {
                using (new FeaturedObjectAccess(obj))
                {
                    all.Add(new FeatureSet(entries.Select(e => e.GetFeature(obj))));
                }
            }
            return all;
        } 

        #endregion
    }
}
