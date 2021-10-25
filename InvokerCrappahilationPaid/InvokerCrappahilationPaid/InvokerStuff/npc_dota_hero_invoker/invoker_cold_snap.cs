using System.Collections.Generic;

using Divine.Entity.Entities.Abilities.Components;
using Divine.Input;

using O9K.Core.Entities.Abilities.Heroes.Invoker;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerColdSnap : InvokerBaseAbility
    {
        private readonly InvokeHelper<ColdSnap> _invokeHelper;

        public InvokerColdSnap(ColdSnap ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<ColdSnap>(ability);
        }


        public string TargetModifierName { get; } = "modifier_invoker_cold_snap";

        public override Key Key { get; set; }

        public override bool CanBeInvoked
        {
            get
            {
                if (IsInvoked) return true;

                return _invokeHelper.CanInvoke(false);
            }
        }

        public override AbilityId[] RequiredOrbs { get; } = new[] {AbilityId.invoker_quas, AbilityId.invoker_quas, AbilityId.invoker_quas};
        public override bool IsInvoked => _invokeHelper.IsInvoked;


        public override bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        // public bool UseAbility(Unit target)
        // {
        //     return Invoke() && base.UseAbility(target) && _invokeHelper.Casted();
        // }
    }
}