using Ensage;
using SharpDX;

namespace TechiesCrappahilationPaid.BombsType.DrawBehaviour
{
    public class CantDrawRange : DrawBombRangeHelper
    {
        public CantDrawRange(Unit me) : base(me)
        {
            Particle.Remove($"{Me.Handle}Range");
        }

        public CantDrawRange(Unit me, float range, Color clr) : base(me, range, clr)
        {
        }
    }
}