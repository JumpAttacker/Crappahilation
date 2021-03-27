using Ensage;
using SharpDX;

namespace TechiesCrappahilationPaid.BombsType.DrawBehaviour
{
    public class CanDrawRangeWithTimer : DrawBombRangeHelper
    {
        public CanDrawRangeWithTimer(Unit me) : base(me)
        {
        }

        public CanDrawRangeWithTimer(Unit me, float range, Color clr) : base(me, range, clr)
        {
            Particle.DrawRange(Me, $"{Me.Handle}Range", range, clr);
        }

        public override void Draw(float range, Color clr)
        {
            Particle.DrawRange(Me, $"{Me.Handle}Range", range, clr);
        }
    }
}