using Divine.Entity.Entities.Abilities;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Extensions;

using TechiesCrappahilationPaid.Helpers;

namespace TechiesCrappahilationPaid.Abilities
{
    public class RemoteMineAbility
    {
        public Ability Ability { get; }

        public RemoteMineAbility(Ability ability)
        {
            Ability = ability;
            var owner = ability.Owner as Hero;
            HasAgh = owner.HasAghanimsScepter();
        }

        //use radius
        public DamageType DamageType { get; } = DamageType.Magical;

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