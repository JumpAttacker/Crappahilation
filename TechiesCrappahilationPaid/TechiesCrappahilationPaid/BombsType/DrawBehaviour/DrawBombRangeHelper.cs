using Ensage;
using Ensage.SDK.Renderer.Particle;
using SharpDX;

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

        protected IParticleManager Particle => TechiesCrappahilationPaid.ParticleManager;

        public virtual void Draw(float range, Color clr)
        {
        }
    }
}