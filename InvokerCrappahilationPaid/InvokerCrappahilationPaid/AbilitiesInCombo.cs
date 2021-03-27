using System.Collections.Generic;

using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Extensions;
using Ensage.SDK.Inventory.Metadata;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;

namespace InvokerCrappahilationPaid
{
    public class AbilitiesInCombo
    {
        private readonly InvokerCrappahilationPaid _main;

        public AbilitiesInCombo(InvokerCrappahilationPaid main)
        {
            _main = main;
            main.Context.Inventory.Attach(this);

            SunStrike = new InvokerSunStrike(LoadAbility(AbilityId.invoker_sun_strike));
            Alacrity = new InvokerAlacrity(LoadAbility(AbilityId.invoker_alacrity));
            Meteor = new InvokerChaosMeteor(LoadAbility(AbilityId.invoker_chaos_meteor));
            ColdSnap = new InvokerColdSnap(LoadAbility(AbilityId.invoker_cold_snap));
            Blast = new InvokerDeafeningBlast(LoadAbility(AbilityId.invoker_deafening_blast));
            Emp = new InvokerEmp(LoadAbility(AbilityId.invoker_emp));
            ForgeSpirit = new InvokerForgeSpirit(LoadAbility(AbilityId.invoker_forge_spirit));
            GhostWalk = new InvokerGhostWalk(LoadAbility(AbilityId.invoker_ghost_walk));
            IceWall = new InvokerIceWall(LoadAbility(AbilityId.invoker_ice_wall));
            Invoke = new InvokerInvoke(LoadAbility(AbilityId.invoker_invoke));
            Tornado = new InvokerTornado(LoadAbility(AbilityId.invoker_tornado));
            Quas = new InvokerQuas(LoadAbility(AbilityId.invoker_quas));
            Wex = new InvokerWex(LoadAbility(AbilityId.invoker_wex));
            Exort = new InvokerExort(LoadAbility(AbilityId.invoker_exort));

            AllAbilities = new List<ActiveAbility>
            {
                SunStrike,
                Alacrity,
                Meteor,
                ColdSnap,
                Blast,
                Emp,
                ForgeSpirit,
                IceWall,
                Tornado,
                GhostWalk
            };

            LoadAbilitiesFromDota(AbilityId.item_sheepstick, AbilityId.item_refresher, AbilityId.item_orchid,
                AbilityId.item_bloodthorn, AbilityId.item_blink, AbilityId.item_cyclone, AbilityId.item_black_king_bar,
                AbilityId.item_shivas_guard, AbilityId.item_refresher_shard);

            LoadAbilitiesFromDota(AbilityId.invoker_quas, AbilityId.invoker_wex, AbilityId.invoker_exort);
        }

        public List<ActiveAbility> AllAbilities { get; set; }

        public InvokerExort Exort { get; set; }

        public InvokerWex Wex { get; set; }

        public InvokerQuas Quas { get; set; }

        public InvokerTornado Tornado { get; set; }

        public InvokerInvoke Invoke { get; set; }

        public InvokerIceWall IceWall { get; set; }

        public InvokerGhostWalk GhostWalk { get; set; }

        public InvokerForgeSpirit ForgeSpirit { get; set; }

        public InvokerEmp Emp { get; set; }

        public InvokerDeafeningBlast Blast { get; set; }

        public InvokerColdSnap ColdSnap { get; set; }

        public InvokerChaosMeteor Meteor { get; set; }

        public InvokerAlacrity Alacrity { get; set; }
        public InvokerSunStrike SunStrike { get; set; }


        [ItemBinding] public item_sheepstick Hex { get; set; }

        [ItemBinding] public item_necronomicon Necronomicon { get; set; }

        [ItemBinding] public item_necronomicon_2 Necronomicon2 { get; set; }

        [ItemBinding] public item_necronomicon_3 Necronomicon3 { get; set; }

        [ItemBinding] public item_shivas_guard Shiva { get; set; }

        [ItemBinding] public item_black_king_bar Bkb { get; set; }

        [ItemBinding] public item_orchid Orchid { get; set; }

        [ItemBinding] public item_bloodthorn Bloodthorn { get; set; }

        [ItemBinding] public item_cyclone Eul { get; set; }

        [ItemBinding] public item_refresher Refresher { get; set; }

        [ItemBinding] public item_refresher_shard RefresherShard { get; set; }

        [ItemBinding] public item_blink Blink { get; set; }

        [ItemBinding] public item_veil_of_discord Veil { get; set; }

        [ItemBinding] public item_ethereal_blade EtherealBlade { get; set; }

        private void LoadAbilitiesFromDota(params AbilityId[] abilities)
        {
            foreach (var id in abilities) _main.Context.TextureManager.LoadAbilityFromDota(id);
        }

        private Ability LoadAbility(AbilityId id)
        {
            _main.Context.TextureManager.LoadAbilityFromDota(id);
            return _main.Context.Owner.GetAbilityById(id);
        }
    }
}