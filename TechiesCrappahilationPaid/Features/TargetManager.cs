using System.Collections.Generic;
using System.Linq;
using Divine.Entity;
using Divine.Update;
using Divine.Entity.Entities.Units;
using Divine.Entity.Entities.Units.Heroes;

namespace TechiesCrappahilationPaid.Features
{
    public static class TargetManager
    {
        public static readonly List<Hero> Targets = new List<Hero>();

        public static void Init(TechiesCrappahilationPaid main)
        {
            var me = main.Me;
            UpdateManager.CreateUpdate(500, () =>
            {
                var enemies = EntityManager.GetEntities<Hero>().Where(x =>
                    x.IsValid && x.Team != me.Team && !x.IsIllusion && IsHeroOrDangerUnit(x));
                foreach (var enemy in enemies)
                {
                    if (Targets.Contains(enemy))
                        continue;
                    Targets.Add(enemy);
                    main.MenuManager.Targets.AddValue(enemy.HeroId, true);
                    foreach (var bombManagerRemoteMine in main.Updater.BombManager.RemoteMines)
                    {
                        bombManagerRemoteMine.Stacker.UpdateDetonateDict();
                    }
                }

                foreach (var enemy in Targets.ToList().Where(enemy => enemy is null || !enemy.IsValid))
                {
                    Targets.Remove(enemy);
                }
            });
        }

        private static bool IsHeroOrDangerUnit(Unit unit)
        {
            return unit is Hero || unit.NetworkName == "CDOTA_Unit_SpiritBear";
        }
    }
}