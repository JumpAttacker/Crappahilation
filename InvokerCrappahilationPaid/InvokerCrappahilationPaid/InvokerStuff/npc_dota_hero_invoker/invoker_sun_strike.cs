// <copyright file="invoker_sun_strike.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System.Collections.Generic;
using System.Windows.Input;

using SharpDX;

using AbilityId = Ensage.AbilityId;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerSunStrike : CircleAbility, IInvokableAbility, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<InvokerSunStrike> _invokeHelper;

        public InvokerSunStrike(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerSunStrike>(this);
        }

        public override float ActivationDelay => Ability.GetAbilitySpecialData("delay");

        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);
        public override float CastRange => 9999999;

//        public bool IsCataclysmActive => Owner.GetAbilityById(AbilityId.special_bonus_unique_invoker_4)?.Level > 0;
        public bool IsCataclysmActive => (Owner as Hero).GetItemById(AbilityId.item_ultimate_scepter) != null ||
                                         Owner.HasAnyModifiers("modifier_item_ultimate_scepter_consumed");

        public override float Radius => Ability.GetAbilitySpecialData("area_of_effect");

        protected override float RawDamage => Ability.GetAbilitySpecialData("damage", _invokeHelper.Exort.Level);

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
            {AbilityId.invoker_exort, AbilityId.invoker_exort, AbilityId.invoker_exort};

        public bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        public override bool UseAbility(Unit target)
        {
            return Invoke() && base.UseAbility(target);
        }

        public override bool UseAbility()
        {
            return Invoke() && base.UseAbility() && _invokeHelper.Casted();
        }

        public override bool UseAbility(Vector3 position)
        {
            return Invoke() && base.UseAbility(position) && _invokeHelper.Casted();
        }
    }
}