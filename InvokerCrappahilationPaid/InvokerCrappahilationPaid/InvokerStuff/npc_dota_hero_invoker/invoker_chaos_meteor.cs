using System.Collections.Generic;
using System.Windows.Input;
using Divine.Entity.Entities.Abilities.Components;
using InvokerCrappahilationPaid.Extensions;
using O9K.Core.Entities.Abilities.Heroes.Invoker;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerChaosMeteor : InvokerBaseAbility
    {
        private readonly InvokeHelper<ChaosMeteor> _invokeHelper;

        public InvokerChaosMeteor(ChaosMeteor ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<ChaosMeteor>(ability);
        }

        public string TargetModifierName { get; } = "modifier_invoker_chaos_meteor_burn";

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
            {AbilityId.invoker_exort, AbilityId.invoker_exort, AbilityId.invoker_wex};

        public override bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        // public override bool UseAbility(Unit target)
        // {
        //     return Invoke() && base.UseAbility(target) && _invokeHelper.Casted();
        // }
        //
        // public override bool UseAbility(Vector3 position)
        // {
        //     return Invoke() && base.UseAbility(position) && _invokeHelper.Casted();
        // }
    }
}