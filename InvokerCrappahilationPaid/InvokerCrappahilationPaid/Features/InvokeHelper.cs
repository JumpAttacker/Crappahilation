using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Abilities;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;

namespace InvokerCrappahilationPaid.Features
{
    public class InvokeHelper
    {
        private readonly Config _config;

        private readonly MenuFactory _main;

//        private MenuItem<bool> useCataclysm;
        private bool isFirstClick;
        private MenuItem<Slider> delayForCataclysm;

        public InvokeHelper(Config config)
        {
            _config = config;
            _main = _config.Factory.Menu("Invoke helper");

            UpdateManager.BeginInvoke(() =>
            {
                foreach (var activeAbility in config.Main.AbilitiesInCombo.AllAbilities) MakeMenu(activeAbility);

                MakeMenu(config.Main.AbilitiesInCombo.GhostWalk);
            }, 500);

            Sleeper = new Sleeper();
        }

        public Sleeper Sleeper { get; }

        private void MakeMenu(ActiveAbility activeAbility)
        {
            var main = _main.MenuWithTexture("", $"{activeAbility.Ability.Id}", $"{activeAbility.Ability.Id}");
            var enable = main.Item("Enable", true);
            var key = main.Item("Invoke Key", new KeyBind('0'));
            var ignore = main.Item("Ignore invisibility", false);
            MenuItem<bool> useOnMainHeroAfterInvoke = null;
            MenuItem<bool> use = null;
            if (activeAbility is InvokerAlacrity || activeAbility is InvokerForgeSpirit ||
                activeAbility is InvokerGhostWalk || activeAbility is InvokerIceWall)
            {
                useOnMainHeroAfterInvoke = main.Item("Use on main hero after Invoke", false);
                use = main.Item("Use if already invoked", false);
            }
            else if (activeAbility is InvokerTornado || activeAbility is InvokerChaosMeteor ||
                     activeAbility is InvokerDeafeningBlast || activeAbility is InvokerEmp ||
                     activeAbility is InvokerSunStrike || activeAbility is InvokerColdSnap)
            {
                useOnMainHeroAfterInvoke = main.Item("Use after Invoke", false);
                use = main.Item("Use if already invoked", false);
                if (activeAbility is InvokerSunStrike)
                {
                    delayForCataclysm = main.Item("time for double click for cataclysm", new Slider(0, 0, 100));
                }

//                if (activeAbility is InvokerSunStrike)
//                {
//                    useCataclysm = main.Item("Use cactaclysm", false);
//                }
            }

            var reInvoke = main.Item("Use invoke if skill in slot #5", false);
            ((IHaveFastInvokeKey) activeAbility).Key =
                key.Value.Key == '0' ? Key.None : KeyInterop.KeyFromVirtualKey((int) key.Value.Key);

            var value = key.Value.Active;
            isFirstClick = true;
            key.PropertyChanged += async (sender, args) =>
            {
                if (!enable) return;
                if (key)
                {
                    if (value)
                        return;
                    value = true;
                    if (_config.SmartSphere.InChanging.Sleeping)
                        while (_config.SmartSphere.InChanging.Sleeping)
                            await Task.Delay(5);

                    if (!_config.Main.Me.IsAlive || !_config.Main.Me.CanCastAbilities())
                        return;
                    if ( /*|| !activeAbility.CanBeCasted ||*/
                        _config.Main.Me.HasAnyModifiers(_config.Main.AbilitiesInCombo.GhostWalk.ModifierName,
                            "item_glimmer_cape") && !ignore)
                        return;
                    if (!ObjectManager.LocalPlayer.Selection.Any(x => x.Equals(_config.Main.Me)))
                        return;
                    var slot = activeAbility.Ability.AbilitySlot;
                    if (reInvoke && slot == AbilitySlot.Slot_5)
                    {
                        _config.Main.Combo.InvokeThisShit(activeAbility);
                        return;
                    }

                    if (slot == AbilitySlot.Slot_4 ||
                        slot == AbilitySlot.Slot_5)
                    {
                        if (use != null && use) JustUse(activeAbility);

                        return;
                    }

                    if (!_config.Main.AbilitiesInCombo.Invoke.CanBeCasted)
                        return;
                    if (useOnMainHeroAfterInvoke == null)
                        InvokeThenCast(activeAbility);
                    else
                        InvokeThenCast(activeAbility, useOnMainHeroAfterInvoke);
                }
                else
                {
                    if (value)
                    {
                        value = false;
                    }
                    else
                    {
                        ((IHaveFastInvokeKey) activeAbility).Key = KeyInterop.KeyFromVirtualKey((int) key.Value.Key);
                        Console.WriteLine(
                            $"({activeAbility}) Changed: to {((IHaveFastInvokeKey) activeAbility).Key} ({key.Value.Key})");
                    }

                    //Console.WriteLine($"({activeAbility}) Changed: to {((IHaveFastInvokeKey)activeAbility).Key} ({key.Value.Key})");
                }
            };
        }

        private void InvokeThenCast(ActiveAbility activeAbility, bool thenCast = false)
        {
            if (Sleeper.Sleeping)
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
                            activeAbility.UseAbility(Game.MousePosition);

                    break;
                case InvokerSunStrike ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility(Game.MousePosition);

                    break;
                case InvokerEmp ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility(Game.MousePosition);

                    break;
                case InvokerColdSnap ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                        {
                            var target = _config.Main.Combo.Target ??
                                         _config.Main.Context.TargetSelector?.Active.GetTargets().FirstOrDefault();
                            if (target == null)
                                break;
                            ability.UseAbility(target);
                        }

                    break;
                case InvokerDeafeningBlast ability:
                    invoked = ability.Invoke();
                    if (invoked)
                        if (thenCast)
                            activeAbility.UseAbility(Game.MousePosition);

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
                            Sleeper.Sleep(1000);
                            _config.SmartSphere.Sleeper.Sleep(2500);
                            UpdateManager.BeginInvoke(() =>
                            {
                                if (activeAbility.CanBeCasted)
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
                            }, 250);
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
                            activeAbility.UseAbility(Game.MousePosition);

                    break;
            }

            if (invoked)
                Sleeper.Sleep(500);
        }

        private void JustUse(ActiveAbility activeAbility)
        {
            switch (activeAbility)
            {
                case InvokerAlacrity ability:
                    ability.UseAbility(activeAbility.Owner);
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
                    var target = _config.Main.Combo.Target ??
                                 _config.Main.Context.TargetSelector?.Active.GetTargets().FirstOrDefault();
                    if (target == null)
                        break;
                    ability.UseAbility(target);
                    break;
                case InvokerSunStrike ability:
                    if (isFirstClick)
                    {
                        isFirstClick = false;
                        if (delayForCataclysm == 0)
                        {
                            ability.Ability.UseAbility(Game.MousePosition);
                            isFirstClick = true;
                        }
                        else
                            UpdateManager.BeginInvoke(() =>
                            {
                                if (isFirstClick) return;
                                ability.UseAbility(Game.MousePosition);
                                isFirstClick = true;
                            }, delayForCataclysm);
                    }
                    else
                    {
                        ability.Ability.UseAbility(ability.Owner);
                        isFirstClick = true;
                    }

                    break;
                default:
                    activeAbility.UseAbility(Game.MousePosition);
                    break;
            }
        }
    }
}