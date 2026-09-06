namespace Game.Enemies
{
    public interface IDeathAnimation
    {
        public void TriggerDeathAnimation(IDamageSource killer);
    }
}
