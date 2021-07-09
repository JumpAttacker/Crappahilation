<<<<<<< HEAD
﻿using InvokerCrappahilationPaid.Features;
=======
﻿using Divine.Menu.Items;
using InvokerCrappahilationPaid.Features;
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893

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
            Factory.CreateText("Mode: QUAS + EXORT in priority");
            RefresherBehavior = Factory.CreateSelector("Refresher Behavior",
                new[] {"After Meteor+Blast", "When 95% of abilities are on cd", "In both cases"});
            PrepareKey = Factory.CreateHoldKey("Prepare key");
            UseForges = Factory.CreateSwitcher("Use forges in Combo");
            UseEul = Factory.CreateSwitcher("Use eul in Dynamic Combo", true);
            UseIceWall = Factory.CreateSwitcher("Use IceWall in Dynamic Combo", true);
            BackToDynamicCombo = Factory.CreateSwitcher("Back to dynamic combo after custom combo", true);
            UseCataclysm = Factory.CreateSlider("Min targets in eul/tornado for cataclysm", 1, 0, 5);
            //ComboType = new ComboType(this);
            AbilityPanel = new AbilityPanel(this);
            AutoSunStrike = new AutoSunStrike(this);
            SmartSphere = new SmartSphere(this);
            AutoGhostWalk = new AutoGhostWalk(this);
            FastInvoke = new InvokeHelper(this);
            ComboPanel = new ComboPanel(this);
            Prepare = new Prepare(Main);
        }

        public MenuSlider UseCataclysm { get; set; }

        public MenuSwitcher UseEul { get; set; }

        public MenuSwitcher UseForges { get; set; }

        public MenuSwitcher UseIceWall { get; set; }

        public MenuSwitcher BackToDynamicCombo { get; set; }

        public MenuHoldKey PrepareKey { get; set; }

        public MenuSelector RefresherBehavior { get; set; }

        public MenuHoldKey ComboKey { get; set; }

        public RootMenu Factory { get; set; }

        public AbilityPanel AbilityPanel { get; }
        public AutoSunStrike AutoSunStrike { get; }
        public SmartSphere SmartSphere { get; }
        public AutoGhostWalk AutoGhostWalk { get; }
        public InvokeHelper FastInvoke { get; }
        public ComboPanel ComboPanel { get; }
        public Prepare Prepare { get; }
    }
}