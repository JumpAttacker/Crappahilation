// <copyright file="IInvokableAbility.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using Ensage;

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