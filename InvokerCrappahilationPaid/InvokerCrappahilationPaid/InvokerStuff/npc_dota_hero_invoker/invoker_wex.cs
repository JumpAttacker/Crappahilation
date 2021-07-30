using Divine.Entity.Entities.Abilities;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Abilities.Heroes.Invoker.BaseAbilities;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerWex : InvokerSimpleBaseAbility
    {
        public InvokerWex(Wex baseAbility) : base(baseAbility)
        {
            BaseAbility = baseAbility;
        }
    }
}