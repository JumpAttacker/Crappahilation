using System;
using System.Collections.Generic;
using System.Linq;
using Divine;

using TechiesCrappahilationPaid.Features;

namespace TechiesCrappahilationPaid.BombsType
{
    public class Stacker
    {
        public bool IsActive => Counter > 0;
        public int Counter;
        public readonly Dictionary<HeroId, bool> DetonateDict;

        public Stacker()
        {
            Counter = 0;
            DetonateDict = new Dictionary<HeroId, bool>();
            foreach (var target in TargetManager.Targets.GroupBy(x => x.HeroId))
            {
                DetonateDict.Add(target.Key, true);
                Console.WriteLine("adding..." + target.Key);
            }
        }

        public void UpdateDetonateDict()
        {
            foreach (var target in TargetManager.Targets.GroupBy(x => x.HeroId))
            {
                if (!DetonateDict.TryGetValue(target.Key, out _))
                {
                    DetonateDict.Add(target.Key, true);
                    Console.WriteLine("adding..." + target.Key);
                }
            }
        }
    }
}