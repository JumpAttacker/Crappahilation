using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Divine.Entity;
using Divine.Extensions;
using Divine.Numerics;
using Divine.Update;
using Divine.Entity.Entities;
using Divine.Entity.Entities.Units;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Menu.Items;
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
            var bombName = bombBase.Owner.Name.Replace("npc_dota_", "");
            // var searchName = d.Key.Replace("_mines", "_mine");

            // foreach (var d in AbilityRangeToggle.GetValue(bombBase.))
            // {
            //     var bombName = bombBase.Owner.Name.Replace("npc_dota_", "");
            //     var searchName = d.Key.Replace("_mines", "_mine");
            //     if (bombName == searchName)
            //     {
            //         return d.Value;
            //     }
            // }

            return true;
        }

        public BombManager(Updater updater)
        {
            _updater = updater;

            FullBombList = new List<BombBase>();
            RemoteMines = new List<RemoteMine>();
            StasisTraps = new List<StasisTrap>();
            LandMines = new List<LandMine>();

            var dict = new Dictionary<AbilityId, bool>
            {
                {AbilityId.techies_remote_mines, true},
                {AbilityId.techies_stasis_trap, true},
                {AbilityId.techies_land_mines, true}
            };
            AbilityRangeToggle = updater._main.MenuManager.RangeMenu.CreateAbilityToggler("Show range", dict);
            //TODO: проверить че за хуйня
            // var lastDict = AbilityRangeToggle.Values.ToDictionary(x => x.Key, z => z.Value);
            // AbilityRangeToggle.ValueChanged += (sender) =>
            // {
            //     foreach (var d in dict)
            //     {
            //         foreach (var f in lastDict)
            //         {
            //             if (f.Key != d.Key) continue;
            //             if (f.Value == d.Value) continue;
            //             foreach (var bombBase in FullBombList)
            //             {
            //                 var bombName = bombBase.Owner.Name.Replace("npc_dota_", "");
            //                 var searchName = d.Key.Replace("_mines", "_mine");
            //                 if (bombName == searchName)
            //                 {
            //                     bombBase.ChangeDrawType(d.Value,
            //                         bombBase is RemoteMine ? Color.Red : Color.White);
            //                 }
            //             }
            //         }
            //     }
            //
            //     // lastDict = AbilityRangeToggle.Value.Dictionary.ToDictionary(x => x.Key, z => z.Value);
            // };


            foreach (var unit in EntityManager.GetEntities<Unit>())
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


            EntityManager.EntityAdded += (sender) =>
            {
                if (sender.IsCollection)
                    return;
                UpdateManager.BeginInvoke(0, () =>
                {
                    var unit = sender.Entity as Unit;
                    if (unit == null)
                        return;
                    var name = unit.Name;
                    switch (name)
                    {
                        case "npc_dota_techies_land_mine":
                            AddNewBombToSystem(new LandMine(unit).SetDamage(_updater._main.LandMine.GetDamage()),
                                BombEnums.BombTypes.LandMine);
                            break;
                        case "npc_dota_techies_stasis_trap":
                            AddNewBombToSystem(new StasisTrap(unit, new CantDetonate()),
                                BombEnums.BombTypes.StasisTrap);
                            break;
                        case "npc_dota_techies_remote_mine":
                            var bomb = new RemoteMine(unit).UpdateStacker(RemoteMines)
                                .SetDamage(_updater._main.RemoteMine.GetDamage(), true);
                            AddNewBombToSystem(bomb,
                                BombEnums.BombTypes.RemoveMine);
                            (bomb as RemoteMine)?.DrawSpawnRange();
                            break;
                    }
                });
            };

            EntityManager.EntityRemoved += (sender) =>
            {
                var unit = sender.Entity as Unit;
                if (unit == null)
                    return;
                var name = unit.Name;
                var bomb = FullBombList.Find(x => x.Owner.Handle == unit.Handle);
                if (bomb != null)
                {
                    RemoveBombFromSystem(bomb);
                }
            };

            Entity.NetworkPropertyChanged += (unit, args) =>
            {
                var propertyName = args.PropertyName;
                if (!propertyName.Equals("m_iHealth") && !propertyName.Equals("m_NetworkActivity") &&
                    !propertyName.Equals("m_iTaggedAsVisibleByTeam"))
                    return;
                UpdateManager.BeginInvoke(() =>
                {
                    var bomb = FullBombList.Find(x => x.Owner.Handle == unit.Handle);
                    if (bomb == null) return;

                    if (propertyName == "m_iHealth")
                    {
                        if (args.NewValue.GetInt32() <= 0)
                            RemoveBombFromSystem(bomb);
                        if (updater._main.MenuManager.DetonateOnLowHp && args.NewValue.GetInt32() <= 150)
                        {
                            (bomb as RemoteMine)?.Owner.Spellbook.Spell1.Cast();
                        }
                    }
                    else if (propertyName == "m_NetworkActivity")
                    {
                        // Console.WriteLine(args.NewValue.GetInt32());
                        bomb.IsActive = args.NewValue.GetInt32() == (int) BombEnums.SpawnStatus.IsActive;
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
                        if (propertyName == "m_iTaggedAsVisibleByTeam")
                        {
                            var isVisible = (args.NewValue.GetInt32() & 15) == 14;
                            bomb.ChangeDrawType(true,
                                isVisible ? Color.Red : Color.White);
                        }
                    }
                    else if (bomb is LandMine land)
                    {
                        if (propertyName == "m_iTaggedAsVisibleByTeam")
                        {
                            //UpdateManager.BeginInvoke(() =>
                            //{
                            land.BombStatus = (BombEnums.BombStatus) args.NewValue.GetInt32();
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
                });
            };


            UpdateManager.BeginInvoke(5000, async () =>
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
            });
        }

        public MenuAbilityToggler AbilityRangeToggle { get; set; }


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
                                    x.Owner.IsInRange(bomb.Owner, AutoPlanter.RangeForMinesAutoMoving.Value))
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