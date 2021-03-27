using System;
using System.Linq;
using System.Threading.Tasks;
using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;

namespace TechiesCrappahilationPaid.Abilities
{
    public class SuicideAbility : CircleAbility
    {
        public SuicideAbility(Ability ability) : base(ability)
        {
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
        public override DamageType DamageType { get; } = DamageType.Magical;

        public override UnitState AppliesUnitState { get; } = UnitState.Silenced;

        public override float GetDamage(params Unit[] targets)
        {
            if (!targets.Any()) return RawDamage;
            return base.GetDamage(targets);
        }

        public float GetDamage(Unit target)
        {
            return DamageHelpers.GetSpellDamage(Ability.GetAbilitySpecialData("damage") + ExtraDamage,
                Owner.GetSpellAmplification(),
                Ability.GetDamageReduction(target, DamageType));
        }
    }
}