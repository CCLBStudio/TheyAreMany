#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.ScriptableValue
{
    public abstract class ScriptableValue<T> : BaseScriptableValue
    {
        #if UNITY_EDITOR

        public static string ValueProperty => nameof(value);
        public static string InitialValueProperty => nameof(initialValue);
        public override Type TargetType => typeof(T);

#endif
        
        public T Value
        {
            get => value;
            set => SetValue(value);
        }

        public event Action<T> OnValueChanged;

        [SerializeField] protected T value;
        
        #if UNITY_EDITOR
        [SerializeField] protected T initialValue;
        #endif

        protected virtual void SetValue(T newValue)
        {
            value = newValue;
            OnValueChanged?.Invoke(value);
        }
        
#if UNITY_EDITOR

        private void CopyValueToInitialValue()
        {
            CopyFromTo(ref value, ref initialValue);
        }

        private void CopyInitialValueToValue()
        {
            CopyFromTo(ref initialValue, ref value);
        }

        private void CopyFromTo(ref T from, ref T to)
        {
            if (IsTargetTypeArray())
            {
                var elementType = TargetType.GetElementType();
                var originalArray = from as Array;
                if (elementType == null || originalArray == null)
                {
                    return;
                }
                
                var newArray = Array.CreateInstance(elementType, originalArray.Length);
                Array.Copy(originalArray, newArray, originalArray.Length);
                to = (T)(object)newArray;
                return;
            }

            if (IsTargetTypeList())
            {
                var newList = (IList)Activator.CreateInstance(TargetType);
                var originalList = (IList)from;
                if (null == originalList)
                {
                    return;
                }
                
                foreach (var item in originalList)
                {
                    if (item is ICloneableValue cloneableValue)
                    {
                        newList.Add(cloneableValue.Clone());
                        continue;
                    }
                    
                    newList.Add(item);
                }

                to = (T)newList;
                return;
            }
            
            to = from;
        }
        
        private void OnEnable()
        {
            CopyValueToInitialValue();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                CopyInitialValueToValue();
            }
        }
        
#endif
    }
}