// <copyright file="invoker_ice_wall.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Components;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using SharpDX;
using Vector3Extensions = Ensage.Common.Extensions.SharpDX.Vector3Extensions;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerIceWall : ActiveAbility, IInvokableAbility, IHasTargetModifier, IHaveFastInvokeKey
    {
        private readonly InvokeHelper<InvokerIceWall> _invokeHelper;
        public bool InAction;

        public InvokerIceWall(Ability ability)
            : base(ability)
        {
            _invokeHelper = new InvokeHelper<InvokerIceWall>(this);
        }


        public override bool CanBeCasted => base.CanBeCasted && _invokeHelper.CanInvoke(!IsInvoked);

        public override float Duration => Ability.GetAbilitySpecialData("duration", _invokeHelper.Quas.Level);

        public string TargetModifierName { get; } = "modifier_invoker_ice_wall_slow_debuff";

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
            {AbilityId.invoker_quas, AbilityId.invoker_quas, AbilityId.invoker_exort};

        public bool Invoke(List<AbilityId> currentOrbs = null, bool skip = false)
        {
            return _invokeHelper.Invoke(currentOrbs, skip);
        }

        private void PlayerOnOnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (args.IsPlayerInput) args.Process = false;
        }

        public override bool UseAbility()
        {
            return Invoke() && base.UseAbility() && _invokeHelper.Casted();
        }

        public async Task<bool> CastAsync(Unit enemy)
        {
            InAction = true;
            InvokerCrappahilationPaid.Log.Warn("[Async] IceWall Start");
            if (!Invoke())
            {
                InvokerCrappahilationPaid.Log.Warn("[Async] IceWall cant invoke");
                InAction = false;
                return false;
            }

            Player.OnExecuteOrder += PlayerOnOnExecuteOrder;
            Owner.Stop();
            await Task.Delay((int) (Game.Ping + 150));
            Vector3 pos;
            float num1;
            if (!enemy.IsMoving || enemy.HasAnyModifiers("modifier_invoker_deafening_blast_knockback"))
            {
                num1 = Owner.TurnTime(enemy.NetworkPosition) + 0.1f;
                Owner.Move(enemy.NetworkPosition);
                pos = enemy.NetworkPosition;
            }
            else
            {
                pos = enemy.InFront(enemy.MovementSpeed * 0.6f);
                num1 = Owner.TurnTime(pos) + 0.1f;
                Owner.Move(pos);
            }

            if (num1 > 0.0) await Task.Delay((int) (num1 * 1000.0));
            Owner.Stop();
            var delay = Game.Ping > 1.0 ? Game.Ping / 1000f : Game.Ping;
            await Task.Delay((int) (delay * 1000.0));
            var num2 = 220f / Vector3Extensions.Distance(pos, Owner.NetworkPosition);
            var num3 = Owner.NetworkRotationRad - (float) Math.Acos(num2);
            var position = new Vector3(Owner.NetworkPosition.X + (float) (Math.Cos(num3) * 10.0),
                Owner.NetworkPosition.Y + (float) (Math.Sin(num3) * 10.0), Owner.NetworkPosition.Z);
            var num4 = Owner.TurnTime(position) + 0.1f;
            Owner.MoveToDirection(position);
            if (num4 > 0.0) await Task.Delay((int) (num4 * 1000.0));
            Owner.Stop();
            Player.OnExecuteOrder -= PlayerOnOnExecuteOrder;

            InvokerCrappahilationPaid.Log.Warn("[Async] IceWall End (phase 1)");
            UpdateManager.BeginInvoke(() =>
            {
                InvokerCrappahilationPaid.Log.Warn("[Async] IceWall End (phase 2)");
                InAction = false;
            }, (int) (Game.Ping * 3f));
            return UseAbility();
            //await Task.Delay((int)(100.0 + delay));
        }
    }
}