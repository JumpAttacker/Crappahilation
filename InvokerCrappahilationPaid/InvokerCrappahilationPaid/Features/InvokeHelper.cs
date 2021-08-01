using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using Divine.Entity;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Extensions;
using Divine.Game;
using Divine.Menu.Items;
using Divine.Update;
using InvokerCrappahilationPaid.Extensions;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Helpers;

namespace InvokerCrappahilationPaid.Features
{
    public class InvokeHelper
    {
        private readonly Config _config;


//        private MenuItem<bool> useCataclysm;
        private bool isFirstClick;
        private readonly Menu _main;
        private MenuSlider delayForCataclysm;


        public InvokeHelper(Config config)
        {
            _config = config;
            _main = _config.Factory.CreateMenu("Invoke helper");

            UpdateManager.BeginInvoke(500, () =>
            {
                foreach (var activeAbility in config.Main.AbilitiesInCombo.AllAbilities) MakeMenu(activeAbility);

                MakeMenu(config.Main.AbilitiesInCombo.GhostWalk);
            });

            Sleeper = new Sleeper();
        }

        public Sleeper Sleeper { get; }

        private void MakeMenu(InvokerBaseAbility activeAbility)
        {
            var main = _main.CreateMenu(activeAbility.Id.ToString());
            var enable = main.CreateSwitcher("Enable", true);
            var key = main.CreateHoldKey("Invoke Key");
            var ignore = main.CreateSwitcher("Ignore invisibility", false);
            MenuSwitcher useOnMainHeroAfterInvoke = null;
            MenuSwitcher use = null;
            if (activeAbility is InvokerAlacrity or InvokerForgeSpirit or InvokerGhostWalk or InvokerIceWall)
            {
                useOnMainHeroAfterInvoke = main.CreateSwitcher("Use on main hero after Invoke", false);
                use = main.CreateSwitcher("Use if already invoked", false);
            }
            else if (activeAbility is InvokerTornado or InvokerChaosMeteor or InvokerDeafeningBlast or InvokerEmp or InvokerSunStrike or InvokerColdSnap)
            {
                useOnMainHeroAfterInvoke = main.CreateSwitcher("Use after Invoke", false);
                use = main.CreateSwitcher("Use if already invoked", false);
                if (activeAbility is InvokerSunStrike)
                {
                    delayForCataclysm = main.CreateSlider("time for double click for cataclysm", 0, 0, 100);
                }

//                if (activeAbility is InvokerSunStrike)
//                {
//                    useCataclysm = main.Item("Use cactaclysm", false);
//                }
            }

            var reInvoke = main.CreateSwitcher("Use invoke if skill in slot #5", false);
            ((IHaveFastInvokeKey) activeAbility).Key =
                key.Key is Key.None ? Key.None : KeyInterop.KeyFromVirtualKey((int) key.Key);

            isFirstClick = true;
            var oldKey = Key.None;
            UpdateManager.CreateIngameUpdate(5000, () =>
            {
                if (oldKey != key.Key)
                {
                    oldKey = key.Key;
                    ((IHaveFastInvokeKey) activeAbility).Key = key.Key;
                    // Console.WriteLine(
                        // $"({activeAbility}) Changed: from {((IHaveFastInvokeKey) activeAbility).Key} key ({key.Key})");
                }
            });
            key.ValueChanged += async (sender, args) =>
            {
                if (!enable) return;
                if (args.Value)
                {
                    if (_config.SmartSphere.InChanging.IsSleeping)
                        while (_config.SmartSphere.InChanging.IsSleeping)
                            await Task.Delay(5);
                    if (!_config.Main.Me.IsAlive || !_config.Main.Me9.CanUseAbilities)
                        return;
                    if ( /*|| !activeAbility.CanBeCasted ||*/
                        _config.Main.Me.HasAnyModifiers(_config.Main.AbilitiesInCombo.GhostWalk.ModifierName,
                            "item_glimmer_cape") && !ignore)
                        return;
                    if (EntityManager.LocalPlayer is not null && !EntityManager.LocalPlayer.SelectedUnits.Any(x => x.Equals(_config.Main.Me)))
                        return;
                    var slot = activeAbility.AbilitySlot;
                    if (reInvoke && slot == AbilitySlot.Slot5)
                    {
                        _config.Main.Combo.InvokeThisShit(activeAbility);
                        return;
                    }
                    if (slot is AbilitySlot.Slot4 or AbilitySlot.Slot5)
                    {
                        if (use is {Value: true}) JustUse(activeAbility);

                        return;
                    }
                    if (!_config.Main.AbilitiesInCombo.Invoke.BaseAbility.CanBeCasted())
                        return;
                    if (useOnMainHeroAfterInvoke == null)
                        InvokeThenCast(activeAbility);
                    else
                        InvokeThenCast(activeAbility, useOnMainHeroAfterInvoke);
                }
            };
        }

        private Unit GetActualTarget()
        {
            var target = _config.Main.Combo.Target ??
                         EntityManager.GetEntities<Hero>().Where(x => x.IsValid && x.IsEnemy(_config.Main.Me) && x.IsAlive && x.IsVisibleToEnemies).OrderBy(z => z.Distance2D(_config.Main.Me)).FirstOrDefault();
            return target;
        }

        private void InvokeThenCast(InvokerBaseAbility activeAbility, bool thenCast = false)
        {
            if (Sleeper.IsSleeping)
                return;
            var invoked = false;
            switch (activeAbility)
            {
                case InvokerAlacrity ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility(activeAbility.Owner);

                    break;
                case InvokerChaosMeteor ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility(GameManager.MousePosition);

                    break;
                case InvokerSunStrike ability:
                    invoked = ability.Invoke();
                    // Console.WriteLine($"Invoke {invoked}");
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility(GameManager.MousePosition);

                    break;
                case InvokerEmp ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility(GameManager.MousePosition);

                    break;
                case InvokerColdSnap ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                        {
                            var target = _config.Main.Combo.Target ??
                                         EntityManager.GetEntities<Hero>().Where(x => x.IsValid &&  x.IsEnemy(_config.Main.Me) && x.IsAlive && x.IsVisibleToEnemies).OrderBy(z => z.Distance2D(_config.Main.Me)).FirstOrDefault();
                            if (target == null)
                                break;
                            ability.BaseAbility.BaseAbility.Cast(target);
                        }

                    break;
                case InvokerDeafeningBlast ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility(GameManager.MousePosition);

                    break;
                case InvokerForgeSpirit ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility();

                    break;
                case InvokerGhostWalk ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                        {
                            Sleeper.Sleep(1f);
                            _config.SmartSphere.Sleeper.Sleep(2.500f);
                            UpdateManager.BeginInvoke(250, () =>
                            {
                                if (activeAbility.CanBeCasted())
                                {
                                    if (!_config.Main.Me.HasAnyModifiers(_config.Main.AbilitiesInCombo.GhostWalk
                                        .ModifierName))
                                    {
                                        _config.Main.AbilitiesInCombo.Wex.UseAbility();
                                        _config.Main.AbilitiesInCombo.Wex.UseAbility();
                                        _config.Main.AbilitiesInCombo.Wex.UseAbility();
                                    }

                                    activeAbility.UseAbility();
                                }
                            });
                        }

                    break;
                case InvokerIceWall ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility();

                    break;
                case InvokerTornado ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility(GameManager.MousePosition);

                    break;
            }

            if (invoked)
                Sleeper.Sleep(.500f);
        }

        private void JustUse(InvokerBaseAbility activeAbility)
        {
            switch (activeAbility)
            {
                case InvokerAlacrity ability:
                    ability.UseAbility(activeAbility.Owner, false, false);
                    break;
                case InvokerForgeSpirit ability:
                    ability.UseAbility();
                    break;
                case InvokerGhostWalk ability:
                    /*_config.Main.AbilitiesInCombo.Wex.UseAbility();
                    _config.Main.AbilitiesInCombo.Wex.UseAbility();
                    _config.Main.AbilitiesInCombo.Wex.UseAbility();*/
                    ability.UseAbility();
                    break;
                case InvokerIceWall ability:
                    ability.UseAbility();
                    break;
                case InvokerColdSnap ability:
                    var target = GetActualTarget();
                    if (target == null)
                        break;
                    ability.BaseAbility.BaseAbility.Cast(target);
                    break;
                case InvokerSunStrike ability:
                    if (isFirstClick)
                    {
                        isFirstClick = false;
                        if (delayForCataclysm == 0)
                        {
                            ability.BaseAbility.BaseAbility.Cast(GameManager.MousePosition);
                            isFirstClick = true;
                        }
                        else
                            UpdateManager.BeginInvoke(delayForCataclysm, () =>
                            {
                                if (isFirstClick) return;
                                ability.UseAbility(GameManager.MousePosition);
                                isFirstClick = true;
                            });
                    }
                    else
                    {
                        ability.BaseAbility.BaseAbility.Cast(ability.Owner);
                        isFirstClick = true;
                    }

                    break;
                default:
                    activeAbility.UseAbility(GameManager.MousePosition);
                    break;
            }
        }
    }
}