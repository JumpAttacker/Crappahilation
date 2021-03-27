using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;

namespace TechiesCrappahilationPaid.Abilities
{
    public class LandMineAbility : CircleAbility
    {
        public LandMineAbility(Ability ability) : base(ability)
        {
        }

        //use radius
        public override DamageType DamageType { get; } = DamageType.Magical;

        public float GetDamage(Unit target)
        {
            var damage = GetDamage();
            var reduction = Ability.GetDamageReduction(target, DamageType);

            return DamageHelpers.GetSpellDamage(damage, 0f, reduction);
        }
        public float GetDamage(float damage, Unit target)
        {
            var reduction = Ability.GetDamageReduction(target, DamageType);

            return DamageHelpers.GetSpellDamage(damage, 0f, reduction);
        }

        public float GetDamage()
        {
            return Ability.GetAbilitySpecialData("damage");
        }
    }
}