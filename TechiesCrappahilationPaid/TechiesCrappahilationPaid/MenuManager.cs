using System.Collections.Generic;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;
using SharpDX;

namespace TechiesCrappahilationPaid
{
    public class MenuManager
    {
        public MenuManager(TechiesCrappahilationPaid main)
        {
            Main = main;
            Factory = MenuFactory.Create("Techies Crappahilation");
            Factory.Target.SetFontColor(Color.Violet);
            AutoDetonate = Factory.Menu("Auto Detonate");
            AutoPlanting = Factory.Menu("Auto Planting");
            VisualSubMenu = Factory.Menu("Visual");
            DamagePanel = VisualSubMenu.Menu("Damage panel");
            GoodPositions = VisualSubMenu.Menu("Good positions");
            StackMenu = VisualSubMenu.Menu("Stack Info");
            RangeMenu = VisualSubMenu.Menu("Range");


            DrawStacks = StackMenu.Item("Draw stacks", true);
            StackDontDrawSolo = StackMenu.Item("Dont draw stack for only one bomb", true);

            DetonateOnAegis = AutoDetonate.Item("Detonate in aegis", true);
            DetonateAllInOnce = AutoDetonate.Item("Detonate all in once", false);
            UseFocusedDetonation = AutoDetonate.Item("Detonate all in once with focused detonation", false);
            CameraMove = AutoDetonate.Item("Move camera", true);
            UsePrediction = AutoDetonate.Item("Use prediction", true);
            DelayOnDetonate = AutoDetonate.Item("Delay on detonate", new Slider(0, 0, 1000));
            DelayOnDetonate.Item.SetTooltip("set 0 to disable that feature");
            Targets = AutoDetonate.Item("Targets", new HeroToggler(new Dictionary<string, bool>()));
            UseFocusedDetonation.Item.SetTooltip("working only if setting above is enabled");
            DetonateAllInOnce.PropertyChanged += (sender, args) =>
            {
                if (!DetonateAllInOnce)
                {
                    UseFocusedDetonation.Item.SetValue(false);
                }
            };
        }

        public MenuItem<bool> UseFocusedDetonation { get; set; }

        public MenuItem<bool> UsePrediction { get; set; }

        public MenuFactory RangeMenu { get; set; }

        public MenuItem<bool> StackDontDrawSolo { get; set; }

        public MenuItem<bool> DrawStacks { get; set; }

        public MenuFactory StackMenu { get; set; }

        public MenuItem<HeroToggler> Targets { get; set; }


        public MenuFactory GoodPositions { get; set; }

        public MenuFactory DamagePanel { get; set; }

        public MenuFactory AutoPlanting { get; set; }

        public MenuItem<bool> DetonateAllInOnce { get; set; }

        public MenuItem<bool> DetonateOnAegis { get; set; }

        public MenuItem<Slider> DelayOnDetonate { get; set; }

        public MenuItem<bool> CameraMove { get; set; }

        public MenuFactory AutoDetonate { get; set; }

        public MenuFactory VisualSubMenu { get; set; }

        public MenuFactory Factory { get; set; }

        public TechiesCrappahilationPaid Main { get; }
    }
}