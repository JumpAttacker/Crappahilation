using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;

namespace TechiesCrappahilationPaid.Abilities
{
    public class RemoteMineAbility : CircleAbility
    {
        public RemoteMineAbility(Ability ability) : base(ability)
        {
            HasAgh = Owner.HasAghanimsScepter();
        }

        //use radius
        public override DamageType DamageType { get; } = DamageType.Magical;

        public bool HasAgh { get; set; }

        public float GetDamage(Unit target)
        {
            var damage = GetDamage(true);
            var reduction = Ability.GetDamageReduction(target, DamageType);
            return DamageHelpers.GetSpellDamage(damage, 0f, reduction);
        }
        public float GetDamage(float damage, Unit target)
        {
            var reduction = Ability.GetDamageReduction(target, DamageType);
            return DamageHelpers.GetSpellDamage(damage, 0f, reduction);
        }

        public float GetDamage(bool calcWithAgh = false)
        {
            return Ability.GetAbilitySpecialData("damage") + (calcWithAgh ? HasAgh ? 150 : 0 : 0);
        }
    }
}