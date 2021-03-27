using Ensage;

namespace TechiesCrappahilationPaid.BombsType.BombBehaviour
{
    public class CanDetonate : IDetonateType
    {
        public Ability DetonateAbility;

        public CanDetonate(Ability detonateAbility)
        {
            DetonateAbility = detonateAbility;
        }

        public void Detonate()
        {
            DetonateAbility.UseAbility();
        }
    }
}