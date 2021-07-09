using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Divine;
using Divine.Menu.Items;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Helpers;

namespace InvokerCrappahilationPaid.Features
{
    public class InvokeHelper
    {
        private readonly Config _config;

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

        private void MakeMenu(ActiveAbility activeAbility)
        {
            var main = _main.CreateMenu("");
            main.SetAbilityTexture(activeAbility.Id);
            var enable = main.CreateSwitcher("Enable", true);
            var key = main.CreateHoldKey("Invoke Key");
            var ignore = main.CreateSwitcher("Ignore invisibility", false);
            MenuSwitcher useOnMainHeroAfterInvoke = null;
            MenuSwitcher use = null;
            if (activeAbility is InvokerAlacrity || activeAbility is InvokerForgeSpirit ||
                activeAbility is InvokerGhostWalk || activeAbility is InvokerIceWall)
            {
                useOnMainHeroAfterInvoke = main.CreateSwitcher("Use on main hero after Invoke", false);
                use = main.CreateSwitcher("Use if already invoked", false);
            }
            else if (activeAbility is InvokerTornado || activeAbility is InvokerChaosMeteor ||
                     activeAbility is InvokerDeafeningBlast || activeAbility is InvokerEmp ||
                     activeAbility is InvokerSunStrike || activeAbility is InvokerColdSnap)
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
                key.Key == Key.None ? Key.None : KeyInterop.KeyFromVirtualKey((int) key.Key);

            var value = key.Value;
            isFirstClick = true;
            key.ValueChanged += async (sender, args) =>
            {
                if (!enable) return;
                if (args.Value)
                {
                    if (value)
                        return;
                    value = true;
                    if (_config.SmartSphere.InChanging.Sleeping)
                        while (_config.SmartSphere.InChanging.Sleeping)
                            await Task.Delay(5);

                    if (!_config.Main.Me.IsAlive || !_config.Main.Me.CanUseAbilities)
                        return;
                    if ( /*|| !activeAbility.CanBeCasted ||*/
                        _config.Main.Me.HasModifier(_config.Main.AbilitiesInCombo.GhostWalk.ModifierName,
                            "item_glimmer_cape") && !ignore)
                        return;
                    if (!EntityManager.LocalPlayer.SelectedUnits.Any(x => x.Equals(_config.Main.Me)))
                        return;
                    var slot = activeAbility.BaseAbility.AbilitySlot;
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

                    if (!_config.Main.AbilitiesInCombo.Invoke.CanBeCasted())
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
                        ((IHaveFastInvokeKey) activeAbility).Key = KeyInterop.KeyFromVirtualKey((int) key.Key);
                    }
                }
            };
        }

        private void InvokeThenCast(ActiveAbility activeAbility, bool thenCast = false)
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
                            Sleeper.Sleep(1000);
                            _config.SmartSphere.Sleeper.Sleep(2500);
                            UpdateManager.BeginInvoke(250, () =>
                            {
                                if (activeAbility.CanBeCasted())
                                {
                                    if (!_config.Main.Me.HasModifier(_config.Main.AbilitiesInCombo.GhostWalk
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
                                 _config.Main.TargetSelector?.Active.GetTargets().FirstOrDefault();
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
                            ability.UseAbility(GameManager.MousePosition);
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
                        ability.UseAbility(ability.Owner);
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