using Game.Damage;
using UnityEngine;

namespace Game.Loot
{
    public class HealRuntimeLoot : RuntimeLoot<IHealth>
    {
        [SerializeField][Min(1)] private RangeFloat healAmount = new(8f, 12f);

        protected override void Consume(IHealth component)
        {
            component.Heal(healAmount.GetRandom());
        }

        protected override bool CanConsume(IHealth component)
        {
            return !component.IsMaxHealth;
        }
    }
}