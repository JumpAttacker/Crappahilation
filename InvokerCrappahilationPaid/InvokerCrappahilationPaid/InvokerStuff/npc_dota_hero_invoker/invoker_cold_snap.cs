// <copyright file="invoker_cold_snap.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerColdSnap : RangedAbility, IInvokableAbility, IHasTargetModifier, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<InvokerColdSnap> _invokeHelper;

        public InvokerColdSnap(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerColdSnap>(this);
        }

        public override UnitState AppliesUnitState { get; } = UnitState.Stunned;

        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);

        public override float Duration =>
            Ability.GetAbilitySpecialDataWithTalent(Owner, "duration", _invokeHelper.Quas.Level);

        protected override float RawDamage => Ability.GetAbilitySpecialData("freeze_damage", _invokeHelper.Quas.Level);

        public string TargetModifierName { get; } = "modifier_invoker_cold_snap";

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
            {AbilityId.invoker_quas, AbilityId.invoker_quas, AbilityId.invoker_quas};

        public bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        public override bool UseAbility(Unit target)
        {
            return Invoke() && base.UseAbility(target) && _invokeHelper.Casted();
        }
    }
}