using Divine.Numerics;
using Divine.Particle;
using Divine.Entity.Entities.Units;

namespace TechiesCrappahilationPaid.BombsType.DrawBehaviour
{
    public class CanDrawRangeWithTimer : DrawBombRangeHelper
    {
        public CanDrawRangeWithTimer(Unit me) : base(me)
        {
        }

        public CanDrawRangeWithTimer(Unit me, float range, Color clr) : base(me, range, clr)
        {
            ParticleManager.CreateRangeParticle($"{Me.Handle}Range", Me, range, clr);
        }

        public override void Draw(float range, Color clr)
        {
            ParticleManager.CreateRangeParticle($"{Me.Handle}Range", Me, range, clr);
        }
    }
}