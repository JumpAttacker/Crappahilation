using System.Linq;
using System.Threading.Tasks;
using Divine;
using Divine.SDK.Extensions;
using TechiesCrappahilationPaid.Helpers;

namespace TechiesCrappahilationPaid.Abilities
{
    public class SuicideAbility
    {
        public Ability Ability { get; }
        public Hero Owner { get; set; }

        public SuicideAbility(Ability ability)
        {
            Ability = ability;
            Owner = ability.Owner as Hero;
            UpdateManager.BeginInvoke(async () =>
            {
                var extraDamage = Owner.GetAbilityById(AbilityId.special_bonus_unique_techies);

                while (extraDamage.Level == 0)
                {
                    await Task.Delay(500);
                }

                ExtraDamage = extraDamage.GetAbilitySpecialData("value");
            });
        }

        private float ExtraDamage { get; set; } = 0;
        public DamageType DamageType { get; } = DamageType.Magical;

        public UnitState AppliesUnitState { get; } = UnitState.Silenced;

        public float GetDamage(params Unit[] targets)
        {
            var damage = Ability.GetAbilitySpecialData("damage");
            if (targets==null || !targets.Any()) return damage;
            return GetDamage(targets);
        }

        public float GetDamage(Unit target)
        {
            return DamageHelpers.GetSpellDamage(Ability.GetAbilitySpecialData("damage") + ExtraDamage,
                Owner.GetSpellAmplification(),
                Ability.GetDamageReduction(target, DamageType));
        }
    }
}