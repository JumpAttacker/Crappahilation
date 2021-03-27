// <copyright file="invoker_deafening_blast.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;

using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Components;
using Ensage.SDK.Extensions;
using SharpDX;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerDeafeningBlast : ConeAbility, IInvokableAbility, IHaveFastInvokeKey, IHasTargetModifier
    {
        private readonly InvokeHelper<InvokerDeafeningBlast> _invokeHelper;

        public InvokerDeafeningBlast(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerDeafeningBlast>(this);
        }

        public override UnitState AppliesUnitState { get; } = UnitState.Disarmed;

        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);

        public override float EndRadius => Ability.GetAbilitySpecialData("radius_end");

        public override float Radius => Ability.GetAbilitySpecialData("radius_start");

        public override float Speed => Ability.GetAbilitySpecialData("travel_speed");

        protected override float RawDamage => Ability.GetAbilitySpecialData("damage", _invokeHelper.Exort.Level);
        public string TargetModifierName { get; } = "modifier_invoker_deafening_blast_knockback";

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
            {AbilityId.invoker_quas, AbilityId.invoker_wex, AbilityId.invoker_exort};

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
    }
}