using System;
using System.Collections.Generic;
using System.Linq;
using Divine;
using Divine.SDK.Extensions;
using SharpDX;

namespace TechiesCrappahilationPaid.Helpers
{
    public static class CustomExtensions
    {
        public static bool CanDie(this Hero hero, bool checkForAegis = false)
        {
            var mod = !hero.HasModifiers(
                          new[]
                          {
                              "modifier_dazzle_shallow_grave", "modifier_oracle_false_promise",
                              "modifier_skeleton_king_reincarnation_scepter_active", "modifier_abaddon_borrowed_time"
                          },
                          false) &&
                      (hero.NetworkName != ClassId.CDOTA_Unit_Hero_Abaddon.ToString() ||
                       !CanBeCasted(hero.GetAbilityById(AbilityId.abaddon_borrowed_time)));
            
            if (checkForAegis)
                return mod && hero.GetItemById(AbilityId.item_aegis)!=null &&
                       (hero.NetworkName != ClassId.CDOTA_Unit_Hero_SkeletonKing.ToString() ||
                        !CanBeCasted(hero.GetAbilityById(AbilityId.skeleton_king_reincarnation)));
            return mod;
        }

        public static float ManaPercent(this Unit unit)
        {
            return (float) unit.Mana / (float) unit.MaximumMana;
        }

        public static string ToCopyFormat(this object obj)
        {
            switch (obj)
            {
                case null:
                    return string.Empty;
                case Enum _:
                    return obj.GetType().Name + "." + obj;
                case Vector3 v3:
                    return (int) v3.X + ", " + (int) v3.Y + ", " + (int) v3.Z;
                default:
                    return obj.ToString();
            }
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
                    && name != AbilityId.invoker_exort && ability.AbilitySlot != AbilitySlot.Slot_4
                    && ability.AbilitySlot != AbilitySlot.Slot_5)
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

        /// <summary>
        ///     Checks if given ability can be used
        /// </summary>
        /// <param name="ability">
        ///     The ability.
        /// </param>
        /// <param name="target">
        ///     The target.
        /// </param>
        /// <returns>
        ///     returns true in case ability can be used
        /// </returns>
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
        
        /// <summary>
        ///     The rot speed dictionary.
        /// </summary>
        public static Dictionary<float, double> RotSpeedDictionary = new Dictionary<float, double>();
        
        /// <summary>
        ///     Checks if a unit is currently changing their direction
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        /// <param name="tolerancy">
        ///     tolerancy of rotation speed
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsTurning(this Unit unit, double tolerancy = 0)
        {
            double rotSpeed;
            if (!RotSpeedDictionary.TryGetValue(unit.Handle, out rotSpeed))
            {
                return false;
            }

            return Math.Abs(rotSpeed) > tolerancy;
        }
        
        // public static Vector3 InFront(this Unit unit, float distance)
        // {
        //     var v = unit.Position + (unit.Vector3FromPolarAngle() * distance);
        //     return new Vector3(v.X, v.Y, 0);
        // }
        public static Vector3 InFrontSuper(this Entity unit, float distance)
        {
            var alpha = unit.RotationRad;
            var vector2FromPolarAngle = SharpDXExtensions.FromPolarCoordinates(1f, alpha);

            var v = unit.Position + (vector2FromPolarAngle.ToVector3() * distance);
            return new Vector3(v.X, v.Y, 0);
        }
    }
}