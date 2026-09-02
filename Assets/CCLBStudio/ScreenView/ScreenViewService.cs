using System;
using System.Collections.Generic;
using System.Linq;
using CCLBStudio.SerializablePairs;
using CCLBStudio.Utils;
using Scripts.Services;
using UnityEngine;
using Utils;

namespace CCLBStudio.ScreenView
{
    [CreateAssetMenu(menuName = "CCLB Studio/Screen View/Service SO", fileName = "ScreenViewService")]
    public class ScreenViewService : AutoProvidedService<ScreenViewService>
    {
#region Editor Properties
#if UNITY_EDITOR
 
        public static string ScreenViewPrefabsProperty => nameof(screenViewPrefabs);
        public static string UiContainersProperty => nameof(uiContainers);
        public static string ButtonClickSoundProperty => nameof(defaultClickSound);
        
        public Dictionary<ScreenViewContainerHandler, List<ScreenView>> GetContainersViewMapEditor()
        {
            Dictionary<ScreenViewContainerHandler, List<ScreenView>> result = new Dictionary<ScreenViewContainerHandler, List<ScreenView>>();
            Dictionary<string, ScreenViewContainerHandler> containerMap = new Dictionary<string, ScreenViewContainerHandler>();

            foreach (var pair in uiContainers.Dictionary)
            {
                ScreenViewContainerHandler handler = pair.Value.GetComponent<ScreenViewContainerHandler>();
                if (!handler)
                {
                    continue;
                }

                if (result.ContainsKey(handler))
                {
                    continue;
                }
                
                result.Add(handler, new List<ScreenView>());
                containerMap.Add(pair.Key, handler);
            }

            foreach (ScreenView view in screenViewPrefabs.Select(x => x.GetComponent<ScreenView>()).Where(v => v))
            {
                if (containerMap.TryGetValue(view.ContainerId, out var container))
                {
                    result[container].Add(view);
                }
            }

            return result;
        }

#endif
#endregion

        public List<GameObject> ScreenViewPrefabs => screenViewPrefabs;
        public List<string> ContainerIds => uiContainers.GetKeys();
        public List<ScreenViewContainerHandler> ExistingContainers => _containers.Values.ToList();
        public AudioClip DefaultClickSound => defaultClickSound;

        [SerializeField] private List<GameObject> screenViewPrefabs;
        [SerializeField] private SerializableDictionary<string, GameObject> uiContainers;
        
        [Header("Feedback Settings")]
        [SerializeField] private AudioClip defaultClickSound;
        
        private ObserverWrapper<IViewServiceListener> _screenViewObserver;
        private Dictionary<ViewId, GameObject> _screenViews;
        private Dictionary<ViewId, ScreenView> _existingViews;
        private Dictionary<string, ScreenViewContainerHandler> _containers;
        [NonSerialized] private ScreenViewContainerHolder _containerHolder;

        #region Initialization

        public override void Initialize()
        {
            _existingViews = new Dictionary<ViewId, ScreenView>();
            _screenViewObserver = new ObserverWrapper<IViewServiceListener>();
            _screenViews = new Dictionary<ViewId, GameObject>(screenViewPrefabs.Count);
            _containers = new Dictionary<string, ScreenViewContainerHandler>(uiContainers.Count);
            
            screenViewPrefabs.ClearNull();
            foreach (var prefab in screenViewPrefabs)
            {
                var sv = prefab.GetComponentInChildren<ScreenView>();
                if (!sv)
                {
                    continue;
                }

                _screenViews.Add(sv.GetId(), prefab);
            }
            
            _containerHolder = new GameObject("ContainerHolder").AddComponent<ScreenViewContainerHolder>();
            
            foreach (var container in uiContainers.Dictionary)
            {
                var handler = _containerHolder.InstantiateNewContainer(container.Value);
                handler.Id = container.Key;
                _containers.Add(container.Key, handler);
            }
        }

        #endregion

        #region Show / Close Methods

        /// <summary>
        /// A function to show a view, specified by an ID. If the view is already registered, it will be shown and
        /// inform the service listeners. If it's not, it means that it's not yet instantiated, so we instantiate it
        /// and we inform the service listeners.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="onSubmit"></param>
        /// <param name="onCancel"></param>
        public ScreenView Show(ViewId id, Action onSubmit = null, Action onCancel = null)
        {
            // first, we check if the view is already registered. If it is, we simply show it
            // and we inform the listeners 
            ScreenView view = GetExistingViewById(id);
            if (view)
            {
                view.Show(onSubmit, onCancel);
                //NotifyViewShown(view);
                
                return view;
            }

            // if the view isn't registered, we check into the prefabs of the service settings.
            // If we find it, we spawn it and we inform the listeners
            GameObject objView = GetScreenViewPrefab(id);
            if (objView)
            {
                ScreenView instView = Instantiate(objView).GetComponent<ScreenView>();
                
                instView.Show(onSubmit, onCancel);
                AddView(instView);
                
                return instView;
            }

            Debug.LogError($"Unable to show view with id {id}. View not found in prefabs.");
            return null;
        }

        public ScreenView Show(ScreenView view, Action onSubmit = null, Action onCancel = null)
        {
            if (GetExistingViewById(view.GetId()))
            {
                view.Show(onSubmit, onCancel);
                return view;
            }
            
            AddView(view);
            view = Instantiate(view.gameObject).GetComponent<ScreenView>();
            view.Show(onSubmit, onCancel);
            return view;
        }
        
        public ScreenView Close(ViewId id)
        {
            ScreenView view = GetExistingViewById(id);
            if (!view)
            {
                Debug.Log($"--- SCREEN VIEW --- Unable to close view with id {id} (no view found).");
                return null;
            }
            
            view.Close();
            
            return view;
        }

        public void Close(ScreenView view)
        {
            if (!view)
            {
                Debug.Log($"--- SCREEN VIEW --- Unable to close view due to null parameter.");
                return;
            }
            
            view.Close();
        }

        public void CloseAll()
        {
            foreach (var view in GetAllOpenedScreenView())
            {
                Close(view);
            }
        }

        #endregion

        public void PingViews(string ping)
        {
            foreach (ScreenView view in _existingViews.Values)
            {
                view.Ping(ping);
            }
        }
        
        public List<ScreenView> GetAllOpenedScreenView()
        {
            return _existingViews.Values.Where(view => view.IsDisplayed).ToList();
        }

        public bool IsViewOpened(ViewId id)
        {
            return _existingViews.TryGetValue(id, out var view) && view.IsDisplayed;
        }

        public ScreenViewContainerHandler GetContainer(string id)
        {
            return _containers.TryGetValue(id, out var container) ? container : null;
        }
        
        public ScreenViewContainerHandler GetContainer(int index)
        {
            if (index < 0 || index >= _containers.Count)
            {
                return null;
            }

            return _containers.ElementAt(index).Value;
        }

        public void AddView(ScreenView screenView)
        {
            _existingViews.TryAdd(screenView.GetId(), screenView);
        }

        public void RemoveView(ScreenView screenView)
        {
            if (screenView && _existingViews.ContainsKey(screenView.GetId()))
            {
                _existingViews.Remove(screenView.GetId());
            }
        }

        #region Listener Methods

        public void NotifyViewShown(ScreenView view)
        {
            _screenViewObserver.NotifyListeners(nameof(IViewServiceListener.OnViewShown), new object[] {view});
        }
        
        public void NotifyViewHidden(ScreenView view)
        {
            _screenViewObserver.NotifyListeners(nameof(IViewServiceListener.OnViewClosed), new object[] {view});
        }
        
        public void AddServiceListener(IViewServiceListener listener)
        {
            _screenViewObserver.AddListener(listener);
        }

        public void RemoveServiceListener(IViewServiceListener listener)
        {
            _screenViewObserver.RemoveListener(listener);
        }

        public void RemoveAllServiceListeners()
        {
            _screenViewObserver.ClearListeners();
        }

        #endregion
        
        public ScreenView GetExistingViewById(ViewId id)
        {
            return _existingViews.GetValueOrDefault(id);
        }

        public int GetDisplayedScreenViewCount()
        {
            return _existingViews.Count(view => view.Value.IsDisplayed);
        }

        public List<ScreenView> GetAllDisplayedViews()
        {
            return _existingViews.Values.Where(v => v.IsDisplayed).ToList();
        }

        private GameObject GetScreenViewPrefab(ViewId id)
        {
            return _screenViews.GetValueOrDefault(id);
        }
    }
}
