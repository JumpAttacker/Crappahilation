using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;

namespace TechiesCrappahilationPaid.Features.ViewDamageFromBombs
{
    public class ViewManager
    {
        public readonly TechiesCrappahilationPaid Main;

        private ViewBombCountBase _currentView;
        public int EnabledCount;
        public AbilityId[] EnabledList;

        public ViewManager(TechiesCrappahilationPaid main)
        {
            Main = main;

            InterfaceType = Main.MenuManager.DamagePanel.Item("Damage drawing",
                new StringList("Movable Panel", "[not implemented] Top panel"));
            var dict = new Dictionary<string, bool>
            {
                {AbilityId.techies_remote_mines.ToString(), true},
                {AbilityId.techies_suicide.ToString(), true},
                {AbilityId.techies_land_mines.ToString(), true}
            };
            AbilityToggle = Main.MenuManager.DamagePanel.Item("Show damage counter", new AbilityToggler(dict));

            AbilityToggle.PropertyChanged += (sender, args) => { EnabledList = GetEnabledAbilities(); };
            ShowDamageType = Main.MenuManager.DamagePanel.Item("Bomb damage draw type",
                new StringList("Only for current hp", "Only for max hp", "For current & max hp"));

            PositionX = Main.MenuManager.DamagePanel.Item("Extra Position X",
                new Slider(0, -1000, 4000));
            PositionY = Main.MenuManager.DamagePanel.Item("Extra Position Y",
                new Slider(0, -1000, 4000));

            if (InterfaceType.Value.SelectedIndex == 0)
                ChangeView(new ViewOnMovablePanel(this));
            else
                ChangeView(new ViewOnTopPanel(this));
            InterfaceType.PropertyChanged += (sender, args) =>
            {
                if (InterfaceType.Value.SelectedIndex == 0)
                    ChangeView(new ViewOnMovablePanel(this));
                else
                    ChangeView(new ViewOnTopPanel(this));
            };
        }

        public MenuItem<Slider> PositionY { get; set; }

        public MenuItem<Slider> PositionX { get; set; }

        public MenuItem<StringList> ShowDamageType { get; set; }

        public MenuItem<AbilityToggler> AbilityToggle { get; set; }

        public MenuItem<StringList> InterfaceType { get; set; }

        public void ChangeView(ViewBombCountBase nextView)
        {
            _currentView?.Dispose();
            _currentView = nextView;
            Main.Context.RenderManager.Draw += _currentView.Draw;
        }

        public AbilityId[] GetEnabledAbilities()
        {
            var array = AbilityToggle.Value.Dictionary.Where(x => x.Value)
                .Select(x => (AbilityId) Enum.Parse(typeof(AbilityId), x.Key, true)).ToArray();
            EnabledCount = array.Length;
            return array;
        }
    }
}