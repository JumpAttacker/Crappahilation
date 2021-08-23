using System;
using System.Collections.Generic;
using System.Linq;
using Divine.Entity;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units.Components;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Extensions;
using Divine.Game;
using Divine.GameConsole;
using Divine.Menu.Items;
using Divine.Numerics;
using Divine.Particle;
using Divine.Plugins.Humanizer;
using Divine.Prediction;
using Divine.Renderer;
using Divine.Update;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;
using O9K.Core.Entities.Abilities.Heroes.Invoker;
using O9K.Core.Entities.Heroes;
using O9K.Core.Entities.Units;
using O9K.Core.Helpers;
using O9K.Core.Managers.Entity;
using O9K.Core.Prediction.Data;

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
            var main = _config.Factory.CreateMenu("Auto SunStrike");
            Enable = main.CreateSwitcher("Enable", true);
            KillStealOnly = main.CreateSwitcher("Kill Steal Only", true);
            UseOnlyOnStunnedEnemies = main.CreateSwitcher("Only on stunned enemies", true);
            DrawPrediction = main.CreateSwitcher("Draw prediction", true);
            DrawPredictionKillSteal = main.CreateSwitcher("Draw prediction only when target will die after ss", true);
            InvokeSunStike = main.CreateSwitcher("Invoke sun strike", true);
            Notification = main.CreateSwitcher("Notification if target is Killable", true);
            SsTiming = main.CreateSlider("Timing for auto SunStrike (in ms)", 2000, 100, 3500);
            DamageSize = main.CreateSlider("Text size", 15, 10, 50);
            DamageX = main.CreateSlider("X", 0, -150, 150);
            DamageY = main.CreateSlider("Y", 0, -150, 150);

            DrawDamage = main.CreateSwitcher("Draw damage from SunStrike", true);
            MoveCamera = main.CreateSwitcher("Move camera", true);
            //DrawDamageType = main.Item("Type of drawing", new StringList("Only text","Icon + text"));

            _damageDict = new Dictionary<uint, int>();

            // if (Enable) Activate();

            // if (DrawDamage) RendererManager.Draw += OnDraw;

            DrawDamage.ValueChanged += (sender, args) =>
            {
                if (args.Value)
                    RendererManager.Draw += OnDraw;
                else
                    RendererManager.Draw -= OnDraw;
            };

            Enable.ValueChanged += (sender, args) =>
            {
                if (args.Value)
                    Activate();
                else
                    Deactivate();
            };

            var sizeY = Hud.Info.ScreenRatio * 6;

            HpBarSize = new Vector2(sizeY * 2.5f);
            HpBarY = sizeY;
        }

        public MenuSlider DamageY { get; set; }

        public MenuSlider DamageX { get; set; }

        public MenuSlider DamageSize { get; set; }

        public MenuSwitcher MoveCamera { get; set; }

        public MenuSwitcher DrawDamage { get; set; }

        public MenuSlider SsTiming { get; set; }

        public MenuSwitcher Notification { get; set; }

        public MenuSwitcher InvokeSunStike { get; set; }

        public MenuSwitcher DrawPredictionKillSteal { get; set; }

        public MenuSwitcher DrawPrediction { get; set; }

        public MenuSwitcher UseOnlyOnStunnedEnemies { get; set; }

        public MenuSwitcher KillStealOnly { get; set; }

        public MenuSwitcher Enable { get; set; }

        public float HpBarY { get; set; }

        public Vector2 HpBarSize { get; set; }

        public Hero Me => _config.Main.Me;


        public InvokerSunStrike SunStrike => _config.Main.AbilitiesInCombo.SunStrike;


        private void OnDraw()
        {
            var allEnemies = EntityManager.GetEntities<Hero>().Where(x =>
                x.IsValid && x.IsAlive && x.IsVisible && x.IsEnemy(Me) && !x.IsIllusion);
            foreach (var enemy in allEnemies)
            {
                var pos = HUDInfo.GetHpBarPosition(enemy);
                if (pos.IsZero)
                    continue;
                if (!Enable) UpdateDamage(enemy, out _);
                if (_damageDict.TryGetValue(enemy.Handle, out var damage))
                {
                    var fontSize = HpBarY * DamageSize / 10;
                    var mesText = RendererManager.MeasureText($"{damage}", "arial", fontSize);
                    RendererManager.DrawText($"{damage}", pos - new Vector2(mesText.X + 5 + DamageX.Value, 0 + DamageY), Color.White, fontSize);
                }
            }
        }

        public void Activate()
        {
            UpdateManager.CreateIngameUpdate(25, SunStrikeAction);
        }

        public void Deactivate()
        {
            UpdateManager.DestroyIngameUpdate(SunStrikeAction);
        }


        private void SunStrikeAction()
        {
            if (_config.ComboKey.Value) return;
            if (!Me.IsAlive || Me.IsSilenced())
                return;
            var allEnemies = EntityManager.GetEntities<Hero>().Where(x =>
                x.IsValid && x.IsEnemy(Me) && !x.IsIllusion);

            var enumerable = allEnemies as Hero[] ?? allEnemies.ToArray();
            if (!SunStrike.CanBeCasted())
                foreach (var enemy in enumerable)
                {
                    if (UpdateDamage(enemy, out _) && Notification && enemy.IsAlive)
                        _config.Main.NotificationHelper.Notificate(enemy, AbilityId.invoker_sun_strike, 0f);
                    else
                        _config.Main.NotificationHelper.Deactivate(enemy);
                    if (!SunStrike.CanBeCasted())
                        ParticleManager.RemoveParticle($"AutoSunStikePrediction{enemy.Handle}");
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
                    ParticleManager.RemoveParticle($"AutoSunStikePrediction{enemy.Handle}");
                    continue;
                }

                if (UpdateDamage(enemy, out var heroWillDie) || !KillStealOnly)
                {
                    if (Notification && heroWillDie)
                        _config.Main.NotificationHelper.Notificate(enemy, AbilityId.invoker_sun_strike, 0f);
                    // var stunned = UnitExtensions.IsStunned(enemy, out var stunDuration);
                    var o9KEnemy = EntityManager9.GetUnit(enemy.Handle);

                    var stunDuration = o9KEnemy.GetImmobilityDuration();
                    var invulDuration = o9KEnemy.GetInvulnerabilityDuration();
                    var isInvulnerable = o9KEnemy.IsInvulnerable;
                    var isStunned = o9KEnemy.IsStunned;
                    var immobile = stunDuration >= 1.5f; //_config.Main.AbilitiesInCombo.SunStrike.ActivationDelay;
                    // var invulModifier =
                    //     enemy.GetModifierByName("modifier_eul_cyclone") ??
                    //     enemy.GetModifierByName(_config.Main.AbilitiesInCombo.Tornado.TargetModifierName) ??
                    //     enemy.GetModifierByName("modifier_brewmaster_storm_cyclone") ??
                    //     enemy.GetModifierByName("modifier_shadow_demon_disruption") ??
                    //     enemy.GetModifierByName("modifier_obsidian_destroyer_astral_imprisonment_prison");

                    PredictionInput9 input = null;
                    PredictionOutput9 output = null;
                    if (DrawPrediction && (heroWillDie || !DrawPredictionKillSteal))
                    {
                        input = SunStrike.BaseAbility.GetPredictionInput(o9KEnemy);
                        output = SunStrike.BaseAbility.GetPredictionOutput(input);

                        ParticleManager.TargetLineParticle($"AutoSunStikePrediction{enemy.Handle}", enemy,
                            output.TargetPosition, CanSunStikerHit(o9KEnemy) ? Color.AliceBlue : Color.Red);
                    }
                    else
                    {
                        ParticleManager.RemoveParticle($"AutoSunStikePrediction{enemy.Handle}");
                    }

                    if (enemy.HasModifier(_config.Main.AbilitiesInCombo.ColdSnap.TargetModifierName) &&
                        !enemy.HasModifier(_config.Main.AbilitiesInCombo.Tornado.TargetModifierName))
                        continue;

                    if (!isStunned && !isInvulnerable)
                        if (UseOnlyOnStunnedEnemies)
                            continue;

                    if (isInvulnerable)
                    {
                        if (invulDuration <= 1.7f + GameManager.Ping * 0.75f / 1000f &&
                            invulDuration >= 1.0f)
                        {
                            CameraAction(enemy.Position);
                            SunStrike.UseAbility(enemy.Position);
                        }
                    }
                    else
                    {
                        if (input == null)
                        {
                            input = SunStrike.BaseAbility.GetPredictionInput(o9KEnemy);
                            output = SunStrike.BaseAbility.GetPredictionOutput(input);
                        }

                        if (output.HitChance is O9K.Core.Prediction.Data.HitChance.High or O9K.Core.Prediction.Data.HitChance.Medium or O9K.Core.Prediction.Data.HitChance.Low || immobile && output.HitChance == O9K.Core.Prediction.Data.HitChance.Immobile)
                        {
                            if (isStunned)
                            {
                                if (stunDuration >= 1.5f)
                                {
                                    CameraAction(enemy.Position);
                                    SunStrike.UseAbility(enemy.Position);
                                }
                            }
                            else if (heroWillDie && CanSunStrikeHitWithPrediction(enemy))
                            {
                            }
                        }
                    }
                }
                else
                {
                    _config.Main.NotificationHelper.Deactivate(enemy);
                    ParticleManager.RemoveParticle($"AutoSunStikePrediction{enemy.Handle}");
                }
            }
        }

        private void CameraAction(Vector3 enemyPosition)
        {
            if (MoveCamera)
            {
                // if (RendererManager.WorldToScreen(enemyPosition).IsZero)
                // {
                var consolePosition = $"{enemyPosition.X} {enemyPosition.Y}";
                GameConsoleManager.ExecuteCommand($"dota_camera_set_lookatpos {consolePosition}");
                // }
            }
        }

        private bool CanSunStrikeHitWithPrediction(Hero target)
        {
            if (!target.IsRotating() && target.IsMoving)
            {
                var num1 = target.MovementSpeed * 1.75f + GameManager.Ping / 1000f;
                var position = target.InFront(num1);
                var num2 = 0;
                // while (num2 < (double) num1)
                // {
                //     num2 += 64;
                //     _config.Main.NavMeshHelper.Pathfinding.GetCellPosition(target.InFront(num2), out var cellX,
                //         out var cellY);
                //     var flag =
                //         _config.Main.NavMeshHelper.Pathfinding.GetCell(cellX, cellY).NavMeshCellFlags;
                //
                //     if (CheckForFlags(flag)) continue;
                //     FlushTiming(target);
                //     return false;
                // }

                if (!CheckForTiming(target)) return false;
                CameraAction(position);
                SunStrike.UseAbility(position);
                FlushTiming(target);
                return true;
            }

            FlushTiming(target);
            return false;
        }

        private bool CanSunStikerHit(Unit9 target)
        {
            if (target.IsRotating) return false;
            var num1 = target.Speed * 1.75f + GameManager.Ping / 1000f;
            var num2 = 0;
            // while (num2 < (double) num1)
            // {
            //     num2 += 64;
            //     _config.Main.NavMeshHelper.Pathfinding.GetCellPosition(target.InFront(num2), out var cellX,
            //         out var cellY);
            //     var flag = _config.Main.NavMeshHelper.Pathfinding.GetCell(cellX, cellY).NavMeshCellFlags;
            //     if (!CheckForFlags(flag))
            //         return false;
            // }

            return true;
        }

        // private bool CheckForFlags(NavMeshCellFlags flag)
        // {
        //     return flag.HasFlag(NavMeshCellFlags.Walkable) &&
        //            !flag.HasFlag(NavMeshCellFlags.Tree) &&
        //            !flag.HasFlag(NavMeshCellFlags.GridFlagObstacle) &&
        //            !flag.HasFlag(NavMeshCellFlags.MovementBlocker);
        // }

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
            var currentTime = GameManager.RawGameTime;
            if (_timeDictionary.TryGetValue(handle, out var time))
            {
                timing = currentTime - time;
                //InvokerCrappahilationPaid.Log.Warn($"Timing: {currentTime - time}");
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
            if (_multiSleeper.IsSleeping(enemy.Handle))
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

            _multiSleeper.Sleep(enemy.Handle, .50f);
            var enemy9 = EntityManager9.GetUnit(enemy.Handle);
            if (enemy9 == null)
            {
                heroWillDie = false;
                return heroWillDie;
            }

            var willTakeDamageFromTornado =
                enemy.HasModifier(_config.Main.AbilitiesInCombo.Tornado.TargetModifierName);
            var damageFromTornado =
                willTakeDamageFromTornado ? _config.Main.AbilitiesInCombo.Tornado.BaseAbility.GetDamage(enemy9) : 0;
            var healthAfterCast = enemy.Health + enemy.HealthRegeneration * 2 - SunStrike.BaseAbility.GetDamage(enemy9) -
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