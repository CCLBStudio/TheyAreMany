namespace CCLBStudio.ScreenView
{
    public interface IViewServiceListener
    {
        void OnViewShown(ScreenView view);
        void OnViewClosed(ScreenView view);
    }
}