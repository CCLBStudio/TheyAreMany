using System;
using System.Collections.Generic;
using System.Linq;
using CCLBStudio.DependencyInjection;
using CCLBStudio.SerializablePairs;
using CCLBStudio.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CCLBStudio.ScreenView
{
    #region Enums

    [Flags]
    public enum ActionTypeFlag
    {
        Submit = 1,
        Cancel = 2,
        Close = 4
    }
    
    /// <summary>
    /// Defines options triggered when the view is being opened.
    /// </summary>
    public enum ShowBehaviour
    {
        None,
        InsideContainer,
        SetAsLastSibling,
        InsideContainerAsLastSibling
    }
    public enum CloseBehaviour
    {
        None,
        Disable,
        Destroy
    }

    #endregion

    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenView : MonoBehaviour
    {
        #region Editor

#if UNITY_EDITOR
        
        // EDITOR ONLY FIELDS
        public static string ViewIdProperty => nameof(viewId);
        public static string TextEntriesProperty => nameof(textEntries);
        public static string SpriteEntriesProperty => nameof(spriteEntries);
        public static string GameObjectEntriesProperty => nameof(gameObjectEntries);
        public static string RectTransformEntriesProperty => nameof(rectTransformEntries);
        public static string ImageEntriesProperty => nameof(imageEntries);
        public static string ButtonEntriesProperty => nameof(buttonEntries);
        public static string CanvasGroupEntriesProperty => nameof(canvasGroupEntries);
        public static string ShowAnimationProperty => nameof(showAnimations);
        public static string CloseAnimationProperty => nameof(closeAnimations);
        public static string OnShowAnimCompletedProperty => nameof(onShowAnimationCompleted);
        public static string OnShowProperty => nameof(onShow);
        public static string OnCloseProperty => nameof(onClose);
        public static string OnCloseAnimCompletedProperty => nameof(onCloseAnimationCompleted);
        public static string DisableInteractionOnProperty => nameof(disableInteractionOn);
        public static string AutoCloseOnSubmitProperty => nameof(autoCloseOnSubmit);
        public static string BlockRaycastUntilCloseCompletedProperty => nameof(blockRaycastUntilCloseCompleted);
        public static string AutoCloseOnCancelProperty => nameof(autoCloseOnCancel);
        public static string SubmitButtonsProperty => nameof(submitButtons);
        public static string CancelButtonsProperty => nameof(cancelButtons);
        public static string CloseButtonsProperty => nameof(closeButtons);
        public static string CustomButtonsProperty => nameof(customButtons);
        public static string CloseBehaviourProperty => nameof(closeBehaviour);
        public static string DisplayBehaviourProperty => nameof(showBehaviour);
        public static string ContainerIdProperty => nameof(containerId);
        public static string EntriesSectionExpandedProperty => nameof(entriesSectionExpanded);
        public static string AnimationSectionExpandedProperty => nameof(animationSectionExpanded);
        public static string DisplayShowProperty => nameof(displayShow);
        public static string DisplayCloseProperty => nameof(displayClose);
        public static string DisableAndCloseSectionExpandedProperty => nameof(disableAndCloseSectionExpanded);
        public static string MainButtonsSectionExpandedProperty => nameof(mainButtonsSectionExpanded);
        public static string DisplaySectionExpandedProperty => nameof(displaySectionExpanded);
        public static string ButtonFeedbacksSectionExpandedProperty => nameof(buttonFeedbacksSectionExpanded);
        public static string ContainerIndexProperty => nameof(containerIndex);
        public static string OptiSectionExpandedProperty => nameof(optimizationSectionExpanded);
        public static string EnableOptiProperty => nameof(enableOptimization);
        public static string UpdateTargetFrameRateProperty => nameof(updateTargetFrameRate);
        public static string NewTargetFrameRateProperty => nameof(newTargetFrameRate);
        public static string DisableLowerCanvasProperty => nameof(disableCanvasWithLowerSortOrder);
        public static string DisableMainCamProperty => nameof(disableMainCamera);
        public static string ButtonFeedbacksProperty => nameof(buttonFeedbacks);

        [SerializeField] private bool entriesSectionExpanded = true;
        [SerializeField] private bool animationSectionExpanded = true, displayShow = true, displayClose = true;
        [SerializeField] private bool mainButtonsSectionExpanded = true;
        [SerializeField] private bool disableAndCloseSectionExpanded = true;
        [SerializeField] private bool displaySectionExpanded = true;
        [SerializeField] private bool buttonFeedbacksSectionExpanded = true;
        [SerializeField] private bool optimizationSectionExpanded = true;
    
#endif

        #endregion

        public UnityEvent OnShow => onShow;
        public UnityEvent OnClose => onClose;
        public UnityEvent OnShowAnimationCompleted => onShowAnimationCompleted;
        public UnityEvent OnCloseAnimationCompleted => onCloseAnimationCompleted;
        
        /// <summary>
        /// True if a submit button has been clicked. Value is reset to false every time the Show method is called.
        /// </summary>
        public bool Submitted { get; private set; }
        public string ContainerId => containerId;

        [Tooltip("The id of this screen view. It must be unique. It can be used as parameter to show or close the view.")]
        [SerializeField] private ViewId viewId = ViewId.Empty;
        [Tooltip("A list of TMP entries linked to a key, that allows us to access those fields.")]
        [SerializeField] private List<SerializableStringValuePair<TextMeshProUGUI>> textEntries;
        [Tooltip("A list of sprite entries linked to a key, that allows us to access those fields.")]
        [SerializeField] private List<SerializableStringValuePair<Sprite>> spriteEntries;
        [Tooltip("A list of game object entries linked to a key, that allows us to access those fields.")]
        [SerializeField] private List<SerializableStringValuePair<GameObject>> gameObjectEntries;
        [SerializeField] private List<SerializableStringValuePair<RectTransform>> rectTransformEntries;
        [Tooltip("A list of image entries linked to a key, that allows us to access those fields.")]
        [SerializeField] private List<SerializableStringValuePair<Image>> imageEntries;
        [Tooltip("A list of button entries linked to a key, that allows us to access those fields.")]
        [SerializeField] private List<SerializableStringValuePair<Button>> buttonEntries;
        [Tooltip("A list of canvas group entries linked to a key, that allows us to access those fields.")]
        [SerializeField] private List<SerializableStringValuePair<CanvasGroup>> canvasGroupEntries;
        
        [Tooltip("Animations played when the Play() method is called.")]
        [SerializeReference] private List<IScreenViewAnimation> showAnimations;
        [Tooltip("Animations played when the Close() method is called.")]
        [SerializeReference] private List<IScreenViewAnimation> closeAnimations;
        [Tooltip("Event raised when the Show method is called.")]
        [SerializeField] private UnityEvent onShow;
        [Tooltip("Event raised when the Show animation has completed.")]
        [SerializeField] private UnityEvent onShowAnimationCompleted;
        [Tooltip("Event raised when the Close method is called.")]
        [SerializeField] private UnityEvent onClose;
        [Tooltip("Event raised when the Close animation has completed.")]
        [SerializeField] private UnityEvent onCloseAnimationCompleted;

        [Tooltip("All the buttons that will automatically call the OnSubmit action when clicked. Will close the view if the related option is toggled.")]
        [SerializeField] private List<Button> submitButtons;
        [Tooltip("All the buttons that will automatically call the OnCancel action when clicked. Will close the view if the related option is toggled.")]
        [SerializeField] private List<Button> cancelButtons;
        [Tooltip("All the buttons that will automatically close the view when clicked.")]
        [SerializeField] private List<Button> closeButtons;
        [Tooltip("List of Button/Event pairs. Each button will have the related Event added to its onClick actions.")]
        [SerializeField] private List<SerializableKeyValuePair<Button, UnityEvent>> customButtons;

        [Tooltip("Determine which events will disable the interactions. Interactions will be enabled again the next time the Show() method is called.")]    
        [SerializeField] private ActionTypeFlag disableInteractionOn;
        [Tooltip("If true, clicking a submit button will also close the view.")]
        [SerializeField] private bool autoCloseOnSubmit = true;
        [Tooltip("If true, the view will block any raycast until the close animation has completed. Otherwise the view will stop blocking raycasts when the Close method is called.")]
        [SerializeField] private bool blockRaycastUntilCloseCompleted = true;
        [Tooltip("If true, clicking a cancel button will also close the view.")]
        [SerializeField] private bool autoCloseOnCancel;

        [Tooltip("Determines how and where the view will displayed when the Show() method is called.")]
        [SerializeField] private ShowBehaviour showBehaviour = ShowBehaviour.InsideContainerAsLastSibling;
        [Tooltip("The container in which the view will be displayed.")]
        [SerializeField] private string containerId;
        [SerializeField] private int containerIndex;
        [Tooltip("Determines how the view behaves when to Close() method has completed (i.e. the animations have finished).")]
        [SerializeField] private CloseBehaviour closeBehaviour = CloseBehaviour.Disable;
        
        [Tooltip("Manage button feedbacks that will be played on click.")]
        [SerializeField] private List<ScreenViewButtonFeedback> buttonFeedbacks;

        [Tooltip("If true, the optimization options defined below will be applied. All the settings will be reverted when the view closes.")]
        [SerializeField] private bool enableOptimization;
        [Tooltip("If true, the screen view will modify the target frame rate on show, once the animation is completed. Useful when most of the world behind is hidden and doesn't need to refresh as much as usual.")]
        [SerializeField] private bool updateTargetFrameRate;
        [Tooltip("The new target frame rate to apply.")]
        [Range(30, 144)][SerializeField] private int newTargetFrameRate = 30;
        [Tooltip("If true, the view will disable all the UI Containers stored in the service that have a lower sort order.")]
        [SerializeField] private bool disableCanvasWithLowerSortOrder;
        [Tooltip("If true, the view will disable on show the camera stored in the context, once the animation is completed. Useful when the view completely hides the world behind.")]
        [SerializeField] private bool disableMainCamera;

        private bool _isDisplayed;
        private bool _isAnimating;
        private bool _isInit;
        protected CanvasGroup canvasGroup;
        protected Canvas canvas;

        public Action onSubmit;
        public Action onCancel;

        [NonSerialized][Inject]
        private ScreenViewService _screenViewService;

        private ScreenViewAnimator _showAnimator;
        private ScreenViewAnimator _closeAnimator;

        private OptimizationSettings _revertSettings;

        #region Unity Events

        protected void Start()
        {
            if (_isInit)
            {
                return;
            }
            
            Initialize();
        }
        
        protected virtual void OnDestroy()
        {
            _screenViewService.RemoveView(this);
        }

        #endregion

        #region Initialization

        protected virtual void Initialize()
        {
            _isInit = true;
            Injector.InjectNewConsumer(this);
            
            VerifyContainer();
            InitializeAnimations();
            InitializeButtonFeedbacks();
            InitializeButtons();

            _screenViewService.AddView(this);
            
            canvasGroup = GetComponent<CanvasGroup>();
            var container = _screenViewService.GetContainer(containerId);
            canvas = container ? container.Canvas : GetComponentInParent<Canvas>();
        }

        private void VerifyContainer()
        {
            if (showBehaviour == ShowBehaviour.None || showBehaviour == ShowBehaviour.SetAsLastSibling)
            {
                containerId = string.Empty;
                return;
            }
            
            // If our containerId is not empty we can assume that we expect a container with this id to exist.
            // Otherwise we can just return.
            if (string.IsNullOrEmpty(containerId))
            {
                return;
            }
            
            // First we check if we can access a container with our id. If it's the case, we can return.
            var container = _screenViewService.GetContainer(containerId);
            if (container)
            {
                return;
            }
            
            // If we did not found a container with our id, we use our saved index to try to find one.
            container = _screenViewService.GetContainer(containerIndex);
            if (container)
            {
                // If we found one, we reassign our id.
                containerId = container.Id;
            }
        }

        private void InitializeAnimations()
        {
            _showAnimator = new ScreenViewAnimator();
            _closeAnimator = new ScreenViewAnimator();
            
            _showAnimator.Initialize(showAnimations);
            _closeAnimator.Initialize(closeAnimations);

            _showAnimator.OnLaunch += _closeAnimator.Kill;
            _showAnimator.OnCompleted += ExecuteOnShowCompleted;
            
            _closeAnimator.OnLaunch += _showAnimator.Kill;
            _closeAnimator.OnCompleted += ExecuteOnCloseCompleted;
            
            if (enableOptimization)
            {
                _revertSettings = new OptimizationSettings
                {
                    disabledContainers = new List<ScreenViewContainerHandler>(),
                    frameRate = Application.targetFrameRate
                };
                
                _showAnimator.OnCompleted += ApplyOptimization;
                _closeAnimator.OnLaunch += RevertOptimization;
            }

            switch (closeBehaviour)
            {
                case CloseBehaviour.None:
                    break;
                
                case CloseBehaviour.Disable:
                    _closeAnimator.OnCompleted += DisableView;
                    break;
                
                case CloseBehaviour.Destroy:
                    _closeAnimator.OnCompleted += DestroyView;
                    break;
            }
        }

        private void InitializeButtonFeedbacks()
        {
            foreach (var feedback in buttonFeedbacks.WithoutNull())
            {
                feedback.Bind();
            }
        }

        private void DestroyView()
        {
            Destroy(gameObject);
        }

        private void DisableView()
        {
            gameObject.SetActive(false);
        }

        private void InitializeButtons()
        {
            foreach (var submitBtn in submitButtons)
            {
                if (autoCloseOnSubmit)
                {
                    submitBtn.onClick.AddListener(SelfClose);
                }

                if ((disableInteractionOn & ActionTypeFlag.Submit) == ActionTypeFlag.Submit)
                {
                    submitBtn.onClick.AddListener(() => canvasGroup.interactable = false);
                }

                submitBtn.onClick.AddListener(() => Submitted = true);
                submitBtn.onClick.AddListener(ExecuteOnSubmit);
            }
            
            foreach (var cancelBtn in cancelButtons)
            {
                if (autoCloseOnCancel)
                {
                    cancelBtn.onClick.AddListener(SelfClose);
                }

                if ((disableInteractionOn & ActionTypeFlag.Cancel) == ActionTypeFlag.Cancel)
                {
                    cancelBtn.onClick.AddListener(() => canvasGroup.interactable = false);
                }
                
                cancelBtn.onClick.AddListener(ExecuteOnCancel);
            }
            
            foreach (var closeBtn in closeButtons)
            {
                closeBtn.onClick.AddListener(SelfClose);
            }

            foreach (var pair in customButtons)
            {
                pair.Key.onClick.AddListener(() => pair.Value?.Invoke());
            }
        }

        #endregion
        
        /// <summary>
        /// Method called through the Ping() method of the service.
        /// </summary>
        /// <param name="ping">The custom messaged passed as a parameter in the service method.</param>
        public virtual void Ping(string ping) { }
        
        /// <summary>
        /// Warning: that function shouldn't be used outside the service scope!
        /// </summary>
        internal virtual void Show(Action onSubmitCallback = null, Action onCancelCallback = null)
        {
            if (!_isInit)
            {
                Initialize();
            }
            
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 1f;
            gameObject.SetActive(true);
            _isDisplayed = true;
            Submitted = false;
            
            onSubmit = onSubmitCallback;
            onCancel = onCancelCallback;

            HandleContainer();
            HandleShowAnimation();
            onShow?.Invoke();
        }

        private void HandleShowAnimation()
        {
            _isAnimating = true;
            _showAnimator.Launch();
        }

        private void HandleCloseAnimation()
        {
            _isAnimating = true;
            _closeAnimator.Launch();
        }

        private void HandleContainer()
        {
            if (showBehaviour == ShowBehaviour.None || showBehaviour == ShowBehaviour.SetAsLastSibling)
            {
                return;
            }

            var container = _screenViewService.GetContainer(containerId);
            if (!container)
            {
                Debug.LogError($"Container with id {containerId} not found. Will use first container instead ({_screenViewService.GetContainer(0).name})");
                container = _screenViewService.GetContainer(0);
            }

            container.ViewAdded(this);
        }

        private void ExecuteOnSubmit()
        {
            onSubmit?.Invoke();
        }
        
        private void ExecuteOnCancel()
        {
            onCancel?.Invoke();
        }

        private void ExecuteOnShowCompleted()
        {
            _isAnimating = false;
            onShowAnimationCompleted?.Invoke();
            _screenViewService.NotifyViewShown(this);
        }
        
        private void ExecuteOnCloseCompleted()
        {
            _isAnimating = false;
            canvasGroup.blocksRaycasts = false;
            onCloseAnimationCompleted?.Invoke();
            _screenViewService.NotifyViewHidden(this);
        }

        private void ApplyOptimization()
        {
            _revertSettings.disabledContainers.Clear();

            if (updateTargetFrameRate)
            {
                Debug.Log($"Applying frame rate optimization. New frame rate: {newTargetFrameRate}");
                Application.targetFrameRate = newTargetFrameRate;
            }

            if (disableCanvasWithLowerSortOrder)
            {
                var toDisable = _screenViewService.ExistingContainers.Where(x => x.Canvas.enabled && x.Canvas.sortingOrder < canvas.sortingOrder);
                foreach (ScreenViewContainerHandler container in toDisable)
                {
                    container.DisableCanvas();
                    _revertSettings.disabledContainers.Add(container);
                }
            }

            if (disableMainCamera)
            {
                Debug.LogError("Update main camera not implemented yet.");
            }
        }

        private void RevertOptimization()
        {
            if (updateTargetFrameRate)
            {
                Debug.Log($"reverting frame rate optimization. New frame rate: {_revertSettings.frameRate}");
                Application.targetFrameRate = _revertSettings.frameRate;
            }

            if (disableCanvasWithLowerSortOrder)
            {
                foreach (var container in _revertSettings.disabledContainers)
                {
                    container.EnableCanvasIfAnythingDisplayed();
                }
            }
            
            _revertSettings.disabledContainers.Clear();
        }
        
        /// <summary>
        /// Warning: that function shouldn't be used outside the service scope!
        /// </summary>
        internal virtual void Close()
        {
            _isDisplayed = false;
            HandleCloseAnimation();

            canvasGroup.blocksRaycasts = blockRaycastUntilCloseCompleted;
            if ((disableInteractionOn & ActionTypeFlag.Close) == ActionTypeFlag.Close)
            {
                canvasGroup.interactable = false;
            }
            
            onClose?.Invoke();
        }

        /// <summary>
        /// Closes this view. Does nothing if the view is closing.
        /// </summary>
        public void SelfClose()
        {
            if (!_isDisplayed)
            {
                return;
            }
            
            _screenViewService.Close(this);
        }
        
        #region field methods

        public ScreenView SetTextField(string key, string value, bool setAllTextFieldsWithMatchingKey = false)
        {
            foreach (var e in textEntries.Where(x => x.Key.Equals(key)))
            {
                e.Value.SetText(value);
                if (!setAllTextFieldsWithMatchingKey) return this;
            }

            return this;
        }

        public ScreenView SetImageFromSprite(string key, Sprite sprite, bool setAllImagesWithMatchingKey = false)
        {
            foreach (var e in imageEntries.Where(e => e.Key.Equals(key)))
            {
                e.Value.overrideSprite = sprite;
                if (!setAllImagesWithMatchingKey) return this;
            }

            return this;
        }

        public ScreenView SetImageFromSpriteKey(string imageKey, string spriteKey)
        {
            Sprite sprite = GetSpriteEntryByKey(spriteKey);
            if (!sprite) return this;

            foreach (var e in imageEntries.Where(e => e.Key.Equals(imageKey)))
            {
                e.Value.overrideSprite = sprite;
                return this;
            }

            return this;
        }

        public TextMeshProUGUI GetTextEntryByKey(string key)
        {
            return textEntries.FirstOrDefault(textEntry => key.Equals(textEntry.Key))?.Value;
        }

        public GameObject GetGameObjectEntryByKey(string key)
        {
            return gameObjectEntries.FirstOrDefault(objectEntry => key.Equals(objectEntry.Key))?.Value;
        }

        public RectTransform GetRectTransformEntryByKey(string key)
        {
            return rectTransformEntries.FirstOrDefault(rectEntry => key.Equals(rectEntry.Key))?.Value;
        }
        
        public Sprite GetSpriteEntryByKey(string key)
        {
            return spriteEntries.FirstOrDefault(spriteEntry => key.Equals(spriteEntry.Key))?.Value;
        }
        
        public CanvasGroup GetCanvasGroupEntryByKey(string key)
        {
            return canvasGroupEntries.FirstOrDefault(spriteEntry => key.Equals(spriteEntry.Key))?.Value;
        }

        public Image GetImageEntryByKey(string key)
        {
            return imageEntries.FirstOrDefault(imageEntry => key.Equals(imageEntry.Key))?.Value;
        }
        
        public Button GetButtonEntryByKey(string key)
        {
            return buttonEntries.FirstOrDefault(buttonEntry => key.Equals(buttonEntry.Key))?.Value;
        }

        #endregion
        
        #region Getters
        
        public ViewId GetId() { return viewId; }
        public List<SerializableStringValuePair<TextMeshProUGUI>> GetTextEntries() { return textEntries; }
        public List<SerializableStringValuePair<Sprite>> GetSpriteEntries() { return spriteEntries; }
        public List<SerializableStringValuePair<CanvasGroup>> GetCanvasGroupEntries() { return canvasGroupEntries; }
        public List<SerializableStringValuePair<GameObject>> GetGameObjectEntries() { return gameObjectEntries; }
        public List<SerializableStringValuePair<RectTransform>> GetRectTransformEntries() { return rectTransformEntries; }
        public List<SerializableStringValuePair<Image>> GetImageEntries() { return imageEntries; }
        public List<SerializableStringValuePair<Button>> GetButtonsEntries() {return buttonEntries; }
        public ShowBehaviour GetShowMode() { return showBehaviour; }

        public bool IsDisplayed => _isDisplayed;
        public bool IsDisplayedOrAnimating => _isDisplayed || _isAnimating;
        public bool IsClosing => !_isDisplayed && _isAnimating;
        public bool IsClosedAndAnimationCompleted => !_isDisplayed && !_isAnimating;
        public bool IsAnimating => _isAnimating;

        #endregion

        #region Setters

        public void EnableInteraction(bool value) => canvasGroup.blocksRaycasts = value;

        #endregion

        private struct OptimizationSettings
        {
            public List<ScreenViewContainerHandler> disabledContainers;
            public int frameRate;
        }
    }
}
