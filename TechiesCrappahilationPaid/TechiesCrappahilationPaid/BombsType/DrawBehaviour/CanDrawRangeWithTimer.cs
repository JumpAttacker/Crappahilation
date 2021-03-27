using Divine;

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
            ParticleManager.RangeParticle($"{Me.Handle}Range", Me, range, clr);
        }

        public override void Draw(float range, Color clr)
        {
            ParticleManager.RangeParticle($"{Me.Handle}Range", Me, range, clr);
        }
    }
}