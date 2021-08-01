using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Divine.Entity.Entities.Abilities;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Extensions;
using Divine.Update;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Abilities.Heroes.Brewmaster.Spirits;
using O9K.Core.Entities.Abilities.Heroes.Invoker;
using O9K.Core.Entities.Abilities.Heroes.Invoker.BaseAbilities;
using O9K.Core.Entities.Abilities.Items;
using O9K.Core.Managers.Entity;
using O9K.Core.Managers.Entity.Monitors;

namespace InvokerCrappahilationPaid
{
    public class AbilitiesInCombo
    {
        private readonly InvokerCrappahilationPaid _main;

        public AbilitiesInCombo(InvokerCrappahilationPaid main)
        {
            _main = main;

            SunStrike = new InvokerSunStrike((SunStrike) (LoadAbility(AbilityId.invoker_sun_strike)));
            Alacrity = new InvokerAlacrity((Alacrity) (LoadAbility(AbilityId.invoker_alacrity)));
            Meteor = new InvokerChaosMeteor((ChaosMeteor) (LoadAbility(AbilityId.invoker_chaos_meteor)));
            ColdSnap = new InvokerColdSnap((ColdSnap) (LoadAbility(AbilityId.invoker_cold_snap)));
            Blast = new InvokerDeafeningBlast((DeafeningBlast) (LoadAbility(AbilityId.invoker_deafening_blast)));
            Emp = new InvokerEmp((EMP) (LoadAbility(AbilityId.invoker_emp)));
            ForgeSpirit = new InvokerForgeSpirit((ForgeSpirit) (LoadAbility(AbilityId.invoker_forge_spirit)));
            GhostWalk = new InvokerGhostWalk((GhostWalk) (LoadAbility(AbilityId.invoker_ghost_walk)));
            IceWall = new InvokerIceWall((IceWall) (LoadAbility(AbilityId.invoker_ice_wall)));
            Invoke = new InvokerInvoke((Invoke) (LoadAbility(AbilityId.invoker_invoke)));
            Tornado = new InvokerTornado((Tornado) (LoadAbility(AbilityId.invoker_tornado)));
            Quas = new InvokerQuas((Quas) (LoadAbility(AbilityId.invoker_quas)));
            Wex = new InvokerWex((Wex) (LoadAbility(AbilityId.invoker_wex)));
            Exort = new InvokerExort((Exort) (LoadAbility(AbilityId.invoker_exort)));

            AllAbilities = new List<InvokerBaseAbility>
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

            Spheres = new List<InvokerSimpleBaseAbility>
            {
                Quas, Wex, Exort
            };

            foreach (var activeAbility in AllAbilities)
            {
                // Console.WriteLine($"Active: {activeAbility.BaseAbility.Id} {activeAbility.BaseAbility.Owner}");
            }

            foreach (var activeAbility in Spheres)
            {
                // Console.WriteLine($"Active: {activeAbility.BaseAbility.Id} {activeAbility.BaseAbility.Owner}");
            }


            AllItems = new List<ActiveAbility>
            {
                Hex, Shiva, Bkb, Orchid, Bloodthorn, Eul, Refresher, RefresherShard, Blink, Veil, EtherealBlade
            };
            // EntityManager9.AbilityAdded += entity => { Console.WriteLine($"AbilityAdded: {entity.Id}"); };
            // EntityManager9.AbilityRemoved += entity => { Console.WriteLine($"AbilityRemoved: {entity.Id}"); };
            

            UpdateManager.CreateIngameUpdate(100, () => { });
        }

        public List<InvokerBaseAbility> AllAbilities { get; set; }
        public List<ActiveAbility> AllItems { get; set; }
        public List<InvokerSimpleBaseAbility> Spheres { get; set; }

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


        
        public ScytheOfVyse Hex => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_sheepstick) as ScytheOfVyse;
        public ShivasGuard Shiva => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_shivas_guard) as ShivasGuard;
        public BlackKingBar Bkb => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_black_king_bar) as BlackKingBar;
        public OrchidMalevolence Orchid => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_orchid) as OrchidMalevolence;
        public Bloodthorn Bloodthorn => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_bloodthorn) as Bloodthorn;
        public EulsScepterOfDivinity Eul => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_cyclone) as EulsScepterOfDivinity;
        public RefresherOrb Refresher => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_refresher) as RefresherOrb;
        public RefresherOrb RefresherShard => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_refresher_shard) as RefresherOrb;
        public BlinkDagger Blink => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_blink) as BlinkDagger;
        public VeilOfDiscord Veil => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_recipe_veil_of_discord) as VeilOfDiscord;
        public EtherealBlade EtherealBlade => EntityManager9.Abilities.FirstOrDefault(z => z.Id == AbilityId.item_ethereal_blade) as EtherealBlade;

        // public ShivasGuard Shiva { get; set; }

        // public BlackKingBar Bkb { get; set; }

        // public OrchidMalevolence Orchid { get; set; }

        // public Bloodthorn Bloodthorn { get; set; }

        // public EulsScepterOfDivinity Eul { get; set; }

        // public RefresherOrb Refresher { get; set; }

        // public RefresherOrb RefresherShard { get; set; }

        // public BlinkDagger Blink { get; set; }

        // public VeilOfDiscord Veil { get; set; }

        // public EtherealBlade EtherealBlade { get; set; }

        private Ability9 LoadAbility(AbilityId id)
        {
            var ability = _main.Me.GetAbilityById(id);
            if (ability == null)
            {
                Console.WriteLine($"can find ability with id: {id}");
                throw new NullReferenceException($"can find ability with id: {id}");
            }

            var abi = EntityManager9.GetAbility(ability.Handle);
            // Console.WriteLine($"{abi.Id} -> {abi.Owner}");

            return EntityManager9.GetAbility(ability.Handle);
            // return ability;
        }
    }
}