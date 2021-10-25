// <copyright file="invoker_ghost_walk.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;

using Divine.Entity.Entities.Abilities.Components;
using Divine.Input;

using O9K.Core.Entities.Abilities.Heroes.Invoker;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerGhostWalk : InvokerBaseAbility
    {
        private readonly InvokeHelper<GhostWalk> _invokeHelper;

        public InvokerGhostWalk(GhostWalk ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<GhostWalk>(ability);
        }

        public string ModifierName { get; } = "modifier_invoker_ghost_walk_self";

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
            {AbilityId.invoker_quas, AbilityId.invoker_quas, AbilityId.invoker_wex};

        public override bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        // public override bool UseAbility()
        // {
        //     return Invoke() && base.UseAbility() && _invokeHelper.Casted();
        // }
    }
}