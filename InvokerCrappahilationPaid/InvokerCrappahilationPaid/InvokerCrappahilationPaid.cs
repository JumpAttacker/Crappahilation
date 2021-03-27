using System;
using System.ComponentModel.Composition;
using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using InvokerCrappahilationPaid.Features;
using NLog;
using PlaySharp.Sentry;
using PlaySharp.Sentry.Data;

namespace InvokerCrappahilationPaid
{
    [ExportPlugin(
        mode: StartupMode.Auto,
        name: "InvokerCrappahilationPaid",
        units: new[] {HeroId.npc_dota_hero_invoker})]
    public sealed class InvokerCrappahilationPaid : Plugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private SentryClient _client;

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

//            Log.Debug($"Init TextureHelper");
//            TextureHelper.Init(Context);

            Log.Debug("Load abilities");
            AbilitiesInCombo = new AbilitiesInCombo(this);

            Log.Debug("Load config");
            Config = new Config(this);

            Log.Debug("Load updater");
            Updater = new Updater(this);

            Log.Debug("Load combo");
            Combo = new Combo(this);

            Log.Debug("Load Notification Helper");
            NotificationHelper = new NotificationHelper(this);

            Log.Debug("Load NavMeshHelper");
            NavMeshHelper = new NavMeshHelper(this);

            Log.Warn(AbilitiesInCombo.Tornado.Duration);
            //var test=new DivineSuccess();
            
            _client = new SentryClient(
                "https://6b8fedb4d4b942949c4d2a3ed019873f:78d8171a47df490d9d85f7f806b9095b@sentry.io/1545139");
//            _client.Client.Environment = "info";

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Console.WriteLine(args);
                var ex = (Exception) args.ExceptionObject;
                _client.CaptureAsync(ex);
            };
            if (Game.GameMode != GameMode.Demo)
                _client.Capture(new SentryEvent("invoker loaded")); 
            
        }
    }
}