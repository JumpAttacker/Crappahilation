// <copyright file="invoker_emp.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System;
using System.Collections.Generic;
using System.Windows.Input;

using SharpDX;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerEmp : CircleAbility, IInvokableAbility, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<InvokerEmp> _invokeHelper;

        public InvokerEmp(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerEmp>(this);
        }

        public override float ActivationDelay => Ability.GetAbilitySpecialData("delay");

        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);

        public override float Radius => Ability.GetAbilitySpecialData("area_of_effect");

        protected override float RawDamage => Ability.GetAbilitySpecialData("mana_burned", _invokeHelper.Wex.Level);

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
            {AbilityId.invoker_wex, AbilityId.invoker_wex, AbilityId.invoker_wex};

        public bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        public override float GetDamage(params Unit[] targets)
        {
            var totalDamage = 0.0f;

            var damage = RawDamage;
            var damagePercentage = Ability.GetAbilitySpecialData("damage_per_mana_pct") / 100f;
            var amplify = Owner.GetSpellAmplification();
            foreach (var target in targets)
            {
                var reduction = Ability.GetDamageReduction(target, DamageType);
                var manaBurn = Math.Min(target.Mana, damage) * damagePercentage;
                totalDamage += DamageHelpers.GetSpellDamage(manaBurn, amplify, reduction);
            }

            return totalDamage;
        }

        public override bool UseAbility(Vector3 position)
        {
            return Invoke() && base.UseAbility(position) && _invokeHelper.Casted();
        }

        public override bool UseAbility(Unit target)
        {
            return Invoke() && base.UseAbility(target) && _invokeHelper.Casted();
        }
    }
}