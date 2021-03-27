using System;
using Ensage;
using Ensage.SDK.Renderer;
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

        private static IRenderManager Renderer => TechiesCrappahilationPaid.Renderer;

        public void StartTimer()
        {
            _startTime = Game.RawGameTime;
            Renderer.Draw += RendererOnDraw;
        }

        private void RendererOnDraw(IRenderer renderer)
        {
            try
            {
                if (Drawing.WorldToScreen(Owner.Position, out var pos))
                    renderer.DrawText(pos, $"{1.601f - (Game.RawGameTime - _startTime):0.00}",
                        System.Drawing.Color.White,
                        25);
            }
            catch (Exception)
            {
                StopTimer();
                return;
            }

            if (1.601f - (Game.RawGameTime - _startTime) <= 0) StopTimer();
        }

        public void StopTimer()
        {
            Renderer.Draw -= RendererOnDraw;
        }
    }
}