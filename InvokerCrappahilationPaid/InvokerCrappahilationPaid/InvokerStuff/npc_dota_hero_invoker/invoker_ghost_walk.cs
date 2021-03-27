// <copyright file="invoker_ghost_walk.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;

using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Components;
using Ensage.SDK.Extensions;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerGhostWalk : ActiveAbility, IInvokableAbility, IHasModifier, IHasTargetModifier,
        IAreaOfEffectAbility, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<InvokerGhostWalk> _invokeHelper;

        public InvokerGhostWalk(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerGhostWalk>(this);
        }

        public override UnitState AppliesUnitState { get; } = UnitState.Invisible;

        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);

        public override float Duration => Ability.GetAbilitySpecialData("duration");

        public float Radius => Ability.GetAbilitySpecialData("area_of_effect");

        public string ModifierName { get; } = "modifier_invoker_ghost_walk_self";

        public string TargetModifierName { get; } = "modifier_invoker_ghost_walkenemy";

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
            {AbilityId.invoker_quas, AbilityId.invoker_quas, AbilityId.invoker_wex};

        public bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        public override bool UseAbility()
        {
            return Invoke() && base.UseAbility() && _invokeHelper.Casted();
        }
    }
}