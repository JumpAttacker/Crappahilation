using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Linq;

=======
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;

using SharpDX;

using UnitExtensions = Ensage.Common.Extensions.UnitExtensions;

namespace InvokerCrappahilationPaid.Features
{
    public class AutoSunStrike
    {
        private readonly Config _config;

        private readonly Dictionary<uint, int> _damageDict;

        private readonly MultiSleeper _multiSleeper = new MultiSleeper();
        private readonly Dictionary<uint, float> _timeDictionary = new Dictionary<uint, float>();

        public AutoSunStrike(Config config)
        {
            _config = config;
            var main = _config.Factory.Menu("Auto SunStrike");
            Enable = main.Item("Enable", true);
            KillStealOnly = main.Item("Kill Steal Only", true);
            UseOnlyOnStunnedEnemies = main.Item("Only on stunned enemies", true);
            DrawPrediction = main.Item("Draw prediction", true);
            DrawPredictionKillSteal = main.Item("Draw prediction only when target will die after ss", true);
            InvokeSunStike = main.Item("Invoke sun strike", true);
            Notification = main.Item("Notification if target is Killable", true);
            SsTiming = main.Item("Timing for auto SunStrike (in ms)", new Slider(2000, 100, 3500));

            DrawDamage = main.Item("Draw damage from SunStrike", true);
            MoveCamera = main.Item("Move camera", true);
            //DrawDamageType = main.Item("Type of drawing", new StringList("Only text","Icon + text"));

            _damageDict = new Dictionary<uint, int>();

            if (Enable) Activate();

            if (DrawDamage) Drawing.OnDraw += OnDraw;

            DrawDamage.PropertyChanged += (sender, args) =>
            {
                if (DrawDamage)
                    Drawing.OnDraw += OnDraw;
                else
                    Drawing.OnDraw -= OnDraw;
            };

            Enable.PropertyChanged += (sender, args) =>
            {
                if (Enable)
                    Activate();
                else
                    Deactivate();
            };

            HpBarSize = new Vector2(HUDInfo.GetHpBarSizeY() * 2.5f);
            HpBarY = HUDInfo.GetHpBarSizeY();
        }

        private IRenderManager Renderer => _config.Main.Context.RenderManager;

        public MenuItem<bool> MoveCamera { get; set; }

        public MenuItem<bool> Notification { get; set; }

        public MenuItem<StringList> DrawDamageType { get; set; }

        public float HpBarY { get; set; }

        public Vector2 HpBarSize { get; set; }

        public MenuItem<bool> DrawDamage { get; set; }

        public IParticleManager ParitecleManager => _config.Main.Context.Particle;

        public MenuItem<bool> KillStealOnly { get; set; }

        public Hero Me => _config.Main.Me;

        public MenuItem<bool> DrawPredictionKillSteal { get; set; }

        public MenuItem<bool> InvokeSunStike { get; set; }

        public MenuItem<bool> DrawPrediction { get; set; }

        public InvokerSunStrike SunStrike => _config.Main.AbilitiesInCombo.SunStrike;

        public MenuItem<bool> UseOnlyOnStunnedEnemies { get; set; }

        public MenuItem<Slider> SsTiming { get; set; }

        public MenuItem<bool> Enable { get; set; }

        private void OnDraw(EventArgs args)
        {
            var allEnemies = EntityManager<Hero>.Entities.Where(x =>
                x.IsValid && x.IsAlive && x.IsVisible && x.IsEnemy(Me) && !x.IsIllusion);
            foreach (var enemy in allEnemies)
            {
                var pos = HUDInfo.GetHPbarPosition(enemy);
                if (pos.IsZero)
                    continue;
                if (!Enable) UpdateDamage(enemy, out _);
                if (_damageDict.TryGetValue(enemy.Handle, out var damage))
                {
                    var fontSize = HpBarY * 1.5f;
                    var mesText = Drawing.MeasureText($"{damage}", "arial", new Vector2(fontSize), FontFlags.None);
                    Drawing.DrawText($"{damage}", pos - new Vector2(mesText.X + 5, 0), new Vector2(fontSize),
                        Color.White, FontFlags.None);
                }
            }
        }

        public void Activate()
        {
            UpdateManager.Subscribe(SunStrikeAction, 25);
        }

        public void Deactivate()
        {
            UpdateManager.Unsubscribe(SunStrikeAction);
        }


        private void SunStrikeAction()
        {
            if (_config.ComboKey) return;
            if (!Me.IsAlive || Me.IsSilenced())
                return;
            var allEnemies = EntityManager<Hero>.Entities.Where(x =>
                x.IsValid && x.IsEnemy(Me) && !x.IsIllusion);

            var enumerable = allEnemies as Hero[] ?? allEnemies.ToArray();
            if (!SunStrike.CanBeCasted)
                foreach (var enemy in enumerable)
                {
                    if (UpdateDamage(enemy, out _) && Notification && enemy.IsAlive)
                        _config.Main.NotificationHelper.Notificate(enemy, AbilityId.invoker_sun_strike, 0f);
                    else
                        _config.Main.NotificationHelper.Deactivate(enemy);
                    if (!SunStrike.CanBeCasted)
                        ParitecleManager.Remove($"AutoSunStikePrediction{enemy.Handle}");
                    return;
                }

            if (!SunStrike.IsInvoked)
                if (!InvokeSunStike)
                    return;
            if (Me.IsInvisible())
                return;
            //var ssDamage = SunStrike.GetDamage();
            foreach (var enemy in enumerable)
            {
                var isAlive = enemy.IsAlive;
                if (!isAlive || !enemy.IsVisible)
                {
                    FlushTiming(enemy);
                    if (!isAlive)
                        _config.Main.NotificationHelper.Deactivate(enemy);
                    ParitecleManager.Remove($"AutoSunStikePrediction{enemy.Handle}");
                    continue;
                }

                if (UpdateDamage(enemy, out var heroWillDie) || !KillStealOnly)
                {
                    if (Notification && heroWillDie)
                        _config.Main.NotificationHelper.Notificate(enemy, AbilityId.invoker_sun_strike, 0f);
                    var stunned = UnitExtensions.IsStunned(enemy, out var stunDuration);
                    var immobile = stunDuration >= 1.5f; //_config.Main.AbilitiesInCombo.SunStrike.ActivationDelay;
                    var invulModifier =
                        enemy.GetModifierByName("modifier_eul_cyclone") ??
                        enemy.GetModifierByName(_config.Main.AbilitiesInCombo.Tornado.TargetModifierName) ??
                        enemy.GetModifierByName("modifier_brewmaster_storm_cyclone") ??
                        enemy.GetModifierByName("modifier_shadow_demon_disruption") ??
                        enemy.GetModifierByName("modifier_obsidian_destroyer_astral_imprisonment_prison");

                    PredictionInput input = null;
                    PredictionOutput output = null;
                    if (DrawPrediction && (heroWillDie || !DrawPredictionKillSteal))
                    {
                        input = SunStrike.GetPredictionInput(enemy);
                        output = SunStrike.GetPredictionOutput(input);

                        ParitecleManager.DrawTargetLine(enemy, $"AutoSunStikePrediction{enemy.Handle}",
                            output.UnitPosition, CanSunStikerHit(enemy) ? Color.AliceBlue : Color.Red);
                    }
                    else
                    {
                        ParitecleManager.Remove($"AutoSunStikePrediction{enemy.Handle}");
                    }

                    if (enemy.HasModifier(_config.Main.AbilitiesInCombo.ColdSnap.TargetModifierName) &&
                        !enemy.HasModifier(_config.Main.AbilitiesInCombo.Tornado.TargetModifierName))
                        continue;

                    if ((enemy.UnitState & UnitState.Stunned) == 0 && invulModifier == null)
                        if (UseOnlyOnStunnedEnemies)
                            continue;


                    if (invulModifier != null)
                    {
                        if (invulModifier.RemainingTime <= 1.7f + Game.Ping * 0.75f / 1000f &&
                            invulModifier.RemainingTime >= 1.0f)
                        {
                            InvokerCrappahilationPaid.Log.Warn(
                                $"[AutoSunStrike] use on invul modifiers [{enemy.HeroId}] (mod RemTime: {invulModifier.RemainingTime}ms + ping: {Game.Ping * 0.75f}ms) total: {1.7f + Game.Ping * 0.75f / 1000f}");
                            CameraAction(enemy.NetworkPosition);
                            SunStrike.UseAbility(enemy.NetworkPosition);
                        }
                    }
                    else
                    {
                        if (input == null)
                        {
                            input = SunStrike.GetPredictionInput(enemy);
                            output = SunStrike.GetPredictionOutput(input);
                        }

                        if (output.HitChance == HitChance.High || output.HitChance == HitChance.VeryHigh ||
                            output.HitChance == HitChance.Medium ||
                            immobile && output.HitChance == HitChance.Immobile)
                        {
                            if ((enemy.UnitState & UnitState.Stunned) != 0)
                            {
                                if (stunDuration >= 1.5f)
                                {
                                    InvokerCrappahilationPaid.Log.Warn(
                                        $"[AutoSunStrike] use on stunned enemy [{enemy.HeroId}] (left {stunDuration}ms) HitChance: {output.HitChance}");
                                    CameraAction(enemy.NetworkPosition);
                                    SunStrike.UseAbility(enemy.NetworkPosition);
                                }
                                else
                                {
                                    InvokerCrappahilationPaid.Log.Warn(
                                        $"[AutoSunStrike] DONT use on stunned enemy [{enemy.HeroId}] (left {stunDuration}ms) HitChance: {output.HitChance}");
                                }
                            }
                            else if (heroWillDie && CanSunStrikeHitWithPrediction(enemy))
                            {
                                InvokerCrappahilationPaid.Log.Warn(
                                    $"[AutoSunStrike] use cuz killSteal on predicted position [{enemy.HeroId}] HitChance: {output.HitChance}");
                            }
                        }
                    }
                }
                else
                {
                    _config.Main.NotificationHelper.Deactivate(enemy);
                    ParitecleManager.Remove($"AutoSunStikePrediction{enemy.Handle}");
                }
            }
        }

        private void CameraAction(Vector3 enemyNetworkPosition)
        {
            if (MoveCamera)
                if (Drawing.WorldToScreen(enemyNetworkPosition).IsZero)
                {
                    var consolePosition = $"{enemyNetworkPosition.X} {enemyNetworkPosition.Y}";
                    Game.ExecuteCommand($"dota_camera_set_lookatpos {consolePosition}");
                }
        }

        private bool CanSunStrikeHitWithPrediction(Hero target)
        {
            if (!target.IsRotating() && target.IsMoving)
            {
                var num1 = target.MovementSpeed * 1.75f + Game.Ping / 1000f;
                var position = target.InFront(num1);
                var num2 = 0;
                while (num2 < (double) num1)
                {
                    num2 += 64;
                    _config.Main.NavMeshHelper.Pathfinding.GetCellPosition(target.InFront(num2), out var cellX,
                        out var cellY);
                    var flag =
                        _config.Main.NavMeshHelper.Pathfinding.GetCell(cellX, cellY).NavMeshCellFlags;

                    if (CheckForFlags(flag)) continue;
                    FlushTiming(target);
                    return false;
                }

                if (!CheckForTiming(target)) return false;
                CameraAction(position);
                SunStrike.UseAbility(position);
                FlushTiming(target);
                return true;
            }

            FlushTiming(target);
            return false;
        }

        private bool CanSunStikerHit(Hero target)
        {
            if (target.IsRotating()) return false;
            var num1 = target.MovementSpeed * 1.75f + Game.Ping / 1000f;
            var num2 = 0;
            while (num2 < (double) num1)
            {
                num2 += 64;
                _config.Main.NavMeshHelper.Pathfinding.GetCellPosition(target.InFront(num2), out var cellX,
                    out var cellY);
                var flag = _config.Main.NavMeshHelper.Pathfinding.GetCell(cellX, cellY).NavMeshCellFlags;
                if (!CheckForFlags(flag))
                    return false;
            }

            return true;
        }

        private bool CheckForFlags(NavMeshCellFlags flag)
        {
            return flag.HasFlag(NavMeshCellFlags.Walkable) &&
                   !flag.HasFlag(NavMeshCellFlags.Tree) &&
                   !flag.HasFlag(NavMeshCellFlags.GridFlagObstacle) &&
                   !flag.HasFlag(NavMeshCellFlags.MovementBlocker);
        }

        private void FlushTiming(Hero target)
        {
            //InvokerCrappahilationPaid.Log.Warn($"Flush for {target.HeroId}");
            var handle = target.Handle;
            if (_timeDictionary.ContainsKey(handle))
                _timeDictionary.Remove(handle);
        }

        private bool CheckForTiming(Hero target, out float timing)
        {
            var handle = target.Handle;
            var currentTime = Game.RawGameTime;
            if (_timeDictionary.TryGetValue(handle, out var time))
            {
                timing = currentTime - time;
                return timing >= SsTiming / 1000f;
            }

            timing = 0f;
            _timeDictionary.Add(handle, currentTime);
            return false;
        }

        private bool CheckForTiming(Hero target)
        {
            return CheckForTiming(target, out _);
        }

        private bool UpdateDamage(Hero enemy, out bool heroWillDie)
        {
            if (_multiSleeper.Sleeping(enemy.Handle))
            {
                if (_damageDict.TryGetValue(enemy.Handle, out var hp))
                {
                    heroWillDie = hp <= 0;
                    if (!heroWillDie) FlushTiming(enemy);
                    return heroWillDie;
                }

                FlushTiming(enemy);
                heroWillDie = false;
                return false;
            }

            _multiSleeper.Sleep(50, enemy.Handle);
            var willTakeDamageFromTornado =
                enemy.GetModifierByName(_config.Main.AbilitiesInCombo.Tornado.TargetModifierName) != null;
            var damageFromTornado =
                willTakeDamageFromTornado ? _config.Main.AbilitiesInCombo.Tornado.GetDamage(enemy) : 0;
            var healthAfterCast = enemy.Health + enemy.HealthRegeneration * 2 - SunStrike.GetDamage(enemy) -
                                  damageFromTornado;
            if (!_damageDict.TryGetValue(enemy.Handle, out _))
                _damageDict.Add(enemy.Handle, (int) healthAfterCast);
            else
                _damageDict[enemy.Handle] = (int) healthAfterCast;
            heroWillDie = healthAfterCast <= 0;
            if (!heroWillDie) FlushTiming(enemy);
            return heroWillDie;
        }
    }
}