// <copyright file="invoker_forge_spirit.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;

using Divine.Entity.Entities.Abilities.Components;
using Divine.Input;

using O9K.Core.Entities.Abilities.Heroes.Invoker;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerForgeSpirit : InvokerBaseAbility
    {
        private readonly InvokeHelper<ForgeSpirit> _invokeHelper;

        public InvokerForgeSpirit(ForgeSpirit ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<ForgeSpirit>(ability);
        }

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
            {AbilityId.invoker_exort, AbilityId.invoker_exort, AbilityId.invoker_quas};

        public override bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }
    }
}