using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Divine.Extensions;
using Divine.Numerics;
using Divine.Particle;
using Divine.Update;
using Divine.Entity.Entities.Units;
using TechiesCrappahilationPaid.BombsType.DrawBehaviour;

namespace TechiesCrappahilationPaid.BombsType
{
    public class RemoteMine : BombBase
    {
        public bool HasAghBuff;

        public RemoteMine(Unit owner) : base(owner)
        {
            Range = 425;
            RangeSystem = new CanDrawRange(Owner, Range, Color.Gray);
            Stacker = new Stacker();

            //TODO: delete after update
            // UpdateManager.BeginInvoke(500, async () =>
            // {
            //     try
            //     {
            //         while (owner != null && owner.IsValid && owner.Health <= 1)
            //         {
            //             await Task.Delay(100);
            //         }
            //
            //         if (owner == null || !owner.IsValid)
            //             return;
            //
            //         IsActive = true;
            //         DisposeSpawnRange();
            //         var isVisible = Owner.IsVisibleToEnemies;
            //         ChangeDrawType(true,
            //             isVisible ? Color.Red : Color.White);
            //     }
            //     catch (Exception e)
            //     {
            //         Console.WriteLine(e);
            //     }
            // });
        }

        public Stacker Stacker;
        public RemoteMine StackerMain;

        public RemoteMine UpdateStacker(IEnumerable<RemoteMine> bombs)
        {
            var closest =
                bombs.Where(
                        x =>
                            !x.Equals(this) && x.Stacker.IsActive &&
                            x.Position.Distance2D(Position) <= 200)
                    .OrderBy(y => y.Position.Distance2D(Position))
                    .FirstOrDefault();
            if (closest != null)
            {
                closest.Stacker.Counter++;
                StackerMain = closest;
            }
            else
            {
                Stacker.Counter++;
            }

            return this;
        }

        public void UnStacker(List<RemoteMine> bombs)
        {
            var closest =
                bombs.Where(
                        x =>
                            !x.Equals(this) && x.Stacker.IsActive &&
                            x.Position.Distance2D(Position) <= 200)
                    .OrderBy(y => y.Position.Distance2D(Position))
                    .FirstOrDefault();
            if (closest != null)
            {
                closest.Stacker.Counter--;
                StackerMain = null;
            }
            else if (Stacker.IsActive)
            {
                Stacker.Counter--;
                if (Stacker.IsActive)
                {
                    closest =
                        bombs.Where(
                                x =>
                                    !x.Equals(this) && !x.Stacker.IsActive &&
                                    x.Position.Distance2D(Position) <= 200)
                            .OrderBy(y => y.Position.Distance2D(Position))
                            .FirstOrDefault();
                    if (closest != null)
                    {
                        RefreshStacker(bombs);
                    }
                }
            }
        }

        private void RefreshStacker(List<RemoteMine> bombs)
        {
            foreach (
                var bomb in
                bombs.Where(
                    manager =>
                        !manager.Equals(this) /*&&
                            manager.BombPosition.Distance2D(BombPosition) <= StackerRange*2 + 50*/))
            {
                bomb.Stacker.Counter = 0;
                bomb.InitNewStacker(bomb, bombs);
            }
        }

        public void InitNewStacker(RemoteMine target, List<RemoteMine> bombs)
        {
            var closest =
                bombs.Where(
                        x =>
                            !x.Equals(this) && x.Stacker.IsActive &&
                            x.Position.Distance2D(target.Position) <= 200)
                    .OrderBy(y => y.Position.Distance2D(target.Position))
                    .FirstOrDefault();
            if (closest != null)
            {
                closest.Stacker.Counter++;
            }
            else
            {
                Stacker.Counter++;
            }
        }

        public bool UnderTrueSight { get; set; }

        public void DrawSpawnRange()
        {
            // ParticleManager.RangeParticle(Owner.Handle.ToString(), Owner, 425, Color.DimGray);
            ParticleManager.CircleParticle(Owner.Handle.ToString(), Owner.Position, 425, Color.DimGray);
            // RangeEffect = new ParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf", Owner.Position);
            // RangeEffect.SetControlPoint(1, new Vector3(Range, 255, 0));
            // RangeEffect.SetControlPoint(2, new Vector3(100, 100, 100));
        }

        public void DisposeSpawnRange()
        {
            ParticleManager.RemoveParticle(Owner.Handle.ToString());
        }
    }
}