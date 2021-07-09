using Divine.Entity;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Update;

using O9K.Core.Entities.Abilities.Items;

namespace TechiesCrappahilationPaid.Managers
{
    public class Updater
    {
        public readonly TechiesCrappahilationPaid _main;

        public Updater(TechiesCrappahilationPaid main)
        {
            _main = main;

            DamageChecker = new DamageChecker(this);

            DamageChecker.OnAghanimStatusChanging += OnChange;

            BombManager = new BombManager(this);

            BombDamageManager = new BombDamageManager(this);
            
            UpdateManager.CreateUpdate(500, () =>
            {
                Eul = O9K.Core.Managers.Entity.EntityManager9.GetAbility<EulsScepterOfDivinity>(EntityManager.LocalHero.Handle);
                ForceStaff = O9K.Core.Managers.Entity.EntityManager9.GetAbility<ForceStaff>(EntityManager.LocalHero.Handle);
                Hex = O9K.Core.Managers.Entity.EntityManager9.GetAbility<ScytheOfVyse>(EntityManager.LocalHero.Handle);
            });

        }
        public ForceStaff ForceStaff { get; set; }
        public ScytheOfVyse Hex { get; set; }
        public EulsScepterOfDivinity Eul { get; set; }
        
        public DamageChecker DamageChecker { get; }

        private Hero Me => _main.Me;

        public BombManager BombManager { get; }
        public BombDamageManager BombDamageManager { get; }

        private void OnChange(bool dropped)
        {
            _main.RemoteMine.HasAgh = !dropped;
            foreach (var remoteMine in BombManager.RemoteMines)
            {
                float val;
                if (remoteMine.HasAghBuff && dropped)
                {
                    val = -150;
                    remoteMine.HasAghBuff = false;
                    remoteMine.SetDamage(remoteMine.Damage + val);
                }
                else if (!dropped && !remoteMine.HasAghBuff)
                {
                    val = 150;
                    remoteMine.HasAghBuff = true;
                    remoteMine.SetDamage(remoteMine.Damage + val);
                }
            }
        }
    }
}