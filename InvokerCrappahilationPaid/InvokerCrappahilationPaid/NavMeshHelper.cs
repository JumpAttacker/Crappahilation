using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Divine;
using Divine.SDK.Extensions;
using Divine.SDK.Prediction;
using Divine.SDK.Prediction.Collision;
using SharpDX;

namespace InvokerCrappahilationPaid
{
    public class NavMeshHelper
    {
        public NavMeshHelper()
        {
        }


        private void AddUnitToSystem(Hero target)
        {
            var localHero = EntityManager.LocalHero;
            if (localHero == null)
            {
                return;
            }

            if (target == null)
            {
                return;
            }

            var sunStrike = localHero.Spellbook.MainSpells.FirstOrDefault(x => x.Id == AbilityId.invoker_sun_strike);

            var input = new PredictionInput
            {
                Owner = localHero,
                AreaOfEffect = false,
                CollisionTypes = CollisionTypes.None,
                Delay = sunStrike.CastPoint + sunStrike.GetAbilitySpecialData("delay"),
                Speed = float.MaxValue,
                Range = float.MaxValue,
                Radius = sunStrike.GetAbilitySpecialData("area_of_effect"),
                PredictionSkillshotType = PredictionSkillshotType.SkillshotCircle
            };

            input = input.WithTarget(target);

            var hookOutput = PredictionManager.GetPrediction(input);

            ParticleManager.CircleParticle("CircleParticle1", hookOutput.CastPosition, input.Radius, Color.Red);
        }
    }
}