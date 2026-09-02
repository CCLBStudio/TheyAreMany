using System;
using System.Collections.Generic;
using System.Linq;
using CCLBStudio.DependencyInjection;
using UnityEngine;
using UnityEngine.UI;

namespace CCLBStudio.ScreenView
{
    public class ScreenViewContainerHandler : SelfInjectedMonoBehaviour, IViewServiceListener
    {
        #region Editor

        #if UNITY_EDITOR

        public static string CanvasProperty => nameof(canvas);
        public static string ContainerProperty => nameof(container);
        
        #endif

        #endregion

        public string Id { get; set; } = string.Empty;
        public Canvas Canvas => canvas;
        
        [Tooltip("The canvas related to this container. Is used as the default container if none is specified in the Container field.")]
        [SerializeField] protected Canvas canvas;
        [Tooltip("The actual container in which the related views will be instantiated.")]
        [SerializeField] protected Transform container;

        protected GraphicRaycaster raycaster;
        protected List<ScreenView> displayedViews;
        
        [NonSerialized][Inject]
        private ScreenViewService _screenViewService;


        protected override void Start()
        {
            base.Start();
            
            raycaster = canvas.GetComponent<GraphicRaycaster>();
            displayedViews = new List<ScreenView>();
            DisableCanvas();

            _screenViewService.AddServiceListener(this);
        }

        protected virtual void OnDestroy()
        {
            _screenViewService.RemoveServiceListener(this);
        }

        public virtual void ViewAdded(ScreenView view)
        {
            displayedViews.Add(view);
            ReplaceView(view);
            EnableCanvas();
        }

        public void EnableCanvas()
        {
            if(canvas.enabled) return;
            
            Debug.Log($"--- SV CONTAINER --- Container {Id} will enable its canvas.");

            canvas.enabled = true;
            if (raycaster)
            {
                raycaster.enabled = true;
            }
        }

        public void DisableCanvas()
        {
            if(!canvas.enabled) return;
            
            Debug.Log($"--- SV CONTAINER --- Container {Id} will disable its canvas.");

            canvas.enabled = false;
            if (raycaster)
            {
                raycaster.enabled = false;
            }
        }

        protected void ReplaceView(ScreenView view)
        {
            var viewObj = view.transform;

            switch (view.GetShowMode())
            {
                case ShowBehaviour.None:
                    break;
                
                case ShowBehaviour.SetAsLastSibling:
                    viewObj.SetAsLastSibling();
                    break;
                
                case ShowBehaviour.InsideContainer:
                    viewObj.SetParent(container, false);
                    break;
                
                case ShowBehaviour.InsideContainerAsLastSibling:
                    viewObj.SetParent(container, false);
                    viewObj.SetAsLastSibling();
                    break;
            }
        }

        public void DisableCanvasIfNothingDisplayed()
        {
            if (displayedViews.Count <= 0)
            {
                DisableCanvas();
                return;
            }
            
            if (displayedViews.All(x => x.IsClosedAndAnimationCompleted))
            {
                DisableCanvas();
            }
        }

        public void EnableCanvasIfAnythingDisplayed()
        {
            if (displayedViews.Count <= 0)
            {
                return;
            }
            
            EnableCanvas();
        }

        public void OnViewShown(ScreenView view)
        {
            EnableCanvasIfAnythingDisplayed();
        }

        public void OnViewClosed(ScreenView view)
        {
            int index = displayedViews.FindIndex(x => x == view);
            if (index < 0)
            {
                return;
            }
            
            displayedViews.RemoveAt(index);
            DisableCanvasIfNothingDisplayed();
        }
    }
}
