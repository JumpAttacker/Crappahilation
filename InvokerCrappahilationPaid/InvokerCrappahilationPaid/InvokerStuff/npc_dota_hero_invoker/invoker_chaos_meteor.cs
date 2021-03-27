// <copyright file="invoker_chaos_meteor.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;
using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Components;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using SharpDX;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerChaosMeteor : LineAbility, IInvokableAbility, IHasDot, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<InvokerChaosMeteor> _invokeHelper;

        public InvokerChaosMeteor(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerChaosMeteor>(this);
        }

        public override float ActivationDelay => Ability.GetAbilitySpecialData("land_time");

        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);

        public override float Radius => Ability.GetAbilitySpecialData("area_of_effect");

        public override float Range => Ability.GetAbilitySpecialData("travel_distance", _invokeHelper.Wex.Level);

        public override float Speed => Ability.GetAbilitySpecialData("travel_speed");

        protected override float RawDamage => Ability.GetAbilitySpecialDataWithTalent(Owner, "main_damage") * 2;

        public float DamageDuration => Ability.GetAbilitySpecialData("burn_duration");

        public bool HasInitialDamage { get; } = true;

        public float RawTickDamage => Ability.GetAbilitySpecialData("burn_dps");

        public string TargetModifierName { get; } = "modifier_invoker_chaos_meteor_burn";

        public float TickRate => Ability.GetAbilitySpecialData("damage_interval");

        public float GetTickDamage(params Unit[] targets)
        {
            var totalDamage = 0.0f;

            var damage = RawTickDamage;
            var amplify = Owner.GetSpellAmplification();
            foreach (var target in targets)
            {
                var reduction = Ability.GetDamageReduction(target, DamageType);
                totalDamage += DamageHelpers.GetSpellDamage(damage, amplify, reduction);
            }

            return totalDamage;
        }

        public float GetTotalDamage(params Unit[] targets)
        {
            return GetDamage(targets) + GetTickDamage(targets) * (DamageDuration / TickRate);
        }

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
            {AbilityId.invoker_exort, AbilityId.invoker_exort, AbilityId.invoker_wex};

        public bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        public override bool UseAbility(Unit target)
        {
            return Invoke() && base.UseAbility(target) && _invokeHelper.Casted();
        }

        public override bool UseAbility(Vector3 position)
        {
            return Invoke() && base.UseAbility(position) && _invokeHelper.Casted();
        }
    }
}