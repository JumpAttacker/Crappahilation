using System;
using Divine;
using Divine.Entity;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Entity.Entities.Units.Heroes.Components;
using Divine.Order;
using Divine.Service;
using InvokerCrappahilationPaid.Features;
using O9K.Core.Entities.Heroes;
using O9K.Core.Managers.Entity;

namespace InvokerCrappahilationPaid
{
    public sealed class InvokerCrappahilationPaid : Bootstrapper
    {
        public Config Config { get; private set; }
        public Combo Combo { get; private set; }
        public Updater Updater { get; private set; }
        public AbilitiesInCombo AbilitiesInCombo { get; private set; }

        public Hero Me { get; set; }
        public Hero9 Me9 => EntityManager9.GetUnit(Me.Handle) as Hero9;
        public NotificationHelper NotificationHelper { get; private set; }

        protected override void OnActivate()
        {
            Me = EntityManager.LocalHero;
            if (Me == null || Me.HeroId != HeroId.npc_dota_hero_invoker)
            {
                return;
            }

            AbilitiesInCombo = new AbilitiesInCombo(this);

            Config = new Config(this);

            Updater = new Updater(this);

            Combo = new Combo(this);

            NotificationHelper = new NotificationHelper(this);

            OrderManager.OrderAdding += args => { Console.WriteLine(args.Order.Type); };
        }
    }
}