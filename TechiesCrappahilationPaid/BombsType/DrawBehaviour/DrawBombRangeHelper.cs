using Divine.Numerics;
using Divine.Entity.Entities.Units;

namespace TechiesCrappahilationPaid.BombsType.DrawBehaviour
{
    public abstract class DrawBombRangeHelper : IRangeSystem
    {
        protected readonly Unit Me;

        protected DrawBombRangeHelper(Unit me)
        {
            Me = me;
        }

        protected DrawBombRangeHelper(Unit me, float range, Color clr)
        {
            Me = me;
        }

        // protected ParticleManager Particle => ParticleManager.;

        public virtual void Draw(float range, Color clr)
        {
        }
    }
}