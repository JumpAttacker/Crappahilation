﻿using System.Collections.Generic;

using Divine;
using Divine.Entity.Entities.Abilities.Components;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public interface IInvokableAbility
    {
        AbilityId[] RequiredOrbs { get; }
        bool IsInvoked { get; }
        bool CanBeInvoked { get; }

        bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false);
    }
}