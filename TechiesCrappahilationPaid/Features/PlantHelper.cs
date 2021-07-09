using System.Collections.Generic;

using Divine.Menu.Items;
using Divine.Numerics;
using Divine.Particle;

namespace TechiesCrappahilationPaid.Features
{
    public class PlantHelper
    {
        private readonly TechiesCrappahilationPaid _main;
        private readonly List<Vector3> _list;
        private MenuSlider _range;

        public PlantHelper(TechiesCrappahilationPaid main)
        {
            _main = main;

            _list = new List<Vector3>()
            {
                new Vector3(-4364, -3901, 384), new Vector3(-6591, -3075, 383), new Vector3(-3568, -6167, 376),
                new Vector3(-1389, -5230, 384), new Vector3(-2227, -2545, 256), new Vector3(-3308, -1431, 384),
                new Vector3(-906, -1483, 256), new Vector3(-588, -2026, 256), new Vector3(925, -2836, 384),
                new Vector3(1464, -4263, 384), new Vector3(2290, -4797, 383), new Vector3(1026, -5357, 384),
                new Vector3(3706, -3337, 256), new Vector3(5323, -5199, 255), new Vector3(5656, -3924, 256),
                new Vector3(5541, -2028, 256), new Vector3(3712, -2089, 256), new Vector3(4110, -1038, 376),
                new Vector3(2371, -1247, 256), new Vector3(3088, -887, 384), new Vector3(3401, 92, 384),
                new Vector3(3572, 962, 384), new Vector3(5125, -138, 381), new Vector3(5554, 1263, 384),
                new Vector3(1009, 2236, 384), new Vector3(-272, 1821, 384), new Vector3(-1735, 3676, 384),
                new Vector3(1501, 4175, 383), new Vector3(456, 5074, 384), new Vector3(-1223, 4849, 384),
                new Vector3(-951, 3809, 384), new Vector3(-2560, 3747, 255), new Vector3(-3624, 3730, 255),
                new Vector3(-3016, 3654, 128), new Vector3(-4059, 1919, 256), new Vector3(-2384, 1800, 159),
                new Vector3(-1710, 1152, 128), new Vector3(-2068, 505, 255), new Vector3(-2841, 296, 384),
                new Vector3(-2077, -75, 256), new Vector3(1139, 1169, 256), new Vector3(1621, 77, 256),
                new Vector3(1409, 117, 256), new Vector3(2389, -1841, 128), new Vector3(-4593, 715, 384),
                new Vector3(-4326, 102, 384), new Vector3(-5299, 379, 384), new Vector3(-5187, -1619, 384),
                new Vector3(-5454, 2247, 255), new Vector3(-5873, 3921, 256), new Vector3(-5696, 4473, 256),
                new Vector3(-5332, 5005, 256), new Vector3(-6565, 5236, 256), new Vector3(-6829, 3761, 256),
                new Vector3(-5930, 2071, 256), new Vector3(-2602, 5720, 256),
                new Vector3(-1185, 5668, 256), new Vector3(2382, -3767, 384), new Vector3(-843, -3988, 384),
                new Vector3(-2534, -3585, 256), new Vector3(-2185, -4082, 256), new Vector3(-1547, -3293, 256),
                new Vector3(-1058, -2581, 256), new Vector3(5119, -3360, 256),
                new Vector3(6821, -3270, 256)
            };

            var enable = main.MenuManager.GoodPositions.CreateSwitcher("Show good position for planting", false);
            _range = main.MenuManager.GoodPositions.CreateSlider("Range ", 425, 25, 425);

            _range.ValueChanged += (sender, args) => { Draw(); };

            /*if (enable)
            {
                Draw();
            }*/

            enable.ValueChanged += (sender, args) =>
            {
                if (args.Value)
                {
                    Draw();
                }
                else
                {
                    var index = 0;
                    foreach (var pos in _list)
                    {
                        ParticleManager.RemoveParticle($"{index++}_pos_helper");
                    }
                }
            };
        }

        private void Draw()
        {
            var index = 0;
            foreach (var pos in _list)
            {
                ParticleManager.CircleParticle($"{index++}_pos_helper", pos, _range,
                    Color.Purple);
            }
        }
    }
}