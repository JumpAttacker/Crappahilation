<<<<<<< HEAD
﻿// <copyright file="invoker_ghost_walk.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;
=======
﻿using System.Windows.Input;
using Divine;
using Divine.Core.Entities.Abilities;
using Divine.Core.Entities.Abilities.Components;
using O9K.Core.Entities.Abilities.Heroes.Invoker;
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerGhostWalk : GhostWalk, IHasModifier, IHasTargetModifier,
        IAreaOfEffectAbility, IHaveFastInvokeKey
    {
        public InvokerGhostWalk(Ability baseAbility) : base(baseAbility)
        {
            
        }

        public Key Key { get; set; }
        public string ModifierName { get; } = "modifier_invoker_ghost_walk_self";

        public string TargetModifierName { get; } = "modifier_invoker_ghost_walkenemy";
    } 
}