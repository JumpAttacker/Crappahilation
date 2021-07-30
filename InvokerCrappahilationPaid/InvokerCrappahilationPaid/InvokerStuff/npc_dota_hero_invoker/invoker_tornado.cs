using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Divine.Entity.Entities.Abilities;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units;
using Divine.Entity.Entities.Units.Components;
using Divine.Numerics;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Abilities.Heroes.Invoker;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerTornado :  InvokerBaseAbility
    {
        private readonly InvokeHelper<Tornado> _invokeHelper;

        public InvokerTornado(Tornado ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<Tornado>(ability);
            // this.Owner = ability.Owner;
        }

        public string TargetModifierName { get; } = "modifier_invoker_tornado";

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
            {AbilityId.invoker_wex, AbilityId.invoker_wex, AbilityId.invoker_quas};

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

        public bool SafeInvoke(params ActiveAbility[] targetAbility)
        {
            return targetAbility.Where(activeAbility => activeAbility.BaseAbility.AbilityState == AbilityState.Ready).Any(activeAbility => _invokeHelper.SafeInvoke(activeAbility));
        }
    }
}