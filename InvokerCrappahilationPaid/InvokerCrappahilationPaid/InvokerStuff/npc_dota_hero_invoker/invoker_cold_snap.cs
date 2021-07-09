<<<<<<< HEAD
﻿// <copyright file="invoker_cold_snap.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;
=======
﻿using System.Windows.Input;
using Divine;
using O9K.Core.Entities.Abilities.Heroes.Invoker;
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerColdSnap : ColdSnap, IHaveFastInvokeKey
    {
        public InvokerColdSnap(Ability ability)
            : base(ability)
        {
        }

        public Key Key { get; set; }
    }
}