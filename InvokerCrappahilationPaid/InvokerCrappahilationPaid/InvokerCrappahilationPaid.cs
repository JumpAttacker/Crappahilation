using System;
using System.ComponentModel.Composition;
using Divine;

using Ensage.SDK.Abilities;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using InvokerCrappahilationPaid.Features;
using NLog;
using PlaySharp.Sentry;
using PlaySharp.Sentry.Data;

namespace InvokerCrappahilationPaid
{
    public sealed class InvokerCrappahilationPaid : Bootstrapper
    {
        [ImportingConstructor]
        public InvokerCrappahilationPaid([Import] IServiceContext context)
        {
            Context = context;
        }

        public static AbilityFactory AbilityFacory { get; set; }

        public IServiceContext Context { get; }
        public Config Config { get; private set; }
        public Combo Combo { get; private set; }
        public Updater Updater { get; private set; }
        public AbilitiesInCombo AbilitiesInCombo { get; private set; }

        public Hero Me { get; set; }
        public NotificationHelper NotificationHelper { get; private set; }
        public NavMeshHelper NavMeshHelper { get; private set; }

        protected override void OnActivate()
        {
            Me = Context.Owner as Hero;
            AbilityFacory = Context.AbilityFactory;

            AbilitiesInCombo = new AbilitiesInCombo(this);

            Config = new Config(this);

            Updater = new Updater(this);

            Combo = new Combo(this);

            NotificationHelper = new NotificationHelper(this);

            NavMeshHelper = new NavMeshHelper(this);

            //var test=new DivineSuccess();

        }
    }
}