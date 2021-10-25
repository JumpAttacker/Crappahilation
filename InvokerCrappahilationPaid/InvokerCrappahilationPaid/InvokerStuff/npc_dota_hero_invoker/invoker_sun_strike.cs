using System.Collections.Generic;

using Divine.Entity.Entities.Abilities.Components;
using Divine.Input;

using O9K.Core.Entities.Abilities.Heroes.Invoker;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerSunStrike : InvokerBaseAbility
    {
        private readonly InvokeHelper<SunStrike> _invokeHelper;

        public InvokerSunStrike(SunStrike ability) : base(ability)
        {
            _invokeHelper = new InvokeHelper<SunStrike>(ability);
        }

        public bool IsCataclysmActive =>  Owner.HasAghanimShard || Owner.HasAghanimsScepter;

        public override Key Key { get; set; }

        public override bool CanBeInvoked
        {
            get
            {
                if (IsInvoked) return true;

                return _invokeHelper.CanInvoke(false);
            }
        }

        public override bool IsInvoked => _invokeHelper.IsInvoked;

        public override AbilityId[] RequiredOrbs { get; } =
            {AbilityId.invoker_exort, AbilityId.invoker_exort, AbilityId.invoker_exort};

        public override bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        // public override bool UseAbility(Unit target)
        // {
        //     return Invoke() && base.UseAbility(target);
        // }
        //
        // public override bool UseAbility()
        // {
        //     return Invoke() && base.UseAbility() && _invokeHelper.Casted();
        // }
        //
        // public override bool UseAbility(Vector3 position)
        // {
        //     return Invoke() && base.UseAbility(position) && _invokeHelper.Casted();
        // }
    }
}