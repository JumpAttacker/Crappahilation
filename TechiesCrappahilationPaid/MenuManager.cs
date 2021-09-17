using System.Collections.Generic;

using Divine.Entity.Entities.Units.Heroes.Components;
using Divine.Menu.Items;
using Divine.Numerics;
using Divine.Renderer;

namespace TechiesCrappahilationPaid
{
    public class MenuManager
    {
        public RootMenu BaseMenu { get; set; }

        public MenuManager(TechiesCrappahilationPaid main)
        {
            Main = main;
            Factory = Divine.Menu.MenuManager.CreateRootMenu("Techies Crappahilation");
            Factory.SetFontColor(Color.Violet);
            AutoDetonate = Factory.CreateMenu("Auto Detonate");
            AutoPlanting = Factory.CreateMenu("Auto Planting");
            VisualSubMenu = Factory.CreateMenu("Visual");
            DamagePanel = VisualSubMenu.CreateMenu("Damage panel");
            GoodPositions = VisualSubMenu.CreateMenu("Good positions");
            StackMenu = VisualSubMenu.CreateMenu("Stack Info");
            RangeMenu = VisualSubMenu.CreateMenu("Range");


            DrawStacks = StackMenu.CreateSwitcher("Draw stacks", true);
            StackDontDrawSolo = StackMenu.CreateSwitcher("Dont draw stack for only one bomb", true);

            EnableAutoDetonate = AutoDetonate.CreateSwitcher("Enable auto detonate", true);
            var enableOnToggleAutoDetonate = AutoDetonate.CreateHoldKey("Enable on toggle auto detonate", System.Windows.Input.Key.None);
            enableOnToggleAutoDetonate.ValueChanged += (_, e) =>
            {
                if (!e.Value)
                {
                    return;
                }

                EnableAutoDetonate.Value = !EnableAutoDetonate;
            };

            DetonateOnAegis = AutoDetonate.CreateSwitcher("Detonate in aegis", true);
            DetonateAllInOnce = AutoDetonate.CreateSwitcher("Detonate all in once", false);
            UseFocusedDetonation = AutoDetonate.CreateSwitcher("Detonate all in once with focused detonation", false);
            DetonateOnLowHp = AutoDetonate.CreateSwitcher("Detonate on low hp", true);
            var toggleDetonateOnLowHp = AutoDetonate.CreateHoldKey("Toggle detonate on low hp", System.Windows.Input.Key.None);
            toggleDetonateOnLowHp.ValueChanged += (_, e) =>
            {
                if (!e.Value)
                {
                    return;
                }

                DetonateOnLowHp.Value = !DetonateOnLowHp;
            };

            CameraMove = AutoDetonate.CreateSwitcher("Move camera", true);
            UsePrediction = AutoDetonate.CreateSwitcher("Use prediction", true);
            DelayOnDetonate = AutoDetonate.CreateSlider("Delay on detonate", 0, 0, 1000);
            DelayOnDetonate.SetTooltip("set 0 to disable that feature");
            Targets = AutoDetonate.CreateHeroToggler("Targets", new Dictionary<HeroId, bool>());
            UseFocusedDetonation.SetTooltip("working only if setting above is enabled");
            DetonateAllInOnce.ValueChanged += (sender, args) =>
            {
                if (!args.Value)
                {
                    UseFocusedDetonation.Value = false;
                }
            };

            RendererManager.Draw += RendererManager_Draw;
        }

        private void RendererManager_Draw()
        {
            if (EnableAutoDetonate)
            {
                return;
            }

            RendererManager.DrawText("Auto detonate disabled", new Vector2(80, 400) * RendererManager.Scaling, Color.Red, 50 * RendererManager.Scaling);
        }

        public MenuHeroToggler Targets { get; set; }

        public MenuSlider DelayOnDetonate { get; set; }

        public MenuSwitcher UsePrediction { get; set; }

        public MenuSwitcher CameraMove { get; set; }

        public MenuSwitcher UseFocusedDetonation { get; set; }

        public MenuSwitcher DetonateOnLowHp { get; }

        public MenuSwitcher DetonateOnAegis { get; set; }

        public MenuSwitcher StackDontDrawSolo { get; set; }
        public MenuSwitcher EnableAutoDetonate { get; }

        public MenuSwitcher DrawStacks { get; set; }

        public Menu RangeMenu { get; set; }

        public Menu StackMenu { get; set; }

        public Menu GoodPositions { get; set; }

        public Menu DamagePanel { get; set; }

        public Menu VisualSubMenu { get; set; }

        public Menu AutoPlanting { get; set; }

        public MenuSwitcher DetonateAllInOnce { get; set; }

        public Menu AutoDetonate { get; set; }


        public RootMenu Factory { get; set; }

        public TechiesCrappahilationPaid Main { get; }
    }
}