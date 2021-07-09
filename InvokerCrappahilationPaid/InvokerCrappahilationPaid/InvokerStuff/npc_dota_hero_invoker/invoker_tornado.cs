// <copyright file="invoker_tornado.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;

using SharpDX;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerTornado : LineAbility, IInvokableAbility, IHasTargetModifier, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<InvokerTornado> _invokeHelper;

        public InvokerTornado(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerTornado>(this);
        }

        public override UnitState AppliesUnitState { get; } = UnitState.Invulnerable;

        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);

        public override float Duration =>
            Ability.GetAbilitySpecialDataWithTalent(Owner, "lift_duration", _invokeHelper.Wex.Level); /* +
            (_extraTimeAbility.Level > 0
                ? _extraTimeAbility.GetAbilitySpecialDataWithTalent(Owner, "value", _extraTimeAbility.Level)
                : 0);*/

        public override float Radius => Ability.GetAbilitySpecialData("area_of_effect");

        public override float Range => Ability.GetAbilitySpecialData("travel_distance", _invokeHelper.Wex.Level);

        public override float Speed => Ability.GetAbilitySpecialData("travel_speed");

        protected override float RawDamage
        {
            get
            {
                var baseDamage = Ability.GetAbilitySpecialData("base_damage");
                var wexDamage = Ability.GetAbilitySpecialData("wex_damage", _invokeHelper.Wex.Level);

                return baseDamage + wexDamage;
            }
        }

        public string TargetModifierName { get; } = "modifier_invoker_tornado";

        public Key Key { get; set; }

        public bool CanBeInvoked
        {
            get
            {
                if (IsInvoked) return true;

                return _invokeHelper.CanInvoke(false);
            }
        }

        public bool IsInvoked => _invokeHelper.IsInvoked;

        public AbilityId[] RequiredOrbs { get; } =
            {AbilityId.invoker_wex, AbilityId.invoker_wex, AbilityId.invoker_quas};

        public bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        public override bool UseAbility(Unit target)
        {
            return Invoke() && base.UseAbility(target) && _invokeHelper.Casted();
        }

        public override bool UseAbility(Vector3 position)
        {
            return Invoke() && base.UseAbility(position) && _invokeHelper.Casted();
        }

        public bool SafeInvoke(params ActiveAbility[] targetAbility)
        {
            foreach (var activeAbility in targetAbility)
                if (activeAbility.Ability.AbilityState == AbilityState.Ready)
                {
                    InvokerCrappahilationPaid.Log.Debug($"SafeInvoke: {this} <-> {activeAbility}");
                    if (_invokeHelper.SafeInvoke(activeAbility)) return true;
                }

            return false;
        }
    }
}