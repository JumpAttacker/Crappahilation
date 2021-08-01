using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Divine.Entity.Entities.Abilities;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units;
using Divine.Extensions;
using Divine.Game;
using Divine.Numerics;
using Divine.Order;
using Divine.Order.EventArgs;
using Divine.Update;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Abilities.Heroes.Invoker;
using O9K.Core.Entities.Heroes;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerIceWall : InvokerBaseAbility
    {
        private readonly InvokeHelper<IceWall> _invokeHelper;
        public static bool InAction;

        public InvokerIceWall(IceWall ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<IceWall>(ability);
        }

        public string TargetModifierName { get; } = "modifier_invoker_ice_wall_slow_debuff";

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
            {AbilityId.invoker_quas, AbilityId.invoker_quas, AbilityId.invoker_exort};

        public override bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        private void PlayerOnOnExecuteOrder(OrderAddingEventArgs args)
        {
            if (!args.IsCustom) args.Process = false;
        }

        // public override bool UseAbility()
        // {
        //     return Invoke() && base.UseAbility() && _invokeHelper.Casted();
        // }

        public async Task<bool> CastAsync(Unit enemy)
        {
            if (InAction)
                return false;
            InAction = true;
            if (!Invoke())
            {
                InAction = false;
                return false;
            }

            OrderManager.OrderAdding += PlayerOnOnExecuteOrder;
            BaseAbility.Owner.Stop();
            await Task.Delay((int) (GameManager.Ping + 500));
            Vector3 pos;
            float num1;
            if (!enemy.IsMoving || enemy.HasAnyModifiers("modifier_invoker_deafening_blast_knockback"))
            {
                num1 = BaseAbility.Owner.GetTurnTime(enemy.Position) + 0.1f;
                BaseAbility.Owner.BaseUnit.MoveToDirection(enemy.Position);
                pos = enemy.Position;
            }
            else
            {
                pos = enemy.InFront(enemy.MovementSpeed * 0.6f);
                num1 = BaseAbility.Owner.GetTurnTime(pos) + 0.1f;
                BaseAbility.Owner.BaseUnit.MoveToDirection(pos);
            }
            
            if (num1 > 0.0) await Task.Delay((int) (num1 + 150.0));
            BaseAbility.Owner.Stop();
            var delay = GameManager.Ping;
            await Task.Delay((int) (delay)+150);
            var num2 = 220f / pos.Distance(BaseAbility.Owner.Position);
            var num3 = BaseAbility.Owner.BaseUnit.NetworkRotationRad - (float) Math.Acos(num2);
            var position = new Vector3(BaseAbility.Owner.Position.X + (float) (Math.Cos(num3) * 10.0),
                BaseAbility.Owner.Position.Y + (float) (Math.Sin(num3) * 10.0), BaseAbility.Owner.Position.Z);
            var num4 = BaseAbility.Owner.GetTurnTime(position) + 0.1f;
            BaseAbility.Owner.BaseUnit.MoveToDirection(position);
            if (num4 > 0.0) await Task.Delay((int) (num4 + 200.0));
            OrderManager.OrderAdding -= PlayerOnOnExecuteOrder;
            UpdateManager.BeginInvoke((int) (GameManager.Ping * 3f + BaseAbility.CastPoint * 1000), () =>
            {
                InAction = false;
            });
            return BaseAbility.BaseAbility.Cast();
            //await Task.Delay((int)(100.0 + delay));
        }
    }
}