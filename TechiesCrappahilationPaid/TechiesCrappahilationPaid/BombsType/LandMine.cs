using System;
using Divine;
using Divine.Zero.Loader;
using SharpDX;
using TechiesCrappahilationPaid.BombsType.BombBehaviour;
using TechiesCrappahilationPaid.BombsType.DrawBehaviour;

namespace TechiesCrappahilationPaid.BombsType
{
    public class LandMine : BombBase
    {
        private float _startTime;

        public LandMine(Unit owner) : base(owner)
        {
            Range = 400f;
            RangeSystem = new CanDrawRange(owner, Range, Color.Gray);
        }

        public LandMine(Unit owner, IDetonateType detonateType) : base(owner, detonateType)
        {
        }


        public void StartTimer()
        {
            _startTime = GameManager.RawGameTime;
            RendererManager.Draw += RendererOnDraw;
        }

        private void RendererOnDraw()
        {
            try
            {
                var pos = RendererManager.WorldToScreen(Owner.Position);
                if (pos.IsZero)
                    return;
                RendererManager.DrawText($"{1.601f - (GameManager.RawGameTime - _startTime):0.00}", pos, Color.White,
                    25);
            }
            catch (Exception)
            {
                StopTimer();
                return;
            }

            if (1.601f - (GameManager.RawGameTime - _startTime) <= 0) StopTimer();
        }

        public void StopTimer()
        {
            RendererManager.Draw -= RendererOnDraw;
        }
    }
}