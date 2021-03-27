using System;
using Divine;

namespace TechiesCrappahilationPaid.Helpers
{
    public static class DamageCalcHelpers
    {
        public static void CalcDamageForDusa(ref float dmg, Hero hero, float treshold)
        {
            float burst;
            if (hero.Mana >= dmg * .6 / treshold)
                burst = 0.6f;
            else
                burst = hero.Mana * treshold / dmg;
            dmg *= 1 - burst;
        }

        public static void CalcDamageForDusa(ref float dmg, ref float mana, float treshold)
        {
            float burst;
            if (mana >= dmg * .6 / treshold)
                burst = 0.6f;
            else
                burst = mana * treshold / dmg;
            var dmgWas = dmg;
            dmg *= 1 - burst;
            var blocked = dmgWas - dmg;
            mana = Math.Abs(mana - mana * blocked / treshold);
        }

    }
}