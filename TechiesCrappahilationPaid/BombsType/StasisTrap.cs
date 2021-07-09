using Divine.Numerics;
using Divine.Entity.Entities.Units;
using TechiesCrappahilationPaid.BombsType.BombBehaviour;
using TechiesCrappahilationPaid.BombsType.DrawBehaviour;

namespace TechiesCrappahilationPaid.BombsType
{
    public class StasisTrap : BombBase
    {
        public StasisTrap(Unit owner, IDetonateType detonateType) : base(owner, detonateType)
        {
            Range = 400f;
            RangeSystem = new CanDrawRange(owner, Range, Color.White);
        }

        public StasisTrap(Unit owner) : base(owner)
        {
        }
    }
}