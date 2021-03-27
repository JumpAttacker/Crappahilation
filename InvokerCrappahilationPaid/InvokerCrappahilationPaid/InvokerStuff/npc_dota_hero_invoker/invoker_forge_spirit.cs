// <copyright file="invoker_forge_spirit.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;

using Ensage.SDK.Abilities;
using Ensage.SDK.Extensions;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerForgeSpirit : ActiveAbility, IInvokableAbility, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<InvokerForgeSpirit> _invokeHelper;

        public InvokerForgeSpirit(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerForgeSpirit>(this);
        }

        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);

        public override float Duration => Ability.GetAbilitySpecialData("spirit_duration", _invokeHelper.Quas.Level);

        public Key Key { get; set; }

        public bool CanBeInvoked
        {
            get
            {
                if (IsInvoked) return true;

                return _invokeHelper.CanInvoke(false);
            }
        }

        public bool IsInvoked => _invokeHelper.IsInvoked;

        public AbilityId[] RequiredOrbs { get; } =
            {AbilityId.invoker_exort, AbilityId.invoker_exort, AbilityId.invoker_quas};

        public bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        public override bool UseAbility()
        {
            return Invoke() && base.UseAbility() && _invokeHelper.Casted();
        }
    }
}