using System;
using System.Collections.Generic;
using System.Linq;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Update;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Heroes;
using O9K.Core.Helpers;

namespace InvokerCrappahilationPaid.Features
{
    public class Prepare
    {
        private readonly InvokerCrappahilationPaid _main;
        private readonly MultiSleeper<string> _sleeper;

        public Prepare(InvokerCrappahilationPaid main)
        {
            _sleeper = new MultiSleeper<string>();
            _main = main;
            UpdateManager.BeginInvoke(100, () =>
            {
                var comboUpdateHandler = UpdateManager.CreateIngameUpdate(100, false, PrepareInAction);
                Config.PrepareKey.ValueChanged += (sender, args) =>
                {
                    if (args.Value)
                        comboUpdateHandler.IsEnabled = true;
                    else
                        comboUpdateHandler.IsEnabled = false;
                };
            });
        }

        private Combo.ComboTypeEnum GameplayType =>
            _main.Config.ComboPanel.IsAutoComboSelected ? Combo.ComboTypeEnum.Auto : Combo.ComboTypeEnum.CustomCombo;

        private Config Config => _main.Config;
        private AbilitiesInCombo Abilities => _main.AbilitiesInCombo;
        private uint ExortLevel => Abilities.Exort.Level;
        private uint WexLevel => Abilities.Wex.Level;
        private uint QuasLevel => Abilities.Quas.Level;
        private Hero Me => _main.Me;
        private Hero9 Me9 => _main.Me9;

        private void PrepareInAction()
        {
            // Console.WriteLine("PrepareInAction. 1");
            if (GameplayType == Combo.ComboTypeEnum.Auto)
                return;
            // Console.WriteLine("PrepareInAction. 2");
            var combo = _main.Config.ComboPanel.SelectedCombo;
            var allAbilities = combo.Items.ToArray();
            var abilities = new List<InvokerBaseAbility?>();
            var one = GetAbility(allAbilities, ref abilities);
            var two = GetAbility(allAbilities, ref abilities);
            var three = GetAbility(allAbilities, ref abilities);

            // Console.WriteLine($"one: {one}");
            // Console.WriteLine($"two: {two}");
            // Console.WriteLine($"three: {three}");
            
            if (three != null) two = three;
            if (one == null)
            {
                // Console.WriteLine("Cant Find Ability for prepare");
                return;
            }

            if (two == null)
            {
                if (one is IInvokableAbility {IsInvoked: false, CanBeInvoked: true} invokable)
                    invokable.Invoke();
                // Console.WriteLine("Will invoke only first ability");
                return;
            }

            var empty1 = Me.Spellbook.Spell4;
            var empty2 = Me.Spellbook.Spell5;

            var ability1Invoked = one.BaseAbility.BaseAbility.Equals(empty1) || one.BaseAbility.BaseAbility.Equals(empty2);
            var ability2Invoked = two.BaseAbility.BaseAbility.Equals(empty1) || two.BaseAbility.BaseAbility.Equals(empty2);
            if (ability1Invoked && ability2Invoked)
            {
                if (one.BaseAbility.BaseAbility.Equals(empty1))
                    InvokeThisShit(two);
                else if (two.BaseAbility.BaseAbility.Equals(empty2)) InvokeThisShit(one);
                return;
            }

            if (ability1Invoked)
            {
                if (one.BaseAbility.BaseAbility.Equals(empty2))
                    InvokeThisShit(one);
                else
                    InvokeThisShit(two);
            }
            else if (ability2Invoked)
            {
                if (two.BaseAbility.BaseAbility.Equals(empty2))
                    InvokeThisShit(two);
                else
                    InvokeThisShit(one);
            }
            else
            {
                InvokeThisShit(one);
                //(one as IInvokableAbility)?.Invoke();
            }
        }

        public InvokerBaseAbility? GetAbility(AbilityId[] allAbilities, ref List<InvokerBaseAbility?> abilities)
        {
            var list = abilities;
            AbilityId firstAbility = allAbilities.First(x => !x.ToString().StartsWith("item_") && list.All(z => z.Id != x));

            var found = _main.AbilitiesInCombo.AllAbilities.Find(z =>
                !list.Contains(z) && z.Id == firstAbility);
            if (found != null) abilities.Add(found);
            return found;
        }

        private bool InvokeThisShit(InvokerBaseAbility? ability)
        {
            // Console.WriteLine($"Trying to invoke -> {ability.Id}");
            if (_sleeper.IsSleeping($"{ability} shit"))
            {
                // Console.WriteLine($"Invoke [blocked] ({ability})");
                return false;
            }

            if (Abilities.Invoke.IsReady)
            {
                var requiredOrbs = (ability as IInvokableAbility)?.RequiredOrbs;
                // Console.WriteLine($"Сферы для инвока: [{requiredOrbs}] {ability.req}");
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        var sphere = _main.AbilitiesInCombo.Spheres.FirstOrDefault(z=>z.Id==abilityId);
                        if (sphere == null)
                        {
                            Console.WriteLine($"Не могу найти сферу для инвока: [{abilityId}]");
                            return false;
                        }
                        if (!sphere.UseAbility()) return false;
                        // Console.WriteLine($"Invoke [Sphere: {abilityId}] ({ability})");
                    }

                    var invoked = Abilities.Invoke.UseAbility();
                    if (invoked)
                    {
                        _sleeper.Sleep($"{ability} shit", .200f);
                        // Console.WriteLine($"Invoke [{ability}]");
                    }

                    return invoked;
                }

                Console.WriteLine($"Error in Invoke function: {ability.Id}");
                return false;
            }

            // Console.WriteLine($"Invoke [on cd] ({ability})");
            return false;
        }
    }
}