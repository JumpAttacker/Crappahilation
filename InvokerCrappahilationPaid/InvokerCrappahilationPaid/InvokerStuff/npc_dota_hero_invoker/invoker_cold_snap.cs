using System.Windows.Input;
using Divine;
using O9K.Core.Entities.Abilities.Heroes.Invoker;

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