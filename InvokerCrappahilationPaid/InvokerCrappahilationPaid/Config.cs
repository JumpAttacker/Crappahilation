using InvokerCrappahilationPaid.Features;

namespace InvokerCrappahilationPaid
{
    public class Config
    {
        public InvokerCrappahilationPaid Main;

        public Config(InvokerCrappahilationPaid main)
        {
            Main = main;
            Factory = MenuFactory.Create("Invoker");
            ComboKey = Factory.Item("Combo key", new KeyBind('0'));
            RefresherBehavior = Factory.Item("Refresher Behavior",
                new StringList("After Meteor+Blast", "When 95% of abilities are on cd", "In both cases"));
            PrepareKey = Factory.Item("Prepare key", new KeyBind('0'));
            UseForges = Factory.Item("Use forges in Combo", true);
            UseNecros = Factory.Item("Use necros (and archer's purge) in Combo", true);
            AutoPurge = Factory.Item("Use necros's purge not in Combo", false);
            UseEul = Factory.Item("Use eul in Dynamic Combo", true);
            UseIceWall = Factory.Item("Use IceWall in Dynamic Combo", true);
            BackToDynamicCombo = Factory.Item("Back to dynamic combo after custom combo", true);
            UseCataclysm = Factory.Item("Min targets in eul/tornado for cataclysm", new Slider(1, 0, 5));
            //ComboType = new ComboType(this);
            AbilityPanel = new AbilityPanel(this);
            AutoSunStrike = new AutoSunStrike(this);
            SmartSphere = new SmartSphere(this);
            AutoGhostWalk = new AutoGhostWalk(this);
            FastInvoke = new InvokeHelper(this);
            ComboPanel = new ComboPanel(this);
            Prepare = new Prepare(Main);
        }

        public MenuItem<bool> UseIceWall { get; set; }

        public MenuItem<bool> BackToDynamicCombo { get; set; }

        public MenuItem<Slider> UseCataclysm { get; set; }

        public MenuItem<bool> UseEul { get; set; }
        public MenuItem<bool> AutoPurge { get; set; }

        public MenuItem<bool> UseForges { get; set; }
        public MenuItem<bool> UseNecros { get; set; }

        public MenuItem<StringList> RefresherBehavior { get; set; }

        public MenuItem<KeyBind> PrepareKey { get; set; }

        public MenuItem<KeyBind> ComboKey { get; set; }

        //public ComboType ComboType { get; }
        public AbilityPanel AbilityPanel { get; }
        public AutoSunStrike AutoSunStrike { get; }
        public SmartSphere SmartSphere { get; }
        public AutoGhostWalk AutoGhostWalk { get; }
        public InvokeHelper FastInvoke { get; }
        public ComboPanel ComboPanel { get; }
        public Prepare Prepare { get; }
        public MenuFactory Factory { get; set; }
    }
}