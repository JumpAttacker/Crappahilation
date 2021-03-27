using System;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.SDK.Renderer;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using NLog;
using PlaySharp.Sentry;
using PlaySharp.Sentry.Data;
using TechiesCrappahilationPaid.Abilities;
using TechiesCrappahilationPaid.Features;
using TechiesCrappahilationPaid.Features.ViewDamageFromBombs;
using TechiesCrappahilationPaid.Helpers;
using TechiesCrappahilationPaid.Managers;

namespace TechiesCrappahilationPaid
{
    [ExportPlugin(
        mode: StartupMode.Auto,
        name: "TechiesCrappahilationPaid",
        units: new[] {HeroId.npc_dota_hero_techies})]
    public sealed class TechiesCrappahilationPaid : Plugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static IRenderManager Renderer;
        private ViewManager _viewManager;

        private SentryClient _client;
//        private static readonly Dictionary<int, SentryClient> sentryClients = new Dictionary<int, SentryClient>();

        [ImportingConstructor]
        public TechiesCrappahilationPaid([Import] IServiceContext context)
        {
            Context = context;
            //var assembly = Assembly.GetCallingAssembly();
            //var id = assembly.GetHashCode();

            //if (!sentryClients.TryGetValue(id, out var sentryClient))
            //{
            //    var metadata = assembly.GetMetadata();

            //    sentryClient = new SentryClient(metadata.SentryProject);
            //    sentryClient.Client.Compression = true;
            //    sentryClient.Client.Release = metadata.Commit;
            //    sentryClient.Client.Environment = metadata.Channel;
            //    sentryClient.Client.Logger = assembly.GetName().Name;

            //    sentryClient.Tags["Id"] = () => metadata.Id;
            //    sentryClient.Tags["Channel"] = () => metadata.Channel;
            //    sentryClient.Tags["Version"] = () => metadata.Version;
            //    sentryClient.Tags["Build"] = () => metadata.Build;
            //    sentryClient.Tags["Commit"] = () => metadata.Commit;

            //    sentryClients[id] = sentryClient;

            //    var message = "message";
            //    var sentryMessage = new SentryMessage(message);
            //    var SentryEvent = new SentryEvent(sentryMessage);
            //    sentryClient.CaptureAsync(SentryEvent);


            //}
        }


        public MenuManager MenuManager { get; private set; }
        public SuicideDamage SuicideDamage { get; private set; }
        public SuicideAbility Suicide { get; private set; }
        public LandMineAbility LandMine { get; private set; }
        public StasisMineAbility StasisMine { get; private set; }
        public RemoteMineAbility RemoteMine { get; private set; }
        public Hero Me { get; set; }
        public IServiceContext Context { get; }
        public Updater Updater { get; private set; }

        public static IParticleManager ParticleManager { get; set; }

        protected override void OnActivate()
        {
            //var ravenClient = new RavenClient("https://6b8fedb4d4b942949c4d2a3ed019873f:78d8171a47df490d9d85f7f806b9095b@sentry.io/1545139");
            //ravenClient.Capture(new SentryEvent("Hello World!"));
            _client = new SentryClient(
                "https://6b8fedb4d4b942949c4d2a3ed019873f:78d8171a47df490d9d85f7f806b9095b@sentry.io/1545139");
//            _client.Client.Environment = "info";

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Console.WriteLine(args);
                var ex = (Exception) args.ExceptionObject;
                _client.CaptureAsync(ex);
            };


            Me = Context.Owner as Hero;

            if (Context.Owner == null || Me == null)
            {
                Log.Error("Owner is not a hero");
                return;
            }
            

            LandMine = new LandMineAbility(Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_land_mines));
            StasisMine = new StasisMineAbility(Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_stasis_trap));
            Suicide = new SuicideAbility(Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_suicide));
            RemoteMine = new RemoteMineAbility(Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_remote_mines));
            FocusedDetonate = Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_focused_detonate);

            ParticleManager = Context.Particle;
            Renderer = Context.RenderManager;
            MenuManager = new MenuManager(this);
            Updater = new Updater(this);
            TargetManager.Init(this);
            TextureHelper.Init(Context);
            var stackInfo = new StackInfo(this);
            SuicideDamage = new SuicideDamage(this);
            _viewManager = new ViewManager(this);
            AutoPlanter.Init(this);
            var plantHelper = new PlantHelper(this);
            _client.CaptureAsync(new SentryEvent("Successful init"));
//            Game.OnWndProc += GameOnWndProc;
        }

        public Ability FocusedDetonate { get; set; }

        private const uint WM_LBUTTONDOWN = 0x0201;

        private static void GameOnWndProc(WndEventArgs args)
        {
            if (args.Msg != WM_LBUTTONDOWN) return;
            using (var sw = File.AppendText(@"E:\test.txt"))
            {
                var pos = Game.MousePosition.ToCopyFormat();
                sw.WriteLine($", new Vector3({pos})");
            }
        }
    }
}