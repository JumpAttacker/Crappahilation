using System.Collections.Generic;
using System.Windows.Input;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Numerics;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Units;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public abstract class InvokerBaseAbility : InvokerSimpleBaseAbility, IInvokableAbility, IHaveFastInvokeKey
    {
        protected InvokerBaseAbility(ActiveAbility activeAbility) : base(activeAbility){}
        
        public abstract AbilityId[] RequiredOrbs { get; }
        public abstract bool IsInvoked { get; }
        public abstract bool CanBeInvoked { get; }
        public abstract bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false);
        public abstract Key Key { get; set; }

        public bool CanBeCasted()
        {
            return BaseAbility.CanBeCasted();
        }
    }
    public abstract class InvokerSimpleBaseAbility : IHaveActiveAbility<ActiveAbility> 
    {
        public InvokerSimpleBaseAbility(ActiveAbility activeAbility)
        {
            BaseAbility = activeAbility;
        }

        public ActiveAbility BaseAbility { get; set; }

        public uint Level => BaseAbility.Level;
        public bool IsReady => BaseAbility.IsReady;
        public AbilityId Id => BaseAbility.Id;
        public AbilityState AbilityState => BaseAbility.BaseAbility.AbilityState;
        public AbilitySlot AbilitySlot => BaseAbility.AbilitySlot;
        public Unit9 Owner => BaseAbility.Owner;

        public bool UseAbility()
        {
            
            return BaseAbility.UseAbility();
        }
        public bool UseAbility(Vector3 pos)
        {
            
            return BaseAbility.UseAbility(pos);
        }
        public bool UseAbility(Unit9 target)
        {
            
            return BaseAbility.UseAbility(target);
        }

    }
}