// <copyright file="invoker_alacrity.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;
using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Components;
using Ensage.SDK.Extensions;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerAlacrity : ActiveAbility, IInvokableAbility, IHasModifier, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<InvokerAlacrity> _invokeHelper;

        public InvokerAlacrity(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerAlacrity>(this);
        }

        public float BonusDamage =>
            Ability.GetAbilitySpecialDataWithTalent(Owner, "bonus_damage", _invokeHelper.Exort.Level);

        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);

        public override float Duration => Ability.GetAbilitySpecialData("duration");

        public string ModifierName { get; } = "modifier_invoker_alacrity";

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
            {AbilityId.invoker_wex, AbilityId.invoker_wex, AbilityId.invoker_exort};

        public bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        public override bool UseAbility(Unit target)
        {
            return Invoke() && base.UseAbility(target) && _invokeHelper.Casted();
        }
    }
}