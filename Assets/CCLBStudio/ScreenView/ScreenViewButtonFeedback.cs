using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CCLBStudio.ScreenView
{
    [Serializable]
    public class ScreenViewButtonFeedback
    {
        #region Editor
        #if UNITY_EDITOR
        
        public static string TargetProperty => nameof(target);
        public static string FeedbacksProperty => nameof(feedbacks);
        
        public void SetButtonEditor(Button button) => target = button;
        
        #endif
        #endregion
        
        [SerializeField] private Button target;
        [SerializeReference] private List<IScreenViewFeedback> feedbacks;

        public void Bind()
        {
            if (!target || feedbacks.Count <= 0)
            {
                return;
            }
            
            target.onClick.AddListener(PlayFeedbacks);
        }

        private void PlayFeedbacks()
        {
            foreach (var feedback in feedbacks)
            {
                feedback.PlayFeedback();
            }
        }
    }
}
