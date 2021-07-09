using System.Collections.Generic;
<<<<<<< HEAD

=======
using Divine;
using Divine.SDK.Extensions;
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Abilities.Heroes.Invoker.BaseAbilities;

namespace InvokerCrappahilationPaid
{
    public class AbilitiesInCombo
    {
        private readonly InvokerCrappahilationPaid _main;

        public AbilitiesInCombo(InvokerCrappahilationPaid main)
        {
            _main = main;

            SunStrike = new InvokerSunStrike(LoadAbility(AbilityId.invoker_sun_strike));
            Alacrity = new InvokerAlacrity(LoadAbility(AbilityId.invoker_alacrity));
            Meteor = new InvokerChaosMeteor(LoadAbility(AbilityId.invoker_chaos_meteor));
            ColdSnap = new InvokerColdSnap(LoadAbility(AbilityId.invoker_cold_snap));
            Blast = new InvokerDeafeningBlast(LoadAbility(AbilityId.invoker_deafening_blast));
            Emp = new InvokerEmp(LoadAbility(AbilityId.invoker_emp));
            ForgeSpirit = new InvokerForgeSpirit(LoadAbility(AbilityId.invoker_forge_spirit));
            GhostWalk = new InvokerGhostWalk(LoadAbility(AbilityId.invoker_ghost_walk));
            IceWall = new InvokerIceWall(LoadAbility(AbilityId.invoker_ice_wall));
            Invoke = new Invoke(LoadAbility(AbilityId.invoker_invoke));
            Tornado = new InvokerTornado(LoadAbility(AbilityId.invoker_tornado));
            Quas = new Quas(LoadAbility(AbilityId.invoker_quas));
            Wex = new Wex(LoadAbility(AbilityId.invoker_wex));
            Exort = new Exort(LoadAbility(AbilityId.invoker_exort));

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
        }

        public List<ActiveAbility> AllAbilities { get; set; }

        public Exort Exort { get; set; }

        public Wex Wex { get; set; }

        public Quas Quas { get; set; }

        public InvokerTornado Tornado { get; set; }

        public Invoke Invoke { get; set; }

        public InvokerIceWall IceWall { get; set; }

        public InvokerGhostWalk GhostWalk { get; set; }

        public InvokerForgeSpirit ForgeSpirit { get; set; }

        public InvokerEmp Emp { get; set; }

        public InvokerDeafeningBlast Blast { get; set; }

        public InvokerColdSnap ColdSnap { get; set; }

        public InvokerChaosMeteor Meteor { get; set; }

        public InvokerAlacrity Alacrity { get; set; }
        public InvokerSunStrike SunStrike { get; set; }

        private static Ability LoadAbility(AbilityId id)
        {
            var me = EntityManager.LocalHero;
            return me.GetAbilityById(id);
        }
    }
}