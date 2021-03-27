using System.Collections.Generic;
using System.Linq;
using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using SharpDX;
using TechiesCrappahilationPaid.Helpers;

namespace TechiesCrappahilationPaid.Features
{
    public static class AutoPlanter
    {
        public static List<Vector3> LandMinePositionRadiant = new List<Vector3>()
        {
//            new Vector3(-5823.125f, -6582.5f, 384f), new Vector3(-5942.25f, -6106.688f, 384f),
//            new Vector3(-6334.906f, -5955.188f, 384f), new Vector3(-6682.031f, -5698.125f, 384f),
//            new Vector3(-6271.281f, -5540.5f, 384f), new Vector3(-5810.875f, -5674.563f, 384f),
//            new Vector3(-5365.781f, -6618.188f, 384f), new Vector3(-5529.594f, -6230.219f, 384f),
//            new Vector3(-5372.906f, -5826.188f, 384f), new Vector3(-5398.969f, -5409.156f, 384f),
//            new Vector3(-5698.688f, -5102f, 384f), new Vector3(-6254.656f, -5093.406f, 384f),
//            new Vector3(-6686.813f, -5235.781f, 384f), new Vector3(-7106.656f, -5173.688f, 384f),
//            new Vector3(-6843.563f, -4842.375f, 384f), new Vector3(-6431.688f, -4718.938f, 384f),
//            new Vector3(-6000.094f, -4731.75f, 384f), new Vector3(-5572.031f, -4742f, 384f),
//            new Vector3(-5195.438f, -4981.094f, 384f), new Vector3(-4975.875f, -5357.125f, 384f),
//            new Vector3(-4943.469f, -5825.719f, 384f), new Vector3(-5099.156f, -6234.5f, 384f),
//            new Vector3(-4933.906f, -6633.438f, 384f), new Vector3(-4671f, -6296.375f, 384f),
//            new Vector3(-4522.25f, -5899.281f, 384f), new Vector3(-4567.281f, -5477.375f, 384f),
//            new Vector3(-4602.688f, -5053.469f, 384f), new Vector3(-4869.75f, -4695.438f, 384f),
//            new Vector3(-5235.844f, -4473.281f, 384f), new Vector3(-5541.625f, -4183.156f, 384f),
//            new Vector3(-5924.469f, -3989.906f, 384f), new Vector3(-6193.469f, -4349.906f, 384f),
//            new Vector3(-7067.813f, -4490.25f, 384f), new Vector3(-6620.469f, -4338.25f, 384f),
//            new Vector3(-6821.313f, -3944.188f, 384f), new Vector3(-6377.406f, -3962.406f, 384f),
//            new Vector3(-6138.344f, -3601.094f, 384f), new Vector3(-7005.25f, -3548.375f, 384f),
//            new Vector3(-6553.313f, -3572f, 384f), new Vector3(-6775.125f, -3169.563f, 384f),
//            new Vector3(-6345.031f, -3157.313f, 384f), new Vector3(-5912.063f, -3234.813f, 384f),
//            new Vector3(-5708.719f, -3605.625f, 384f), new Vector3(-5297.75f, -3832.781f, 384f),
//            new Vector3(-5485.938f, -3232.063f, 384f), new Vector3(-5110.531f, -3465.844f, 384f),
//            new Vector3(-4876.219f, -3821.969f, 384f), new Vector3(-4873.938f, -4260.719f, 384f),
//            new Vector3(-4504.031f, -4477.594f, 384f), new Vector3(-4248.656f, -4819.375f, 384f),
//            new Vector3(-4166.5f, -5246.875f, 384f), new Vector3(-4156.438f, -5616.688f, 384f),
//            new Vector3(-4119.25f, -6052.813f, 384f), new Vector3(-4504.563f, -6696.906f, 384f),
//            new Vector3(-4127.375f, -6480.063f, 384f), new Vector3(-3745.781f, -6679.906f, 384f),
//            new Vector3(-3757f, -6267.844f, 384f), new Vector3(-3738.563f, -5849.219f, 384f),
//            new Vector3(-3780.188f, -5427.563f, 384f), new Vector3(-3832.563f, -5009.469f, 384f),
//            new Vector3(-3871.25f, -4590.375f, 384f), new Vector3(-4143.75f, -4275f, 384f),
//            new Vector3(-4507.844f, -4049.906f, 384f)
            new Vector3(-6334.906f, -5955.25f, 384f), new Vector3(-6271.281f, -5540.5f, 384f),
            new Vector3(-6682.031f, -5698.125f, 384f), new Vector3(-6686.875f, -5235.781f, 384f),
            new Vector3(-6254.656f, -5093.406f, 384f), new Vector3(-6000.125f, -4731.75f, 384f),
            new Vector3(-6193.5f, -4349.906f, 384f), new Vector3(-6620.5f, -4338.25f, 384f),
            new Vector3(-6377.406f, -3962.406f, 384f), new Vector3(-6821.375f, -3944.25f, 384f),
            new Vector3(-6553.375f, -3572f, 384f), new Vector3(-7005.25f, -3548.375f, 384f),
            new Vector3(-6775.125f, -3169.625f, 384f), new Vector3(-6345.031f, -3157.375f, 384f),
            new Vector3(-6138.375f, -3601.125f, 384f), new Vector3(-5912.125f, -3234.875f, 384f),
            new Vector3(-5708.75f, -3605.625f, 384f), new Vector3(-5486f, -3232.125f, 384f),
            new Vector3(-5297.75f, -3832.781f, 384f), new Vector3(-5541.625f, -4183.156f, 384f),
            new Vector3(-5924.5f, -3989.906f, 384f), new Vector3(-5235.875f, -4473.281f, 384f),
            new Vector3(-5572.031f, -4742f, 384f), new Vector3(-5195.5f, -4981.125f, 384f),
            new Vector3(-4869.75f, -4695.5f, 384f), new Vector3(-4602.75f, -5053.5f, 384f),
            new Vector3(-4975.875f, -5357.125f, 384f), new Vector3(-4567.281f, -5477.375f, 384f),
            new Vector3(-4943.5f, -5825.75f, 384f), new Vector3(-4522.25f, -5899.281f, 384f),
            new Vector3(-4671f, -6296.375f, 384f), new Vector3(-5099.156f, -6234.5f, 384f),
            new Vector3(-4933.906f, -6633.5f, 384f), new Vector3(-5365.781f, -6618.25f, 384f),
            new Vector3(-5529.625f, -6230.25f, 384f), new Vector3(-5823.125f, -6582.5f, 384f),
            new Vector3(-5942.25f, -6106.75f, 384f), new Vector3(-5810.875f, -5674.625f, 384f),
            new Vector3(-5372.906f, -5826.25f, 384f), new Vector3(-5399f, -5409.156f, 384f),
            new Vector3(-4156.5f, -5616.75f, 384f), new Vector3(-4119.25f, -6052.875f, 384f),
            new Vector3(-3738.625f, -5849.25f, 384f), new Vector3(-3757f, -6267.875f, 384f),
            new Vector3(-4127.375f, -6480.125f, 384f), new Vector3(-3755.781f, -6707.625f, 384f),
            new Vector3(-4504.625f, -6696.906f, 384f), new Vector3(-3780.25f, -5427.625f, 384f),
            new Vector3(-3832.625f, -5009.5f, 384f), new Vector3(-4248.656f, -4819.375f, 384f),
            new Vector3(-3871.25f, -4590.375f, 384f), new Vector3(-4141.625f, -4244.75f, 384f),
            new Vector3(-4504.031f, -4477.625f, 384f), new Vector3(-4483.875f, -3965.031f, 384f),
            new Vector3(-4874f, -4260.75f, 384f), new Vector3(-4876.25f, -3822f, 384f),
            new Vector3(-6843.625f, -4842.375f, 384f), new Vector3(-7067.875f, -4490.25f, 384f),
            new Vector3(-7106.656f, -5173.75f, 384f), new Vector3(-5129.188f, -3401.25f, 384f)
        };

        public static List<Vector3> LandMinePositionDire = new List<Vector3>()
        {
            new Vector3(6133.25f, 6083.25f, 384f), new Vector3(6453.594f, 5771.625f, 384f),
            new Vector3(6068.188f, 5544.219f, 384f), new Vector3(6468.5f, 5328.938f, 384f),
            new Vector3(6432.094f, 4882.563f, 384f), new Vector3(5962.219f, 4957.75f, 384f),
            new Vector3(5914.938f, 4514f, 384f), new Vector3(6358.563f, 4447.375f, 384f),
            new Vector3(6159.469f, 4046f, 384f), new Vector3(6617.25f, 4043f, 384f),
            new Vector3(6425f, 3625.625f, 384f), new Vector3(6870.563f, 3634.25f, 384f),
            new Vector3(6793.625f, 3203.875f, 384f), new Vector3(7206.469f, 3220f, 384f),
            new Vector3(6980f, 2824.719f, 384f), new Vector3(6561.938f, 2750.844f, 384f),
            new Vector3(6367.625f, 3192.719f, 384f), new Vector3(6146.969f, 2753f, 384f),
            new Vector3(5947.5f, 3174.313f, 384f), new Vector3(5725.469f, 2795.094f, 384f),
            new Vector3(5520.625f, 3266.969f, 384f), new Vector3(5316.875f, 2871.063f, 384f),
            new Vector3(4914.375f, 2981.844f, 384f), new Vector3(5097.188f, 3382.313f, 384f),
            new Vector3(4647.594f, 3581.875f, 384f), new Vector3(4526f, 3146.313f, 384f),
            new Vector3(4328.25f, 3919.125f, 384f), new Vector3(4745.75f, 4032.688f, 384f),
            new Vector3(4427.188f, 4357.188f, 384f), new Vector3(4863.844f, 4565.438f, 384f),
            new Vector3(4540.813f, 4871.219f, 384f), new Vector3(4968.719f, 5007.25f, 384f),
            new Vector3(4889.688f, 5449.344f, 384f), new Vector3(4453.063f, 5304f, 384f),
            new Vector3(4080.5f, 5534.813f, 384f), new Vector3(3924.875f, 5116.563f, 384f),
            new Vector3(3498.813f, 5043.875f, 384f), new Vector3(3467.344f, 5456.5f, 384f),
            new Vector3(3089.563f, 5220.625f, 384f), new Vector3(3099.313f, 5681.438f, 384f),
            new Vector3(3080.969f, 6146.344f, 384f), new Vector3(3493.344f, 6280f, 384f),
            new Vector3(3686f, 5894.094f, 384f), new Vector3(3920.625f, 6350.844f, 384f),
            new Vector3(4115.594f, 5961f, 384f), new Vector3(4488.875f, 6169.375f, 384f),
            new Vector3(4475.844f, 6635.438f, 384f), new Vector3(4049.344f, 6764.125f, 384f),
            new Vector3(3532.063f, 6701.5f, 384f), new Vector3(3077.625f, 6620.813f, 384f),
            new Vector3(4898.094f, 6338.25f, 384f), new Vector3(4951.438f, 6767.25f, 384f),
            new Vector3(5371.719f, 6602.875f, 384f), new Vector3(5309.844f, 6133.438f, 384f),
            new Vector3(5738.438f, 6259.813f, 384f), new Vector3(5727.5f, 5829.25f, 384f),
            new Vector3(5297.313f, 5666.844f, 384f), new Vector3(5650.219f, 5390.75f, 384f),
            new Vector3(4941.875f, 5907.438f, 384f), new Vector3(5834.938f, 6687.938f, 384f),
            new Vector3(7008.563f, 5320.375f, 384f), new Vector3(6884.813f, 4888.25f, 384f),
            new Vector3(7343.188f, 4906.75f, 384f), new Vector3(7304.844f, 4451.75f, 384f),
            new Vector3(6812f, 4441.75f, 384f), new Vector3(7088.125f, 4043f, 384f),
            new Vector3(7335.563f, 3648.094f, 384f), new Vector3(5987.094f, 3620.344f, 384f),
            new Vector3(5560.313f, 3703.5f, 384f), new Vector3(5717.813f, 4119.25f, 384f),
            new Vector3(4134.344f, 4690.063f, 384f), new Vector3(3715.125f, 4597.875f, 384f),
            new Vector3(3331.375f, 4383f, 384f), new Vector3(3618.688f, 4010.875f, 384f),
            new Vector3(3991f, 4242f, 384f), new Vector3(3908.563f, 3711.594f, 384f),
            new Vector3(3147.469f, 4787.563f, 384f), new Vector3(5131.188f, 3827.188f, 384f),
            new Vector3(5481.313f, 4588.438f, 384f), new Vector3(5181.031f, 4265.063f, 384f)
        };


        private static List<Vector3> GetMinesPositions =>
            Me.Team == Team.Dire ? LandMinePositionDire : LandMinePositionRadiant;

        public static Particle Effect;
        public static Particle Effect2;
        public static Vector3 LastPosition;
        public static Vector3 LastPosition2;
        private static TechiesCrappahilationPaid _main;
        private static Hero Me => _main.Me;

        public static bool IsAutoMovingToStaticTraps =>
            _main.Me.Spellbook.GetSpellByName("special_bonus_unique_techies_4")?.Level > 0 && MinesAutoMovingToStun;

        public static void Init(TechiesCrappahilationPaid main)
        {
            _main = main;
            var autoPlanting = main.MenuManager.AutoPlanting;
            var remoteMinePlantCorrector =
                autoPlanting.CreateSwitcher("Help to plant mines in one place", true);
            var enableRemoteMines =
                autoPlanting.CreateToggleKey("Auto remote mine on last cast");
            var enablePoximityMines =
                autoPlanting.CreateSwitcher("Auto proximity mines on base", false);
            MinesAutoMovingToStun = autoPlanting.CreateSwitcher("Move proximity mines on closest stun position", false);
            RangeForMinesAutoMoving = autoPlanting.CreateSlider("Range for auto moving", 1500, 700, 2000);
            var sub = UpdateManager.CreateUpdate(500, enableRemoteMines, Callback);
            var sub2 = UpdateManager.CreateUpdate( 500, enablePoximityMines, AutoProximityMinesCallBack);
            enableRemoteMines.ValueChanged += (sender, args) =>
            {
                sub.IsEnabled = enableRemoteMines;
                if (!enableRemoteMines)
                {
                    Effect?.Dispose();
                }
            };
            enablePoximityMines.ValueChanged += (sender, args) =>
            {
//                var landMinesOnBase =
//                    _main.Updater.BombManager.LandMines.Where(x => Math.Abs(x.Owner.Position.Z - 384f) < 1);
//                var sb = new StringBuilder();
//                foreach (var landMine in landMinesOnBase.Select(x => x.Owner))
//                {
//                    var pos = landMine.Position;
//                    var x = $"{pos.X}f".Replace(",", ".");
//                    var y = $"{pos.Y}f".Replace(",", ".");
//                    var z = $"{pos.Z}f".Replace(",", ".");
//                    sb.Append($"new Vector3({x},{y},{z}),");
//                }
//
//                Console.WriteLine(sb);

                sub2.IsEnabled = enablePoximityMines;
            };
            OrderManager.OrderAdding+= args =>
            {
                if (args.IsCustom)
                    return;
                if (enableRemoteMines)
                {
                    var ability = args.Order.Ability;
                    var pos = args.Order.Position;
                    if (ability == null || ability.Id != AbilityId.techies_remote_mines || pos.IsZero) return;
                    LastPosition = pos;
                    Effect?.Dispose();
                    Effect = ParticleManager.CreateParticle("materials/ensage_ui/particles/range_display_mod.vpcf", LastPosition);
                    Effect.SetControlPoint(1, new Vector3(50, 255, 0));
                    Effect.SetControlPoint(2, new Vector3(0, 255, 255));
                }

                if (remoteMinePlantCorrector)
                {
                    var ability = args.Order.Ability;
                    var pos = args.Order.Position;
                    if (ability == null ||
                        (ability.Id != AbilityId.techies_remote_mines && ability.Id != AbilityId.techies_land_mines &&
                         ability.Id != AbilityId.techies_stasis_trap) || pos.IsZero) return;
                    var closest = main.Updater.BombManager.FullBombList.Where(x => x.Owner.IsInRange(pos, 200))
                        .OrderBy(x => pos.Distance2D(x.Owner.Position)).FirstOrDefault();
                    if (closest != null)
                    {
                        args.Process = false;
                        ability.Cast(closest.Owner.Position);
                    }
                }

                // if (enableRemoteMines)
                // {
                //     var ability = args.Ability;
                //     var pos = args.TargetPosition;
                //     if (ability == null || ability.Id != AbilityId.techies_land_mines || pos.IsZero) return;
                //     LastPosition2 = pos;
                //     Effect2?.Dispose();
                //     Effect2 = new ParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf", LastPosition2);
                //     Effect2.SetControlPoint(1, new Vector3(50, 255, 0));
                //     Effect2.SetControlPoint(2, new Vector3(255, 0, 255));
                // }
            };
        }

        public static MenuSlider RangeForMinesAutoMoving { get; set; }

        public static bool MinesAutoMovingToStun { get; set; }

        private static void AutoProximityMinesCallBack()
        {
            var landMine = _main.LandMine;

            var closetPositionForPlanting = GetMinesPositions
                .Where(x => !_main.Updater.BombManager.LandMines.Any(z => z.Owner.IsInRange(x, 400)))
                .OrderBy(x => x.Distance2D(new Vector3())).FirstOrDefault();

            if (!closetPositionForPlanting.IsZero)
            {
                if (landMine.Ability.CanBeCasted())
                {
                    var arcane = _main.Me.GetItemById(AbilityId.item_arcane_boots);
                    var greaves = _main.Me.GetItemById(AbilityId.item_guardian_greaves);
                    var soulring = _main.Me.GetItemById(AbilityId.item_soul_ring);
                    var clarity = _main.Me.GetItemById(AbilityId.item_clarity);
                    if (clarity != null && _main.Me.ManaPercent() <= 0.7f &&
                        !_main.Me.HasAnyModifiers("modifier_clarity_potion"))
                    {
                        clarity.Cast(_main.Me);
                    }

                    var list = new List<Ability>()
                    {
                        arcane, greaves, soulring
                    };

                    foreach (var ability in list.Where(x => x != null && x.IsValid && x.CanBeCasted()))
                    {
                        ability.Cast();
                    }


                    landMine.Ability.Cast(closetPositionForPlanting);
                }
                else
                {
                    Me.Move(closetPositionForPlanting);
                }
            }
        }


        private static void Callback()
        {
            var remote = _main.RemoteMine;

            if (remote.Ability.CanBeCasted() && _main.Me.IsInRange(LastPosition, remote.Ability.CastRange + 200f))
            {
                var arcane = _main.Me.GetItemById(AbilityId.item_arcane_boots);
                var greaves = _main.Me.GetItemById(AbilityId.item_guardian_greaves);
                var soulring = _main.Me.GetItemById(AbilityId.item_soul_ring);
                var clarity = _main.Me.GetItemById(AbilityId.item_clarity);
                if (clarity != null && _main.Me.ManaPercent() <= 0.7f &&
                    !_main.Me.HasAnyModifiers("modifier_clarity_potion"))
                {
                    clarity.Cast(_main.Me);
                }

                var list = new List<Ability>()
                {
                    arcane, greaves, soulring
                };

                foreach (var ability in list.Where(x => x != null && x.IsValid && x.CanBeCasted()))
                {
                    ability.Cast();
                }

                remote.Ability.Cast(LastPosition);
                // _main.Me.Move(LastPosition);
            }
        }
    }
}