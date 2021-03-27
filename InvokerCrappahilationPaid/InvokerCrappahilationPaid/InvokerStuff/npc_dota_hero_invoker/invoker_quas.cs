// <copyright file="invoker_quas.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>


using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Components;
using Ensage.SDK.Extensions;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerQuas : ActiveAbility, IHasModifier
    {
        public InvokerQuas(Ability ability)
            : base(ability)
        {
        }

        public override bool CanBeCasted
        {
            get
            {
                if (!IsReady) return false;

                var owner = Owner;
                if (owner.IsStunned() || owner.IsSilenced()) return false;

                // skip LastCastAttempt check

                return true;
            }
        }

        public uint Level
        {
            get
            {
                var level = Ability.Level;
                if (Owner.HasAghanimsScepter()) level++;

                return level;
            }
        }

        public string ModifierName { get; } = "modifier_invoker_quas_instance";
    }
}