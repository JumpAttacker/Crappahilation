using Divine.Numerics;
using Divine.Particle;
using Divine.Entity.Entities.Units;

namespace TechiesCrappahilationPaid.BombsType.DrawBehaviour
{
    public class CanDrawRange : DrawBombRangeHelper
    {
        public CanDrawRange(Unit me) : base(me)
        {
        }

        public CanDrawRange(Unit me, float range, Color clr) : base(me, range, clr)
        {
            ParticleManager.RangeParticle($"{Me.Handle}Range", Me, range, clr);
        }

        public override void Draw(float range, Color clr)
        {
            ParticleManager.RangeParticle($"{Me.Handle}Range", Me, range, clr);
        }
    }
}