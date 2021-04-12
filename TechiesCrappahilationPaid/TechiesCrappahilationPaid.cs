using System;
using System.Linq;
using Divine;
using TechiesCrappahilationPaid.Abilities;
using TechiesCrappahilationPaid.Features;
using TechiesCrappahilationPaid.Features.ViewDamageFromBombs;
using TechiesCrappahilationPaid.Helpers;
using Updater = TechiesCrappahilationPaid.Managers.Updater;

namespace TechiesCrappahilationPaid
{
    public sealed class TechiesCrappahilationPaid : Bootstrapper
    {
        private ViewManager _viewManager;

        public MenuManager MenuManager { get; private set; }
        public SuicideDamage SuicideDamage { get; private set; }
        public SuicideAbility Suicide { get; private set; }
        public LandMineAbility LandMine { get; private set; }
        public StasisMineAbility StasisMine { get; private set; }
        public RemoteMineAbility RemoteMine { get; private set; }
        public Hero Me { get; set; }
        public Updater Updater { get; private set; }


        protected override void OnActivate()
        {
            Me = EntityManager.LocalHero;
            if (Me == null || Me.HeroId != HeroId.npc_dota_hero_techies)
            {
                return;
            }

            LandMine = new LandMineAbility(Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_land_mines));
            StasisMine = new StasisMineAbility(Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_stasis_trap));
            Suicide = new SuicideAbility(Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_suicide));
            RemoteMine = new RemoteMineAbility(Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_remote_mines));
            FocusedDetonate = Me.Spellbook.Spells.First(x => x.Id == AbilityId.techies_focused_detonate);

            MenuManager = new MenuManager(this);
            Updater = new Updater(this);
            TargetManager.Init(this);
            var stackInfo = new StackInfo(this);
            SuicideDamage = new SuicideDamage(this);
            _viewManager = new ViewManager(this);
            AutoPlanter.Init(this);
            var plantHelper = new PlantHelper(this);
        }

        public Ability FocusedDetonate { get; set; }
    }
}