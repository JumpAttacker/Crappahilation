using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using SharpDX;
using TechiesCrappahilationPaid.BombsType;
using TechiesCrappahilationPaid.BombsType.BombBehaviour;
using TechiesCrappahilationPaid.BombsType.Enums;
using TechiesCrappahilationPaid.Features;

namespace TechiesCrappahilationPaid.Managers
{
    public class BombManager
    {
        private readonly Updater _updater;
        public List<BombBase> FullBombList;
        public List<LandMine> LandMines;
        public List<RemoteMine> RemoteMines;
        public List<StasisTrap> StasisTraps;

        public bool IsDrawEnabledForBombType(BombBase bombBase)
        {
            foreach (var d in AbilityRangeToggle.Value.Dictionary)
            {
                var bombName = bombBase.Owner.Name.Replace("npc_dota_", "");
                var searchName = d.Key.Replace("_mines", "_mine");
                if (bombName == searchName)
                {
                    return d.Value;
                }
            }

            return false;
        }

        public BombManager(Updater updater)
        {
            _updater = updater;

            FullBombList = new List<BombBase>();
            RemoteMines = new List<RemoteMine>();
            StasisTraps = new List<StasisTrap>();
            LandMines = new List<LandMine>();

            var dict = new Dictionary<string, bool>
            {
                {AbilityId.techies_remote_mines.ToString(), true},
                {AbilityId.techies_stasis_trap.ToString(), true},
                {AbilityId.techies_land_mines.ToString(), true}
            };
            AbilityRangeToggle = updater._main.MenuManager.RangeMenu.Item("Show range", new AbilityToggler(dict));
            var lastDict = AbilityRangeToggle.Value.Dictionary.ToDictionary(x => x.Key, z => z.Value);
            AbilityRangeToggle.PropertyChanged += (sender, args) =>
            {
                foreach (var d in AbilityRangeToggle.Value.Dictionary)
                {
                    foreach (var f in lastDict)
                    {
                        if (f.Key != d.Key) continue;
                        if (f.Value == d.Value) continue;
                        foreach (var bombBase in FullBombList)
                        {
                            var bombName = bombBase.Owner.Name.Replace("npc_dota_", "");
                            var searchName = d.Key.Replace("_mines", "_mine");
                            if (bombName == searchName)
                            {
                                bombBase.ChangeDrawType(d.Value,
                                    bombBase is RemoteMine ? Color.Red : Color.White);
                            }
                        }
                    }
                }

                lastDict = AbilityRangeToggle.Value.Dictionary.ToDictionary(x => x.Key, z => z.Value);
            };


            foreach (var unit in EntityManager<Unit>.Entities)
            {
                var bomb = FullBombList.Find(x => x.Owner.Handle == unit.Handle);
                if (bomb == null)
                {
                    var name = unit.Name;
                    BombBase bombBase = null;
                    switch (name)
                    {
                        case "npc_dota_techies_land_mine":
                            bombBase = new LandMine(unit).SetDamage(_updater._main.LandMine.GetDamage());
                            AddNewBombToSystem(bombBase, BombEnums.BombTypes.LandMine);
                            break;
                        case "npc_dota_techies_stasis_trap":
                            bombBase = new StasisTrap(unit, new CantDetonate());
                            AddNewBombToSystem(bombBase, BombEnums.BombTypes.StasisTrap);
                            break;
                        case "npc_dota_techies_remote_mine":
                            bombBase = new RemoteMine(unit).UpdateStacker(RemoteMines)
                                .SetDamage(_updater._main.RemoteMine.GetDamage(), true);
                            AddNewBombToSystem(bombBase, BombEnums.BombTypes.RemoveMine);
                            break;
                    }

                    if (bombBase != null)
                    {
                        bombBase.IsActive = true;
                        bombBase.ChangeDrawType(true && IsDrawEnabledForBombType(bombBase),
                            bombBase is RemoteMine ? Color.Red : Color.White);
                    }
                }
            }

            EntityManager<Unit>.EntityAdded += (sender, unit) =>
            {
                var name = unit.Name;
                switch (name)
                {
                    case "npc_dota_techies_land_mine":
                        AddNewBombToSystem(new LandMine(unit).SetDamage(_updater._main.LandMine.GetDamage()),
                            BombEnums.BombTypes.LandMine);
                        break;
                    case "npc_dota_techies_stasis_trap":
                        AddNewBombToSystem(new StasisTrap(unit, new CantDetonate()), BombEnums.BombTypes.StasisTrap);
                        break;
                    case "npc_dota_techies_remote_mine":
                        var bomb = new RemoteMine(unit).UpdateStacker(RemoteMines)
                            .SetDamage(_updater._main.RemoteMine.GetDamage(), true);
                        AddNewBombToSystem(bomb,
                            BombEnums.BombTypes.RemoveMine);
                        (bomb as RemoteMine)?.DrawSpawnRange();
                        break;
                }
            };

            EntityManager<Unit>.EntityRemoved += (sender, unit) =>
            {
                var bomb = FullBombList.Find(x => x.Owner.Handle == unit.Handle);
                if (bomb != null)
                {
                    RemoveBombFromSystem(bomb);
                }
            };

            Entity.OnInt32PropertyChange += (unit, args) =>
            {
                var bomb = FullBombList.Find(x => x.Owner.Handle == unit.Handle);
                if (bomb == null) return;
                var propertyName = args.PropertyName;
                if (propertyName == "m_iHealth")
                {
                    if (args.NewValue <= 0)
                        RemoveBombFromSystem(bomb);
                }
                else if (propertyName == "m_NetworkActivity")
                {
                    bomb.IsActive = args.NewValue == (int) BombEnums.SpawnStatus.IsActive;
                    if (bomb.IsActive)
                    {
                        (bomb as RemoteMine)?.DisposeSpawnRange();
                        var isVisible = bomb.Owner.IsVisibleToEnemies;
                        bomb.ChangeDrawType(IsDrawEnabledForBombType(bomb),
                            isVisible ? Color.Red : Color.White);
                        if (bomb is LandMine mine)
                            if (isVisible)
                                mine.StartTimer();
                    }
                    else
                    {
                        bomb.ChangeDrawType(true && IsDrawEnabledForBombType(bomb), Color.Gray);
                    }
                }

                if (bomb is RemoteMine)
                {
                }
                else if (bomb is LandMine land)
                {
                    if (propertyName == "m_iTaggedAsVisibleByTeam")
                    {
                        //UpdateManager.BeginInvoke(() =>
                        //{
                        TechiesCrappahilationPaid.Log.Warn($"{bomb} is visible {args.NewValue}");
                        land.BombStatus = (BombEnums.BombStatus) args.NewValue;
                        var willDetonate = land.BombStatus == BombEnums.BombStatus.WillDetonate;
                        bomb.ChangeDrawType(true && IsDrawEnabledForBombType(bomb),
                            willDetonate
                                ? Color.Red
                                : Color.White);

                        if (willDetonate)
                            land.StartTimer();
                        else
                            land.StopTimer();
                        //}, 25);
                    }
                }
            };


            UpdateManager.BeginInvoke(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (Me.GetAbilityById(AbilityId.special_bonus_unique_techies_4).Level > 0)
                        {
                            var list = RemoteMines.ToList();
                            var tempList = new List<RemoteMine>();
                            foreach (var remoteMine in list)
                            {
                                remoteMine.Stacker.Counter = 0;
                            }

                            foreach (var remoteMine in list)
                            {
                                remoteMine.UpdateStacker(tempList);
                                tempList.Add(remoteMine);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                    await Task.Delay(1000);
                }
            }, 5000);
        }

        public MenuItem<AbilityToggler> AbilityRangeToggle { get; set; }

        private Hero Me => _updater._main.Me;

        private async void AddNewBombToSystem(BombBase bomb, BombEnums.BombTypes type)
        {
            switch (type)
            {
                case BombEnums.BombTypes.RemoveMine:
                    RemoteMines.Add((RemoteMine) bomb);
                    if (Me.GetAbilityById(AbilityId.special_bonus_unique_techies_4).Level > 0)
                    {
                        bomb.StartUpdatingPosition();
                    }

                    break;
                case BombEnums.BombTypes.StasisTrap:
                    StasisTraps.Add((StasisTrap) bomb);
                    break;
                case BombEnums.BombTypes.LandMine:
                    LandMines.Add((LandMine) bomb);
                    await Task.Delay(150);
                    try
                    {
                        if (AutoPlanter.IsAutoMovingToStaticTraps)
                        {
                            var closetStaticMine = StasisTraps.Where(x =>
                                    x.Owner.IsInRange(bomb.Owner, AutoPlanter.RangeForMinesAutoMoving.Value.Value))
                                .OrderBy(z => z.Owner.Distance2D(bomb.Owner)).FirstOrDefault();
                            if (closetStaticMine != null)
                            {
                                bomb.Owner.Move(closetStaticMine.Owner.Position, false, true);
//                                Me.Move(closetStaticMine.Owner.Position, false, true);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }


                    break;
            }

            FullBombList.Add(bomb);
        }

        private void RemoveBombFromSystem(BombBase bomb)
        {
            switch (bomb)
            {
                case RemoteMine mine:
                    RemoteMines.Remove(mine);
                    mine.DisposeSpawnRange();
                    mine.UnStacker(RemoteMines);
                    break;
                case StasisTrap trap:
                    StasisTraps.Remove(trap);
                    break;
                case LandMine mine:
                    LandMines.Remove(mine);
                    break;
            }

            FullBombList.Remove(bomb);
        }
    }
}