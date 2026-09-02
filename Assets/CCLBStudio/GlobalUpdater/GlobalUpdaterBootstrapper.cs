using CCLBStudio.Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace CCLBStudio.GlobalUpdater
{
    internal static class GlobalUpdaterBootstrapper
    {
        private static PlayerLoopSystem _defaultUpdaterSystem;
        private static PlayerLoopSystem _fixedUpdaterSystem;
        private static PlayerLoopSystem _lateUpdaterSystem;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!InsertAllUpdaters(ref currentPlayerLoop))
            {
                return;
            }
            
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            PlayerLoopExtender.PrintPlayerLoop(currentPlayerLoop);
            
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;
#endif

            static void OnPlayModeState(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                    RemoveAllUpdaters(ref currentPlayerLoop);
                    PlayerLoop.SetPlayerLoop(currentPlayerLoop);
                    
                    DefaultUpdater.Clear();
                    FixedUpdater.Clear();
                    LateUpdater.Clear();
                }
            }
        }

        private static bool InsertAllUpdaters(ref PlayerLoopSystem currentPlayerLoop)
        {
            if (!InsertDefaultUpdater<Update>(ref currentPlayerLoop, 0))
            {
                Debug.LogError("Unable to register global updater into the Update player loop system.");
                return false;
            }

            if (!InsertFixedUpdater<FixedUpdate>(ref currentPlayerLoop, 0))
            {
                Debug.LogError("Unable to register global fixed updater into the FixedUpdate player loop system.");
                return false;
            }
            
            if (!InsertLateUpdater<PreLateUpdate>(ref currentPlayerLoop, 0))
            {
                Debug.LogError("Unable to register global fixed updater into the FixedUpdate player loop system.");
                return false;
            }

            return true;
        }

        private static void RemoveAllUpdaters(ref PlayerLoopSystem currentPlayerLoop)
        {
            RemoveDefaultUpdater<Update>(ref currentPlayerLoop);
            RemoveFixedUpdater<FixedUpdate>(ref currentPlayerLoop);
            RemoveLateUpdater<PreLateUpdate>(ref currentPlayerLoop);
        }
        
        private static bool InsertDefaultUpdater<T>(ref PlayerLoopSystem loop, int index)
        {
            _defaultUpdaterSystem = new PlayerLoopSystem
            {
                type = typeof(DefaultUpdater),
                updateDelegate = DefaultUpdater.TickUpdatables,
                subSystemList = null
            };
            
            return PlayerLoopExtender.InsertSystem<T>(ref loop, in _defaultUpdaterSystem, index);
        }

        private static void RemoveDefaultUpdater<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopExtender.RemoveSystem<T>(ref loop, _defaultUpdaterSystem);
        }

        private static bool InsertFixedUpdater<T>(ref PlayerLoopSystem loop, int index)
        {
            _fixedUpdaterSystem = new PlayerLoopSystem
            {
                type = typeof(FixedUpdater),
                updateDelegate = FixedUpdater.TickFixedUpdatables,
                subSystemList = null
            };
            
            return PlayerLoopExtender.InsertSystem<T>(ref loop, in _fixedUpdaterSystem, index);
        }
        
        private static void RemoveFixedUpdater<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopExtender.RemoveSystem<T>(ref loop, _fixedUpdaterSystem);
        }
        
        private static bool InsertLateUpdater<T>(ref PlayerLoopSystem loop, int index)
        {
            _lateUpdaterSystem = new PlayerLoopSystem
            {
                type = typeof(LateUpdater),
                updateDelegate = LateUpdater.TickLateUpdatables,
                subSystemList = null
            };
            
            return PlayerLoopExtender.InsertSystem<T>(ref loop, in _lateUpdaterSystem, index);
        }
        
        private static void RemoveLateUpdater<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopExtender.RemoveSystem<T>(ref loop, _lateUpdaterSystem);
        }
    }
}
