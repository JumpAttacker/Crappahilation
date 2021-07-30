

using System;
using System.Collections.Generic;
using System.Linq;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units;
using Divine.Extensions;
using Divine.Game;
using InvokerCrappahilationPaid.Extensions;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Abilities.Heroes.Invoker.BaseAbilities;
using O9K.Core.Managers.Entity;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    internal class InvokeHelper<T> where T: ActiveAbility, O9K.Core.Entities.Abilities.Heroes.Invoker.Helpers.IInvokableAbility
    {
        private readonly T _invokableAbility;

        private readonly InvokerInvoke _invoke;

        private readonly HashSet<ActiveAbility> _myOrbs = new();

        private readonly Dictionary<string, AbilityId> _orbModifiers = new(3);

        private readonly Unit _owner;

        private float _invokeTime;

        public InvokeHelper(T ability)
        {
            _invokableAbility = ability;
            // Console.WriteLine($"InvokeHelper {ability.BaseAbility.Owner}");
            _owner = ability.Owner;

            var wexAbility = _owner.GetAbilityById(AbilityId.invoker_wex);
            Wex = EntityManager9.GetAbility(wexAbility.Handle) as Wex;
            _orbModifiers.Add(Wex.ModifierName ?? string.Empty, Wex.BaseAbility.Id);
            _myOrbs.Add(Wex);

            var quasAbility = _owner.GetAbilityById(AbilityId.invoker_quas);
            Quas = EntityManager9.GetAbility(quasAbility.Handle) as Quas;
            _orbModifiers.Add(Quas.ModifierName, Quas.BaseAbility.Id);
            _myOrbs.Add(Quas);

            var exortAbility = _owner.GetAbilityById(AbilityId.invoker_exort);
            Exort = EntityManager9.GetAbility(exortAbility.Handle) as Exort;
            _orbModifiers.Add(Exort.ModifierName, Exort.BaseAbility.Id);
            _myOrbs.Add(Exort);

            var invokeAbility = _owner.GetAbilityById(AbilityId.invoker_invoke);
            _invoke = new InvokerInvoke(EntityManager9.GetAbility(invokeAbility.Handle) as Invoke);
        }
        
        public Exort Exort { get; }

        public bool IsInvoked
        {
            get
            {
                if (!_invokableAbility.BaseAbility.IsHidden) return true;

                return _invokeTime + 0.5f > GameManager.RawGameTime;
            }
        }

        public Quas Quas { get; }

        public Wex Wex { get; }

        public bool CanInvoke(bool checkAbilityManaCost)
        {
            if (IsInvoked) return true;

            if (_invoke?.BaseAbility.CanBeCasted() != true) return false;

            if (checkAbilityManaCost && _owner.Mana < _invoke.BaseAbility.ManaCost + _invokableAbility.BaseAbility.ManaCost) return false;

            return true;
        }

        public bool Invoke(List<AbilityId> currentOrbs, bool skipCheckingForInvoked = false)
        {
            if (IsInvoked && (!skipCheckingForInvoked || _invokableAbility.BaseAbility.AbilitySlot == AbilitySlot.Slot4))
            {
                return IsInvoked;
            }

            /*if (skipCheckingForInvoked)
            {
                return InvokeThisShit();
            }*/
            if (_invoke.BaseAbility.CanBeCasted() != true)
            {
                return false;
            }
            var orbs = currentOrbs ?? _owner.Modifiers.Where(x => !x.IsHidden && _orbModifiers.ContainsKey(x.Name))
                           .Select(x => _orbModifiers[x.Name]).ToList();
            var missingOrbs = GetMissingOrbs(orbs);

            foreach (var id in missingOrbs)
            {
                var orb = _myOrbs.FirstOrDefault(x => x.Id == id && x.BaseAbility.CanBeCasted());
                if (orb == null)
                {
                    return false;
                }

                if (!orb.BaseAbility.Cast())
                {
                    return false;
                }
            }
            var invoked = _invoke.BaseAbility.BaseAbility.Cast();
            if (invoked) _invokeTime = GameManager.RawGameTime;
            return invoked;
        }


        public bool SafeInvoke(ActiveAbility target)
        {
            if (target.AbilitySlot == AbilitySlot.Slot5)
            {
                var a = InvokeThisShit(target);
                var b = InvokeThisShit();
                return a && b;
            }
            else
            {
                var a = InvokeThisShit();
                var b = InvokeThisShit(target);
                return a && b;
            }
        }

        private bool InvokeThisShit()
        {
            if (_invoke.BaseAbility.CanBeCasted())
            {
                var requiredOrbs = _invokableAbility.RequiredOrbs;
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        var sphere = _myOrbs.FirstOrDefault(x => x.Id == abilityId && x.BaseAbility.CanBeCasted());
                        if (sphere == null) return false;
                        if (!sphere.UseAbility()) return false;
                    }

                    var invoked = _invoke.BaseAbility.UseAbility();
                    if (invoked) _invokeTime = GameManager.RawGameTime;
                    return true;
                }

                return false;
            }

            return false;
        }

        private bool InvokeThisShit(ActiveAbility ability)
        {
            if (_invoke.BaseAbility.BaseAbility.AbilityState == AbilityState.Ready)
            {
                var requiredOrbs = (ability as IInvokableAbility)?.RequiredOrbs;
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        var sphere = _myOrbs.FirstOrDefault(x => x.Id == abilityId && x.BaseAbility.CanBeCasted());
                        if (sphere == null)
                        {
                            return false;
                        }

                        if (!sphere.UseAbility())
                        {
                            return false;
                        }
                    }

                    var invoked = _invoke.BaseAbility.UseAbility();
                    return invoked;
                }

                return false;
            }

            return false;
        }

        private IEnumerable<AbilityId> GetMissingOrbs(List<AbilityId> castedOrbs)
        {
            var orbs = castedOrbs.ToList();
            var missing = _invokableAbility.RequiredOrbs.Where(x => !orbs.Remove(x)).ToList();

            if (!missing.Any()) return Enumerable.Empty<AbilityId>();

            castedOrbs.RemoveRange(0,
                Math.Max(castedOrbs.Count - _invokableAbility.RequiredOrbs.Length + missing.Count, 0));
            castedOrbs.AddRange(missing);

            return missing.Concat(GetMissingOrbs(castedOrbs));
        }

        public bool Casted()
        {
            return true;
        }

        // public void SetKey(int key)
        // {
        //     _invokableAbility.Key = KeyInterop.KeyFromVirtualKey(key);
        // }
    }
}