using System.ComponentModel.Composition;
<<<<<<< HEAD

using Divine;

using InvokerCrappahilationPaid.Features;
=======
using Divine;
using InvokerCrappahilationPaid.Features;
using O9K.Core.Entities.Heroes;
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893

namespace InvokerCrappahilationPaid
{
    public sealed class InvokerCrappahilationPaid : Bootstrapper
    {
        [ImportingConstructor]
        public InvokerCrappahilationPaid()
        {
        }

        public Config Config { get; private set; }
        public Combo Combo { get; private set; }
        public Updater Updater { get; private set; }
        public AbilitiesInCombo AbilitiesInCombo { get; private set; }

        public Hero9 Me { get; set; }
        // public NotificationHelper NotificationHelper { get; private set; }
        public NavMeshHelper NavMeshHelper { get; private set; }

        protected override void OnActivate()
        {
            Me = new Hero9(EntityManager.LocalHero);
            if (Me == null || Me.Id != HeroId.npc_dota_hero_invoker)
            {
                return;
            }

            AbilitiesInCombo = new AbilitiesInCombo(this);

            Config = new Config(this);

            Updater = new Updater(this);

            Combo = new Combo(this);

            // NotificationHelper = new NotificationHelper(this);

            NavMeshHelper = new NavMeshHelper();

            //var test=new DivineSuccess();

        }
    }
}