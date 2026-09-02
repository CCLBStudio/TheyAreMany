using System;

namespace CCLBStudio.ScreenView
{
    public interface IScreenViewAnimation
    {
        public event Action OnCompleted;
        public void Animate();
        public void Kill();
        public float GetTotalTime();

        /// <summary>
        /// This method is invoked by the editor when creating an instance of an object that implements the
        /// <see cref="IScreenViewAnimation"/> interface. It is intended for performing setup operations or custom initialization
        /// logic during the creation phase within the editor workflow.
        /// </summary>
        /// <param name="view">The <see cref="ScreenView"/> instance associated with the created animation object, providing
        /// contextual data or settings that may be required for initialization.</param>
        /// <remarks>
        /// Override this method in a derived class to implement any required setup logic that should occur during the creation
        /// of the animation object by the Unity Editor. It is typically used alongside the ScreenViewEditor to handle animation
        /// setup and configuration.
        /// </remarks>
        public virtual void OnEditorCreated(ScreenView view)
        {
        }
    }
}
