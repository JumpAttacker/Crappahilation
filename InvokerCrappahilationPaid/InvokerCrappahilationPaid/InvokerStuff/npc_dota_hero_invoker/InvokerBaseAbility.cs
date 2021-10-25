using System.Collections.Generic;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Input;
using Divine.Numerics;
using InvokerCrappahilationPaid.Extensions;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Units;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public abstract class InvokerBaseAbility : InvokerSimpleBaseAbility, IInvokableAbility, IHaveFastInvokeKey
    {
        protected InvokerBaseAbility(ActiveAbility activeAbility) : base(activeAbility)
        {
        }

        public abstract AbilityId[] RequiredOrbs { get; }
        public abstract bool IsInvoked { get; }
        public abstract bool CanBeInvoked { get; }
        public abstract bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false);
        public abstract Key Key { get; set; }

        public bool CanBeCasted()
        {
            return BaseAbility.CanBeCasted();
        }

        public override bool UseAbility()
        {
            if (!IsInvoked)
            {
                if (CanBeCasted())
                {
                    Invoke();
                }
                else
                {
                    return false;
                }
            }

            return BaseAbility.BaseAbility.Cast();
        }

        public override bool UseAbility(Vector3 pos)
        {
            if (!IsInvoked)
            {
                if (CanBeCasted())
                {
                    Invoke();
                }
                else
                {
                    return false;
                }
            }

            return BaseAbility.BaseAbility.Cast(pos);
        }

        public override bool UseAbility(Unit9 target)
        {
            if (!IsInvoked)
            {
                if (CanBeCasted())
                {
                    Invoke();
                }
                else
                {
                    return false;
                }
            }

            return BaseAbility.BaseAbility.Cast(target);
        }
    }

    public abstract class InvokerSimpleBaseAbility : IHaveActiveAbility<ActiveAbility>
    {
        public InvokerSimpleBaseAbility(ActiveAbility activeAbility)
        {
            BaseAbility = activeAbility;
        }

        public ActiveAbility BaseAbility { get; set; }

        public uint Level => BaseAbility.Level;
        public bool IsReady => BaseAbility.IsReady;
        public AbilityId Id => BaseAbility.Id;
        public AbilityState AbilityState => BaseAbility.BaseAbility.AbilityState;
        public AbilitySlot AbilitySlot => BaseAbility.AbilitySlot;
        public Unit9 Owner => BaseAbility.Owner;

        public virtual bool UseAbility()
        {
            return BaseAbility.BaseAbility.Cast();
        }

        public virtual bool UseAbility(Vector3 pos)
        {
            return BaseAbility.BaseAbility.Cast(pos);
        }

        public virtual bool UseAbility(Unit9 target)
        {
            return BaseAbility.BaseAbility.Cast(target);
        }
    }
}