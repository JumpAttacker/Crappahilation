using System.Collections.Generic;
using System.Linq;
using Divine;
using Divine.Core.Helpers;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Abilities.Heroes.Invoker.Helpers;

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
            UpdateManager.BeginInvoke(100, () =>
            {
                var comboUpdateHandler = UpdateManager.CreateUpdate(100, false, PrepareInAction);
                Config.PrepareKey.ValueChanged += (sender, e) => { comboUpdateHandler.IsEnabled = e.Value; };
            });
        }

        private Combo.ComboTypeEnum GameplayType =>
            _main.Config.ComboPanel.IsAutoComboSelected ? Combo.ComboTypeEnum.Auto : Combo.ComboTypeEnum.CustomCombo;

        private Config Config => _main.Config;
        private AbilitiesInCombo Abilities => _main.AbilitiesInCombo;
        private uint ExortLevel => Abilities.Exort.Level;
        private uint WexLevel => Abilities.Wex.Level;
        private uint QuasLevel => Abilities.Quas.Level;
        private Hero Me => (Hero) EntityManager.LocalHero;

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
                return;
            }

            if (two == null)
            {
                if (one is IInvokableAbility invokable && !invokable.IsInvoked && invokable.CanBeInvoked)
                    invokable.Invoke();
                return;
            }

            var empty1 = Me.Spellbook.Spell4;
            var empty2 = Me.Spellbook.Spell5;

            var ability1Invoked = one.BaseAbility.Equals(empty1) || one.BaseAbility.Equals(empty2);
            var ability2Invoked = two.BaseAbility.Equals(empty1) || two.BaseAbility.Equals(empty2);
            if (ability1Invoked && ability2Invoked)
            {
                if (one.BaseAbility.Equals(empty1))
                    InvokeThisShit(two);
                else if (two.BaseAbility.Equals(empty2)) InvokeThisShit(one);
                return;
            }

            if (ability1Invoked)
            {
                if (one.BaseAbility.Equals(empty2))
                    InvokeThisShit(one);
                else
                    InvokeThisShit(two);
            }
            else if (ability2Invoked)
            {
                if (two.BaseAbility.Equals(empty2))
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
            var firstAbility = allAbilities.First(x => !x.StartsWith("item_") && list.All(z => z.Name != x));

            var found = _main.AbilitiesInCombo.AllAbilities.Find(z =>
                !list.Contains(z) && z.Name == firstAbility);
            if (found != null) abilities.Add(found);
            return found;
        }

        private bool InvokeThisShit(ActiveAbility ability)
        {
            //TODO: возможно вернуть слипы
            // if (_sleeper.Sleeping($"{ability.Name} shit"))
            // {
            //     InvokerCrappahilationPaid.Log.Debug($"Invoke [blocked] ({ability})");
            //     return false;
            // }

            if (Abilities.Invoke.IsReady)
            {
                var requiredOrbs = (ability as IInvokableAbility)?.RequiredOrbs;
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        var sphere = (ActiveAbility) Abilities.AllAbilities.FirstOrDefault(x => x.Id == abilityId);
                        if (sphere == null) return false;
                        if (!sphere.UseAbility()) return false;
                    }

                    var invoked = Abilities.Invoke.UseAbility();
                    // if (invoked)
                    // {
                    //     _sleeper.Sleep(200, $"{ability} shit");
                    // }

                    return invoked;
                }
                return false;
            }
            return false;
        }
    }
}