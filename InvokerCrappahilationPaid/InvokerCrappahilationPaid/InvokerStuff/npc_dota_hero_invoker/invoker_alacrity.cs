using System.Collections.Generic;
using Divine.Entity.Entities.Abilities;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Input;

using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Abilities.Heroes.Invoker;
using O9K.Core.Entities.Units;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerAlacrity : InvokerBaseAbility, IInvokableAbility, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<Alacrity> _invokeHelper;

        public InvokerAlacrity(Alacrity ability) : base(ability)
        {
            _invokeHelper = new InvokeHelper<Alacrity>(ability);
        }


        public string ModifierName { get; } = "modifier_invoker_alacrity";

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
            {AbilityId.invoker_wex, AbilityId.invoker_wex, AbilityId.invoker_exort};

        public override bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        public bool UseAbility(Unit9 unit9, bool queue, bool bypass)
        {
            return Invoke() && BaseAbility.BaseAbility.Cast(unit9, queue, bypass) && _invokeHelper.Casted();
        }

    }
}