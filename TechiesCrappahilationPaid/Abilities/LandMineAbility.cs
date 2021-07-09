using Divine.Entity.Entities.Abilities;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units;
using Divine.Extensions;

using TechiesCrappahilationPaid.Helpers;

namespace TechiesCrappahilationPaid.Abilities
{
    public class LandMineAbility
    {
        public readonly Ability Ability;

        public LandMineAbility(Ability ability)
        {
            Ability = ability;
        }

        //use radius
        public DamageType DamageType { get; } = DamageType.Magical;

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