using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Logical
{
    public class LogicGateTypes : ISet<LogicGateType>
    {
        #region Construct

        // TODO: Type validation.

        public LogicGateTypes(LogicGateType type, params LogicGateType[] otherTypes)
        {
            types = new HashSet<LogicGateType>();
            types.Add(type);
            if (otherTypes != null) foreach (var ot in otherTypes) types.Add(ot);
            Init();
        }

        public LogicGateTypes(IEnumerable<LogicGateType> typeCollection)
        {
            Contract.Requires(typeCollection != null);

            types = new HashSet<LogicGateType>();
            foreach (var type in typeCollection) types.Add(type);

            if (types.Count == 0) throw new ArgumentException("typeCollection", "Type collection is empty.");

            Init();
        }

        public LogicGateTypes(IList<LogicGateType> typeList)
        {
            Contract.Requires(typeList != null && typeList.Count > 0);

            types = new HashSet<LogicGateType>();
            foreach (var type in typeList) types.Add(type);
            Init();
        }

        void Init()
        {
            Operations = new HashSet<LogicalOperation>(types.Select(t => t.Operation).Distinct());
        }

        #endregion

        #region Type Field

        HashSet<LogicGateType> types;

        public HashSet<LogicalOperation> Operations { get; private set; }

        #endregion

        #region Set

        public bool Add(LogicGateType item)
        {
            throw GetROEx();
        }

        public void ExceptWith(IEnumerable<LogicGateType> other)
        {
            throw GetROEx();
        }

        public void IntersectWith(IEnumerable<LogicGateType> other)
        {
            throw GetROEx();
        }

        public bool IsProperSubsetOf(IEnumerable<LogicGateType> other)
        {
            return types.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<LogicGateType> other)
        {
            return types.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<LogicGateType> other)
        {
            return types.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<LogicGateType> other)
        {
            return types.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<LogicGateType> other)
        {
            return types.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<LogicGateType> other)
        {
            return types.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<LogicGateType> other)
        {
            throw GetROEx();
        }

        public void UnionWith(IEnumerable<LogicGateType> other)
        {
            throw GetROEx();
        }

        void ICollection<LogicGateType>.Add(LogicGateType item)
        {
            throw GetROEx();
        }

        public void Clear()
        {
            throw GetROEx();
        }

        public bool Contains(LogicGateType item)
        {
            return types.Contains(item);
        }

        public void CopyTo(LogicGateType[] array, int arrayIndex)
        {
            types.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return types.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(LogicGateType item)
        {
            throw GetROEx();
        }

        public IEnumerator<LogicGateType> GetEnumerator()
        {
            return types.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        static InvalidOperationException GetROEx()
        {
            return new InvalidOperationException("LogicGateTypes set is read only.");
        }

        #endregion
    }
}
