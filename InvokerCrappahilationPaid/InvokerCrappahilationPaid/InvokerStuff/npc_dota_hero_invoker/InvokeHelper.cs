// <copyright file="InvokeHelper.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Divine;
using Divine.SDK.Extensions;
using Divine.Zero.Loader;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    internal class InvokeHelper<T>
        where T : ActiveAbility, IInvokableAbility, IHaveFastInvokeKey
    {
        private readonly T _invokableAbility;

        private readonly InvokerInvoke _invoke;

        private readonly HashSet<ActiveAbility> _myOrbs = new HashSet<ActiveAbility>();

        private readonly Dictionary<string, AbilityId> _orbModifiers = new Dictionary<string, AbilityId>(3);

        private readonly Unit _owner;

        private float _invokeTime;

        public InvokeHelper(T ability)
        {
            _invokableAbility = ability;
            _owner = ability.Owner;

            var wexAbility = _owner.GetAbilityById(AbilityId.invoker_wex) ??
                             EntityManager<Ability>.Entities.FirstOrDefault(x =>
                                 x.IsValid && x.Id == AbilityId.invoker_wex);
            if (wexAbility != null)
            {
                Wex = new InvokerWex(wexAbility);
                _orbModifiers.Add(Wex.ModifierName, Wex.Ability.Id);
                _myOrbs.Add(Wex);
            }

            var quasAbility = _owner.GetAbilityById(AbilityId.invoker_quas) ??
                              EntityManager<Ability>.Entities.FirstOrDefault(x =>
                                  x.IsValid && x.Id == AbilityId.invoker_quas);
            if (quasAbility != null)
            {
                Quas = new InvokerQuas(quasAbility);
                _orbModifiers.Add(Quas.ModifierName, Quas.Ability.Id);
                _myOrbs.Add(Quas);
            }

            var exortAbility = _owner.GetAbilityById(AbilityId.invoker_exort)
                               ?? EntityManager<Ability>.Entities.FirstOrDefault(x =>
                                   x.IsValid && x.Id == AbilityId.invoker_exort);
            if (exortAbility != null)
            {
                Exort = new InvokerExort(exortAbility);
                _orbModifiers.Add(Exort.ModifierName, Exort.Ability.Id);
                _myOrbs.Add(Exort);
            }

            var invokeAbility = _owner.GetAbilityById(AbilityId.invoker_invoke);
            if (invokeAbility != null) _invoke = new InvokerInvoke(invokeAbility);
        }

        public InvokerExort Exort { get; }

        public bool IsInvoked
        {
            get
            {
                if (!_invokableAbility.Ability.IsHidden) return true;

                return _invokeTime + 0.5f > Game.RawGameTime;
            }
        }

        public InvokerQuas Quas { get; }

        public InvokerWex Wex { get; }

        public bool CanInvoke(bool checkAbilityManaCost)
        {
            if (IsInvoked) return true;

            if (_invoke?.CanBeCasted != true) return false;

            if (checkAbilityManaCost && _owner.Mana < _invoke.ManaCost + _invokableAbility.ManaCost) return false;

            return true;
        }

        public bool Invoke(List<AbilityId> currentOrbs, bool skipCheckingForInvoked = false)
        {
            if (IsInvoked && (!skipCheckingForInvoked || _invokableAbility.Ability.AbilitySlot == AbilitySlot.Slot_4))
            {
                InvokerCrappahilationPaid.Log.Debug(
                    $"[Invoke] {_invokableAbility} already invoked -> {IsInvoked} Skip: {skipCheckingForInvoked}");
                return IsInvoked;
            }

            /*if (skipCheckingForInvoked)
            {
                return InvokeThisShit();
            }*/

            if (_invoke?.CanBeCasted != true)
            {
                InvokerCrappahilationPaid.Log.Debug(
                    $"[Invoke] {_invokableAbility} cant cast invoke ({_invoke?.Ability.Cooldown})");
                return false;
            }

            var orbs = currentOrbs ?? _owner.Modifiers.Where(x => !x.IsHidden && _orbModifiers.ContainsKey(x.Name))
                           .Select(x => _orbModifiers[x.Name]).ToList();
            var missingOrbs = GetMissingOrbs(orbs);

            foreach (var id in missingOrbs)
            {
                var orb = _myOrbs.FirstOrDefault(x => x.Ability.Id == id && x.CanBeCasted);
                if (orb == null)
                {
                    InvokerCrappahilationPaid.Log.Debug(
                        $"[Invoke] {_invokableAbility} cant cast needed sphere [{id}] (1)");
                    return false;
                }

                if (!orb.UseAbility())
                {
                    InvokerCrappahilationPaid.Log.Debug(
                        $"[Invoke] {_invokableAbility} cant cast needed sphere [{id}] (2)");
                    return false;
                }
            }

            var invoked = _invoke.UseAbility();
            if (invoked) _invokeTime = Game.RawGameTime;
            InvokerCrappahilationPaid.Log.Debug($"[Invoke] {_invokableAbility} invoked");
            return invoked;
        }


        public bool SafeInvoke(ActiveAbility target)
        {
            if (target.Ability.AbilitySlot == AbilitySlot.Slot_5)
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
            if (_invoke.CanBeCasted)
            {
                var requiredOrbs = _invokableAbility.RequiredOrbs;
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        var sphere = _myOrbs.FirstOrDefault(x => x.Ability.Id == abilityId && x.CanBeCasted);
                        if (sphere == null) return false;
                        if (!sphere.UseAbility()) return false;
                        InvokerCrappahilationPaid.Log.Debug($"Invoke [Sphere: {abilityId}] ({_invokableAbility})");
                    }

                    var invoked = _invoke.UseAbility();
                    if (invoked) _invokeTime = Game.RawGameTime;
                    InvokerCrappahilationPaid.Log.Debug($"Invoke [{_invokableAbility}]");
                    return true;
                }

                InvokerCrappahilationPaid.Log.Debug($"Error in Invoke function: {_invokableAbility.Ability.Id}");
                return false;
            }

            return false;
        }

        private bool InvokeThisShit(ActiveAbility ability)
        {
            InvokerCrappahilationPaid.Log.Debug($"Trying to invoke -> {ability.Ability.Id}");
            if (_invoke.Ability.AbilityState == AbilityState.Ready)
            {
                var requiredOrbs = (ability as IInvokableAbility)?.RequiredOrbs;
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        var sphere = _myOrbs.FirstOrDefault(x => x.Ability.Id == abilityId && x.CanBeCasted);
                        if (sphere == null)
                        {
                            InvokerCrappahilationPaid.Log.Debug(
                                $"Invoke [Sphere: {abilityId} == null]{ability.Ability.Id}");
                            return false;
                        }

                        if (!sphere.UseAbility())
                        {
                            InvokerCrappahilationPaid.Log.Debug(
                                $"Invoke [Sphere: {abilityId} on cd]{ability.Ability.Id}");
                            return false;
                        }

                        InvokerCrappahilationPaid.Log.Debug($"Invoke [Sphere: {abilityId}] ({ability})");
                    }

                    var invoked = _invoke.UseAbility();
                    if (invoked) InvokerCrappahilationPaid.Log.Debug($"Invoke [{ability}]");
                    return invoked;
                }

                InvokerCrappahilationPaid.Log.Debug($"Error in Invoke function: {ability.Ability.Id}");
                return false;
            }

            InvokerCrappahilationPaid.Log.Debug($"{ability.Ability.Id} invoke on cd");
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
            InvokerCrappahilationPaid.Log.Debug($"[Use] {_invokableAbility}");
            return true;
        }

        public void SetKey(int key)
        {
            _invokableAbility.Key = KeyInterop.KeyFromVirtualKey(key);
        }
    }
}