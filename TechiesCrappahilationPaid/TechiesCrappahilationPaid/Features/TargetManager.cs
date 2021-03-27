using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.SDK.Helpers;

namespace TechiesCrappahilationPaid.Features
{
    public static class TargetManager
    {
        public static readonly List<Hero> Targets = new List<Hero>();

        public static void Init(TechiesCrappahilationPaid main)
        {
            var me = main.Me;
            UpdateManager.Subscribe(() =>
            {
                var enemies = EntityManager<Hero>.Entities.Where(x =>
                    x.IsValid && x.Team != me.Team && !x.IsIllusion && IsHeroOrDangerUnit(x));
                foreach (var enemy in enemies)
                {
                    if (Targets.Contains(enemy))
                        continue;
                    Targets.Add(enemy);
                    main.MenuManager.Targets.Value.Add(enemy.HeroId.ToString());
                    foreach (var bombManagerRemoteMine in main.Updater.BombManager.RemoteMines)
                    {
                        bombManagerRemoteMine.Stacker.UpdateDetonateDict();
                    }
                }

                foreach (var enemy in Targets.ToList())
                {
                    if (enemy is null || !enemy.IsValid)
                        Targets.Remove(enemy);
                }
            }, 500);
        }

        private static bool IsHeroOrDangerUnit(Unit unit)
        {
            return unit is Hero || unit.NetworkName == "CDOTA_Unit_SpiritBear";
        }
    }
}