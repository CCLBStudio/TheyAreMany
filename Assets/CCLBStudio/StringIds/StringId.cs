using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CCLBStudio.StringIds
{
    /// <summary>
    /// Base class wrapping a string id into a typed value.
    /// Concrete ids (ViewId, ...) declare their values as <c>public static readonly</c> fields of their own type,
    /// which the editor tooling (StringIdEditorGUI / StringIdDrawer) can list, add and delete automatically.
    /// Instances are immutable: never assign a shared static instance (e.g. <c>Empty</c>) as a serialized field initializer, use <c>new()</c> instead.
    /// </summary>
    [Serializable]
    public abstract class StringId : IEquatable<StringId>
    {
        #region Editor
        #if UNITY_EDITOR

        public static string IdProperty => nameof(id);

        #endif
        #endregion

        [SerializeField] private string id = string.Empty;

        public string Id => id ?? string.Empty;
        public bool IsEmpty => string.IsNullOrEmpty(id);

        protected StringId() { }
        protected StringId(string id) => this.id = id ?? string.Empty;

        public override string ToString() => IsEmpty ? "<None>" : id;

        public bool Equals(StringId other) => other is not null && GetType() == other.GetType() && string.Equals(Id, other.Id, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is StringId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(GetType(), Id);
        public static bool operator ==(StringId a, StringId b) => a is null ? b is null : a.Equals(b);
        public static bool operator !=(StringId a, StringId b) => !(a == b);

        /// <summary>
        /// Returns every non-empty id declared as a public static readonly field of <paramref name="idType"/>.
        /// </summary>
        public static IEnumerable<StringId> GetAll(Type idType)
        {
            return GetIdFields(idType)
                .Select(f => (StringId)f.GetValue(null))
                .Where(v => v is not null && !v.IsEmpty)
                .Distinct();
        }

        public static IEnumerable<T> GetAll<T>() where T : StringId => GetAll(typeof(T)).Cast<T>();

        public static IEnumerable<FieldInfo> GetIdFields(Type idType)
        {
            return idType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(f => f.IsInitOnly && f.FieldType == idType);
        }
    }
}
