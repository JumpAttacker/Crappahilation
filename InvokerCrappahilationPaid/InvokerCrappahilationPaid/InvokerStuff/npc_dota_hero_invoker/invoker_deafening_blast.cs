// <copyright file="invoker_deafening_blast.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;

using Divine.Entity.Entities.Abilities.Components;
using Divine.Input;

using O9K.Core.Entities.Abilities.Heroes.Invoker;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerDeafeningBlast : InvokerBaseAbility
    {
        private readonly InvokeHelper<DeafeningBlast> _invokeHelper;

        public InvokerDeafeningBlast(DeafeningBlast ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<DeafeningBlast>(ability);
        }

        public string TargetModifierName { get; } = "modifier_invoker_deafening_blast_knockback";

        public override Key Key { get; set; }

        public override bool CanBeInvoked
        {
            get
            {
                if (IsInvoked) return true;

                return _invokeHelper.CanInvoke(false);
            }
        }

        public override bool IsInvoked => _invokeHelper.IsInvoked;

        public override AbilityId[] RequiredOrbs { get; } =
            {AbilityId.invoker_quas, AbilityId.invoker_wex, AbilityId.invoker_exort};

        public override bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }
        //
        // public override bool UseAbility(Unit target)
        // {
        //     return Invoke() && base.UseAbility(target) && _invokeHelper.Casted();
        // }
        //
        // public override bool UseAbility(Vector3 position)
        // {
        //     return Invoke() && base.UseAbility(position) && _invokeHelper.Casted();
        // }
    }
}