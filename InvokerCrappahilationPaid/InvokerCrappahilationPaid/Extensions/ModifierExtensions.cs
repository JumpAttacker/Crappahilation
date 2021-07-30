using System;
using System.Linq;
using Divine.Entity.Entities.Abilities;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Extensions;
using Divine.Game;
using Divine.Modifier.Modifiers;

namespace InvokerCrappahilationPaid.Extensions
{
    public static class ModifierExtensions
    {
        public static Modifier GetFirstValidModifier(this Unit target, params string[] modifiers)
        {
            return modifiers.Select(target.GetModifierByName).FirstOrDefault(mod => mod != null && mod.IsValid);
        }

        public static bool CanBeCasted(this Ability ability, float bonusMana = 0)
        {
            if (ability == null || !ability.IsValid)
            {
                return false;
            }
            //
            // var item = ability as Item;
            // if (item != null)
            // {
            //     return item.CanBeCasted(bonusMana);
            // }

            try
            {
                var owner = ability.Owner as Hero;
                bool canBeCasted;
                if (owner == null)
                {
                    canBeCasted = ability.Level > 0 && ability.Cooldown <= Math.Max(GameManager.Ping / 1000 - 0.1, 0);
                    return canBeCasted;
                }

                if (owner.NetworkName != "CDOTA_Unit_Hero_Invoker")
                {
                    canBeCasted = ability.Level > 0 && owner.Mana + bonusMana >= ability.ManaCost
                                                    && ability.Cooldown <= Math.Max(GameManager.Ping / 1000 - 0.1, 0);
                    return canBeCasted;
                }

                var name = ability.Id;
                if (name != AbilityId.invoker_invoke && name != AbilityId.invoker_quas && name != AbilityId.invoker_wex
                    && name != AbilityId.invoker_exort && ability.AbilitySlot != AbilitySlot.Slot4
                    && ability.AbilitySlot != AbilitySlot.Slot5)
                {
                    return false;
                }

                canBeCasted = ability.Level > 0 && owner.Mana + bonusMana >= ability.ManaCost
                                                && ability.Cooldown <= Math.Max(GameManager.Ping / 1000 - 0.1, 0);
                return canBeCasted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CanBeCasted(this Ability ability, Unit target)
        {
            if (ability == null || !ability.IsValid)
            {
                return false;
            }

            if (target == null || !target.IsValid)
            {
                return false;
            }

            if (!target.IsValidTarget())
            {
                return false;
            }

            var canBeCasted = ability.CanBeCasted();
            if (!target.IsMagicImmune())
            {
                return canBeCasted;
            }

            return canBeCasted;
        }

        public static bool CanHit(this Ability ability, params Unit[] targets)
        {
            if (!targets.Any())
            {
                return true;
            }

            if (ability.Owner.Distance2D(targets.First()) < ability.CastRange)
            {
                return true;
            }

            // moar checks
            return false;
        }
    }
}