using System.Windows.Input;
using Divine;
using Divine.Core.Entities.Abilities;
using Divine.Core.Entities.Abilities.Components;
using O9K.Core.Entities.Abilities.Heroes.Invoker;

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