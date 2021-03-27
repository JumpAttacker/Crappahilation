using System;
using Ensage;
using Ensage.Common.Enums;
using Ensage.SDK.Extensions;
using SharpDX;
using AbilityExtensions = Ensage.Common.Extensions.AbilityExtensions;
using UnitExtensions = Ensage.Common.Extensions.UnitExtensions;

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
                       !AbilityExtensions.CanBeCasted(hero.GetAbilityById(Ensage.AbilityId.abaddon_borrowed_time)));
            if (checkForAegis)
                return mod && !UnitExtensions.HasItem(hero, ItemId.item_aegis) &&
                       (hero.NetworkName != ClassId.CDOTA_Unit_Hero_SkeletonKing.ToString() ||
                        !AbilityExtensions.CanBeCasted(
                            hero.GetAbilityById(Ensage.AbilityId.skeleton_king_reincarnation)));
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
    }
}