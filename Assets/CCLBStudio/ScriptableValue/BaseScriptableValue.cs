using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.ScriptableValue
{
    public abstract class BaseScriptableValue : ScriptableObject
    {
        #region Editor
        #if UNITY_EDITOR

        public abstract Type TargetType { get; }
        public bool TargetTypeIsList => IsTargetTypeList();

        protected virtual bool IsTargetTypeArray()
        {
            return TargetType.IsArray;
        }

        protected virtual bool IsTargetTypeList()
        {
            return TargetType.IsGenericType && TargetType.GetGenericTypeDefinition() == typeof(List<>);
        }

        #endif
        #endregion
    }
}