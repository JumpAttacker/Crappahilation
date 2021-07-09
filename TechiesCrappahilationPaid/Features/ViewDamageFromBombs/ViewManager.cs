using System.Collections.Generic;
using System.Linq;
using Divine.Renderer;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Menu.Items;

namespace TechiesCrappahilationPaid.Features.ViewDamageFromBombs
{
    public class ViewManager
    {
        public readonly TechiesCrappahilationPaid Main;

        private ViewBombCountBase _currentView;
        public int EnabledCount;
        public AbilityId[] EnabledList;
        private Dictionary<AbilityId, bool> dict;

        public ViewManager(TechiesCrappahilationPaid main)
        {
            Main = main;

            InterfaceType = Main.MenuManager.DamagePanel.CreateSwitcher("Damage drawing");
                // new StringList("Movable Panel", "[not implemented] Top panel"));
            dict = new Dictionary<AbilityId, bool>
            {
                {AbilityId.techies_remote_mines, true},
                {AbilityId.techies_suicide, true},
                {AbilityId.techies_land_mines, true}
            };
            AbilityToggle = Main.MenuManager.DamagePanel.CreateAbilityToggler("Show damage counter", dict);

            AbilityToggle.ValueChanged += (sender, args) => { EnabledList = GetEnabledAbilities(); };
            ShowDamageType = Main.MenuManager.DamagePanel.
                CreateSelector("Bomb damage draw type", new []{"Only for current hp", "Only for max hp", "For current & max hp"});
                // new StringList());

            PositionX = Main.MenuManager.DamagePanel.CreateSlider("Extra Position X",
                0, -1000, 4000);
            PositionY = Main.MenuManager.DamagePanel.CreateSlider("Extra Position Y",
                0, -1000, 4000);

            if (InterfaceType.Value)
                ChangeView(new ViewOnMovablePanel(this));
            else
                ChangeView(new ViewOnTopPanel(this));
            InterfaceType.ValueChanged += (sender, args) =>
            {
                if (args.Value)
                    ChangeView(new ViewOnMovablePanel(this));
                else
                    ChangeView(new ViewOnTopPanel(this));
            };
        }

        public MenuSlider PositionY { get; set; }

        public MenuSlider PositionX { get; set; }

        public MenuSelector ShowDamageType { get; set; }

        public MenuAbilityToggler AbilityToggle { get; set; }

        public MenuSwitcher InterfaceType { get; set; }

        public void ChangeView(ViewBombCountBase nextView)
        {
            _currentView?.Dispose();
            _currentView = nextView;
            RendererManager.Draw += _currentView.Draw;
        }

        public AbilityId[] GetEnabledAbilities()
        {
            var array = dict.Select(x => x.Key).Where(x => AbilityToggle.GetValue(x)).ToArray();
                // .Select(x => (AbilityId) Enum.Parse(typeof(AbilityId), x.Key, true)).ToArray();
            EnabledCount = array.Length;
            return array;
        }
    }
}