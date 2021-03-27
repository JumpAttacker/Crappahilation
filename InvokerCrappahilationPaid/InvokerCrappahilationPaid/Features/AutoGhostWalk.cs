using System.Linq;

using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;

namespace InvokerCrappahilationPaid.Features
{
    public class AutoGhostWalk
    {
        private readonly Config _config;
        private readonly Sleeper _sleeper;

        public AutoGhostWalk(Config config)
        {
            _config = config;
            var main = _config.Factory.Menu("Auto GhostWalk");
            Enable = main.Item("Enable", false);
            Enable.Item.SetTooltip("Invoke ghostWalk + 3x Wex + cast ghostWalk");
            BlockPlayerInput = main.Item("Block Player Input for 2500ms before/after invis", false);
            BlockPlayerInput.Item.SetTooltip("will block all input except [MoveLocation] command");
            HealthPercent = main.Item("Health (%)", new Slider(15, 1, 100));
            EnemiesInRange = main.Item("Min enemies in range", new Slider(2, 1, 5));
            Range = main.Item("Range", new Slider(1000, 200, 2500));
            _sleeper = new Sleeper();
            if (Enable) Activate();

            Enable.PropertyChanged += (sender, args) =>
            {
                if (Enable)
                    Activate();
                else
                    Deactivate();
            };
        }

        public MenuItem<bool> BlockPlayerInput { get; set; }

        public MenuItem<Slider> Range { get; set; }

        public MenuItem<Slider> EnemiesInRange { get; set; }

        public MenuItem<Slider> HealthPercent { get; set; }

        public MenuItem<bool> Enable { get; set; }

        private void Activate()
        {
            UpdateManager.Subscribe(AutoGhostWalkAction, 50);
        }

        private void Deactivate()
        {
            UpdateManager.Unsubscribe(AutoGhostWalkAction);
        }

        private void AutoGhostWalkAction()
        {
            if (_sleeper.Sleeping || !_config.Main.AbilitiesInCombo.GhostWalk.CanBeCasted ||
                !_config.Main.AbilitiesInCombo.GhostWalk.CanBeInvoked ||
                _config.Main.AbilitiesInCombo.Quas.Level <= 0 || _config.Main.AbilitiesInCombo.Wex.Level <= 0 ||
                !_config.Main.Me.IsAlive ||
                _config.Main.Me.HealthPercent() > HealthPercent / 100f)
                return;
            if (_config.Main.Me.IsInvisible() || _config.Main.Me.HasAnyModifiers("modifier_invoker_ghost_walk_self",
                    "modifier_rune_invis", "modifier_invisible"))
                return;
            var enemies = EntityManager<Hero>.Entities.Count(x =>
                x.IsValid && x.IsAlive && x.IsEnemy(_config.Main.Me) && x.IsVisible && !x.IsIllusion &&
                x.IsInRange(_config.Main.Me, Range));
            if (enemies >= EnemiesInRange)
            {
                _config.SmartSphere.Sleeper.Sleep(2500);
                _sleeper.Sleep(2500);
                if (BlockPlayerInput)
                {
                    Player.OnExecuteOrder += Blocker;
                    UpdateManager.BeginInvoke(() => { Player.OnExecuteOrder -= Blocker; }, 2250);
                }

                UpdateManager.BeginInvoke(() =>
                {
                    _config.Main.AbilitiesInCombo.GhostWalk.Invoke();
                    _config.Main.AbilitiesInCombo.Wex.UseAbility();
                    _config.Main.AbilitiesInCombo.Wex.UseAbility();
                    _config.Main.AbilitiesInCombo.Wex.UseAbility();
                    _config.Main.AbilitiesInCombo.GhostWalk.UseAbility();
                }, 500);
            }
        }

        private void Blocker(Player player, ExecuteOrderEventArgs args)
        {
            if (BlockPlayerInput)
            {
                if (args.OrderId == OrderId.MoveLocation || args.Ability != null &&
                    (args.Ability.Id == AbilityId.invoker_ghost_walk || args.Ability.Id == AbilityId.invoker_wex ||
                     args.Ability.Id == AbilityId.invoker_invoke || args.Ability.Id == AbilityId.invoker_quas))
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