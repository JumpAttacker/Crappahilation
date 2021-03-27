using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Abilities;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;

namespace InvokerCrappahilationPaid.Features
{
    public class Prepare
    {
        private readonly InvokerCrappahilationPaid _main;
        private readonly MultiSleeper _sleeper;

        public Prepare(InvokerCrappahilationPaid main)
        {
            _sleeper = new MultiSleeper();
            _main = main;
            UpdateManager.BeginInvoke(() =>
            {
                var comboUpdateHandler = UpdateManager.Subscribe(PrepareInAction, 100, false);
                Config.PrepareKey.PropertyChanged += (sender, args) =>
                {
                    if (Config.PrepareKey)
                        comboUpdateHandler.IsEnabled = true;
                    else
                        comboUpdateHandler.IsEnabled = false;
                };
            }, 100);
        }

        private Combo.ComboTypeEnum GameplayType =>
            _main.Config.ComboPanel.IsAutoComboSelected ? Combo.ComboTypeEnum.Auto : Combo.ComboTypeEnum.CustomCombo;

        private IParticleManager ParticleManager => _main.Context.Particle;
        private Config Config => _main.Config;
        private AbilitiesInCombo Abilities => _main.AbilitiesInCombo;
        private uint ExortLevel => Abilities.Exort.Level;
        private uint WexLevel => Abilities.Wex.Level;
        private uint QuasLevel => Abilities.Quas.Level;
        private Hero Me => (Hero) _main.Context.Owner;

        private void PrepareInAction()
        {
            if (GameplayType == Combo.ComboTypeEnum.Auto)
                return;
            var combo = _main.Config.ComboPanel.SelectedCombo;
            var allAbilities = combo.Items.ToArray();
            var abilities = new List<ActiveAbility>();
            var one = GetAbility(allAbilities, ref abilities);
            var two = GetAbility(allAbilities, ref abilities);
            var three = GetAbility(allAbilities, ref abilities);
            if (three != null) two = three;
            if (one == null)
            {
                InvokerCrappahilationPaid.Log.Warn("Cant Find Ability for prepare");
                return;
            }

            if (two == null)
            {
                if (one is IInvokableAbility invokable && !invokable.IsInvoked && invokable.CanBeInvoked)
                    invokable.Invoke();
                InvokerCrappahilationPaid.Log.Warn("Will invoke only first ability");
                return;
            }

            var empty1 = Me.Spellbook.Spell4;
            var empty2 = Me.Spellbook.Spell5;

            var ability1Invoked = one.Ability.Equals(empty1) || one.Ability.Equals(empty2);
            var ability2Invoked = two.Ability.Equals(empty1) || two.Ability.Equals(empty2);
            if (ability1Invoked && ability2Invoked)
            {
                if (one.Ability.Equals(empty1))
                    InvokeThisShit(two);
                else if (two.Ability.Equals(empty2)) InvokeThisShit(one);
                return;
            }

            if (ability1Invoked)
            {
                if (one.Ability.Equals(empty2))
                    InvokeThisShit(one);
                else
                    InvokeThisShit(two);
            }
            else if (ability2Invoked)
            {
                if (two.Ability.Equals(empty2))
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

        public ActiveAbility GetAbility(string[] allAbilities, ref List<ActiveAbility> abilities)
        {
            var list = abilities;
            var firstAbility = allAbilities.First(x => !x.StartsWith("item_") && list.All(z => z.Ability.Name != x));

            var found = _main.AbilitiesInCombo.AllAbilities.Find(z =>
                !list.Contains(z) && z.Ability.Name == firstAbility);
            if (found != null) abilities.Add(found);
            return found;
        }

        private bool InvokeThisShit(ActiveAbility ability)
        {
            InvokerCrappahilationPaid.Log.Debug($"Trying to invoke -> {ability.Ability.Id}");
            if (_sleeper.Sleeping($"{ability} shit"))
            {
                InvokerCrappahilationPaid.Log.Debug($"Invoke [blocked] ({ability})");
                return false;
            }

            if (Abilities.Invoke.IsReady)
            {
                var requiredOrbs = (ability as IInvokableAbility)?.RequiredOrbs;
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        var sphere = (ActiveAbility) _main.Context.AbilityFactory.GetAbility(abilityId);
                        if (sphere == null) return false;
                        if (!sphere.UseAbility()) return false;
                        InvokerCrappahilationPaid.Log.Debug($"Invoke [Sphere: {abilityId}] ({ability})");
                    }

                    var invoked = Abilities.Invoke.Ability.UseAbility();
                    if (invoked)
                    {
                        _sleeper.Sleep(200, $"{ability} shit");
                        InvokerCrappahilationPaid.Log.Debug($"Invoke [{ability}]");
                    }

                    return invoked;
                }

                InvokerCrappahilationPaid.Log.Debug($"Error in Invoke function: {ability.Ability.Id}");
                return false;
            }

            InvokerCrappahilationPaid.Log.Debug($"Invoke [on cd] ({ability})");
            return false;
        }
    }
}