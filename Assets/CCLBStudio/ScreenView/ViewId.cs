using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [Serializable]
    public struct ViewId : IEquatable<ViewId>
    {
        #region Editor
        #if UNITY_EDITOR
        
        public static string IdProperty => nameof(id);
        
        #endif
        #endregion
        public readonly string Id => id;

        [SerializeField] private string id;
        
        public static readonly ViewId Empty = new("");
        public static readonly ViewId Monid = new("Monid");
        public ViewId(string viewId) => id = viewId;

        public override string ToString() => string.IsNullOrEmpty(id) ? "<None>" : id;
        public bool Equals(ViewId other) => id.Equals(other.id, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ViewId other && Equals(other);
        public override int GetHashCode() => id?.GetHashCode() ?? 0;
        public static bool operator ==(ViewId a, ViewId b) => a.Equals(b);
        public static bool operator !=(ViewId a, ViewId b) => !a.Equals(b);

        public static IEnumerable<ViewId> GetAll()
        {
            return typeof(ViewId)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(ViewId))
                .Select(f => (ViewId)f.GetValue(null));
        }
    }
}
