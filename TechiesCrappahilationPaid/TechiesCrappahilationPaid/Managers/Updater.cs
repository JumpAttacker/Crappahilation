using Divine;
using Divine.SDK.Extensions;

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
                var me = EntityManager.LocalHero;
                Eul = me.GetItemById(AbilityId.item_cyclone);
                ForceStaff = me.GetItemById(AbilityId.item_force_staff);
            });

        }
        public Item ForceStaff { get; set; }
        public Item Eul { get; set; }
        
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