using O9K.Core.Entities.Abilities.Base;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public interface IHaveActiveAbility<T> where T: ActiveAbility
    {
        public T BaseAbility { get; set; }
    }
}