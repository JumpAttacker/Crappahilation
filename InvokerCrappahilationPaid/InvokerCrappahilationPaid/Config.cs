using Divine.Menu.Items;
using InvokerCrappahilationPaid.Features;

namespace InvokerCrappahilationPaid
{
    public class Config
    {
        public InvokerCrappahilationPaid Main;

        public Config(InvokerCrappahilationPaid main)
        {
            Main = main;
            Factory = Divine.Menu.MenuManager.CreateRootMenu("Invoker Crappahilation");
            ComboKey = Factory.CreateHoldKey("Combo key");
            RefresherBehavior = Factory.CreateSelector("Refresher Behavior",
                new[] {"After Meteor+Blast", "When 95% of abilities are on cd", "In both cases"});
            PrepareKey = Factory.CreateHoldKey("Prepare key");
            AutoPrepare = Factory.CreateSwitcher("Auto prepare");
            UseForges = Factory.CreateSwitcher("Use forges in Combo");
            // UseNecros = Factory.CreateSwitcher("Use necros (and archer's purge) in Combo");
            // AutoPurge = Factory.CreateSwitcher("Use necros's purge not in Combo", false);
            UseEul = Factory.CreateSwitcher("Use eul in Dynamic Combo");
            UseIceWall = Factory.CreateSwitcher("Use IceWall in Dynamic Combo");
            BackToDynamicCombo = Factory.CreateSwitcher("Back to dynamic combo after custom combo");
            UseCataclysm = Factory.CreateSlider("Min targets in eul/tornado for cataclysm", 1, 1, 5);
            //ComboType = new ComboType(this);
            AbilityPanel = new AbilityPanel(this);
            AutoSunStrike = new AutoSunStrike(this);
            SmartSphere = new SmartSphere(this);
            AutoGhostWalk = new AutoGhostWalk(this);
            FastInvoke = new InvokeHelper(this);
            ComboPanel = new ComboPanel(this);
            Prepare = new Prepare(Main);
        }

        public MenuSwitcher BackToDynamicCombo { get; set; }

        public MenuSlider UseCataclysm { get; set; }

        public MenuSwitcher UseIceWall { get; set; }

        public MenuSwitcher UseEul { get; set; }

        // public MenuSwitcher UseNecros { get; set; }

        // public MenuSwitcher AutoPurge { get; set; }

        public MenuSwitcher UseForges { get; set; }

        public MenuHoldKey PrepareKey { get; set; }
        public MenuSwitcher AutoPrepare { get; }
        public MenuSelector RefresherBehavior { get; set; }

        public MenuHoldKey ComboKey { get; set; }

        public RootMenu Factory { get; set; }


        //public ComboType ComboType { get; }
        public AbilityPanel AbilityPanel { get; }
        public AutoSunStrike AutoSunStrike { get; }
        public SmartSphere SmartSphere { get; }
        public AutoGhostWalk AutoGhostWalk { get; }
        public InvokeHelper FastInvoke { get; }
        public ComboPanel ComboPanel { get; }
        public Prepare Prepare { get; }
    }
}