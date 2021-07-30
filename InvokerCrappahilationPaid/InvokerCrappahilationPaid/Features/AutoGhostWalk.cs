using System.Linq;
using Divine.Entity;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Players;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Extensions;
using Divine.Helpers;
using Divine.Menu.Items;
using Divine.Order;
using Divine.Order.EventArgs;
using Divine.Order.Orders.Components;
using Divine.Update;

namespace InvokerCrappahilationPaid.Features
{
    public class AutoGhostWalk
    {
        private readonly Config _config;
        private readonly Sleeper _sleeper;

        public AutoGhostWalk(Config config)
        {
            _config = config;
            var main = _config.Factory.CreateMenu("Auto GhostWalk");
            Enable = main.CreateSwitcher("Enable", false);
            Enable.SetTooltip("Invoke ghostWalk + 3x Wex + cast ghostWalk");
            BlockPlayerInput = main.CreateSwitcher("Block Player Input for 2500ms before/after invis", false);
            BlockPlayerInput.SetTooltip("will block all input except [MoveLocation] command");
            HealthPercent = main.CreateSlider("Health (%)", 15, 1, 100);
            EnemiesInRange = main.CreateSlider("Min enemies in range", 2, 1, 5);
            Range = main.CreateSlider("Range", 1000, 200, 2500);
            _sleeper = new Sleeper();
            if (Enable) Activate();

            Enable.ValueChanged += (sender, args) =>
            {
                if (Enable)
                    Activate();
                else
                    Deactivate();
            };
        }

        public MenuSlider Range { get; set; }

        public MenuSlider EnemiesInRange { get; set; }

        public MenuSlider HealthPercent { get; set; }

        public MenuSwitcher BlockPlayerInput { get; set; }

        public MenuSwitcher Enable { get; set; }


        private void Activate()
        {
            UpdateManager.CreateIngameUpdate(50, AutoGhostWalkAction);
        }

        private void Deactivate()
        {
            UpdateManager.DestroyGameUpdate(AutoGhostWalkAction);
        }

        private void AutoGhostWalkAction()
        {
            if (_sleeper.Sleeping || !_config.Main.AbilitiesInCombo.GhostWalk.CanBeCasted() ||
                !_config.Main.AbilitiesInCombo.GhostWalk.CanBeInvoked ||
                _config.Main.AbilitiesInCombo.Quas.Level <= 0 || _config.Main.AbilitiesInCombo.Wex.Level <= 0 ||
                !_config.Main.Me.IsAlive ||
                _config.Main.Me.HealthPercent() > HealthPercent / 100f)
                return;
            if (_config.Main.Me.IsInvisible() || _config.Main.Me.HasAnyModifiers("modifier_invoker_ghost_walk_self",
                "modifier_rune_invis", "modifier_invisible"))
                return;
            var enemies = EntityManager.GetEntities<Hero>().Count(x =>
                x.IsValid && x.IsAlive && x.IsEnemy(_config.Main.Me) && x.IsVisible && !x.IsIllusion &&
                x.IsInRange(_config.Main.Me, Range));
            if (enemies >= EnemiesInRange)
            {
                _config.SmartSphere.Sleeper.Sleep(2.500f);
                _sleeper.Sleep(2.500f);
                if (BlockPlayerInput)
                {
                    OrderManager.OrderAdding += Blocker;
                    UpdateManager.BeginInvoke(2250, () => { OrderManager.OrderAdding -= Blocker; });
                }

                UpdateManager.BeginInvoke(500, () =>
                {
                    _config.Main.AbilitiesInCombo.GhostWalk.Invoke();
                    _config.Main.AbilitiesInCombo.Wex.UseAbility();
                    _config.Main.AbilitiesInCombo.Wex.UseAbility();
                    _config.Main.AbilitiesInCombo.Wex.UseAbility();
                    _config.Main.AbilitiesInCombo.GhostWalk.UseAbility();
                });
            }
        }

        private void Blocker(OrderAddingEventArgs args)
        {
            if (BlockPlayerInput)
            {
                if (args.Order.Type == OrderType.MovePosition || args.Order.Ability != null &&
                    (args.Order.Ability.Id is AbilityId.invoker_ghost_walk or AbilityId.invoker_wex or AbilityId.invoker_invoke or AbilityId.invoker_quas))
                {
                    //args.Process = true;
                }
                else
                {
                    args.Process = false;
                }
            }
        }
    }
}