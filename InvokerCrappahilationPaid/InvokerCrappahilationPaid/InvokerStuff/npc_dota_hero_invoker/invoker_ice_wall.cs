using System;
using System.Threading.Tasks;
using System.Windows.Input;
<<<<<<< HEAD

using SharpDX;

using Vector3Extensions = Ensage.Common.Extensions.SharpDX.Vector3Extensions;
=======
using Divine;
using Divine.SDK.Extensions;
using O9K.Core.Entities.Abilities.Heroes.Invoker;
using SharpDX;
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public class InvokerIceWall : IceWall, IHaveFastInvokeKey
    {
        public bool InAction;

        public InvokerIceWall(Ability ability)
            : base(ability)
        {
        }

        public Key Key { get; set; }

        
        private void PlayerOnOnExecuteOrder(OrderAddingEventArgs args)
        {
            if (!args.IsCustom) args.Process = false;
        }
        
        public async Task<bool> CastAsync(Unit enemy)
        {
            InAction = true;
            if (!Invoke())
            {
                InAction = false;
                return false;
            }

            OrderManager.OrderAdding += PlayerOnOnExecuteOrder;
            Owner.Stop();
            await Task.Delay((int) (GameManager.Ping + 150));
            Vector3 pos;
            float num1;
            if (!enemy.IsMoving || enemy.HasAnyModifiers("modifier_invoker_deafening_blast_knockback"))
            {
                num1 = Owner.GetTurnTime(enemy.Position) + 0.1f;
                Owner.Move(enemy.Position);
                pos = enemy.Position;
            }
            else
            {
                pos = enemy.InFront(enemy.MovementSpeed * 0.6f);
                num1 = Owner.GetTurnTime(pos) + 0.1f;
                Owner.Move(pos);
            }
            

            if (num1 > 0.0) await Task.Delay((int) (num1 * 1000.0));
            Owner.Stop();
            var delay = GameManager.Ping > 1.0 ? GameManager.Ping / 1000f : GameManager.Ping;
            await Task.Delay((int) (delay * 1000.0));
            var num2 = 220f / pos.Distance(Owner.Position);
            var num3 = Owner.BaseUnit.NetworkRotationRad - (float) Math.Acos(num2);
            var position = new Vector3(Owner.Position.X + (float) (Math.Cos(num3) * 10.0),
                Owner.Position.Y + (float) (Math.Sin(num3) * 10.0), Owner.Position.Z);
            var num4 = Owner.GetTurnTime(position) + 0.1f;
            Owner.BaseUnit.MoveToDirection(position);
            if (num4 > 0.0) await Task.Delay((int) (num4 * 1000.0));
            Owner.Stop();
            OrderManager.OrderAdding -= PlayerOnOnExecuteOrder;

            UpdateManager.BeginInvoke((int) (GameManager.Ping * 3f), () =>
            {
                InAction = false;
            });
            return UseAbility();
        }
    }
}