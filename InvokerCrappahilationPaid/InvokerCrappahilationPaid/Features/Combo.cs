using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Divine.Entity;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Extensions;
using Divine.Game;
using Divine.Modifier.Modifiers;
using Divine.Numerics;
using Divine.Particle;
using Divine.Prediction;
using Divine.Update;
using InvokerCrappahilationPaid.Extensions;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;
using O9K.Core.Entities.Abilities.Base;
using O9K.Core.Entities.Heroes;
using O9K.Core.Entities.Units;
using O9K.Core.Helpers;
using O9K.Core.Managers.Entity;
using HitChance = O9K.Core.Prediction.Data.HitChance;
using UnitExtensions = Divine.Extensions.UnitExtensions;
using Vector3Extensions = Divine.Extensions.Vector3Extensions;

namespace InvokerCrappahilationPaid.Features
{
    public class Combo
    {
        public enum ComboTypeEnum
        {
            Auto,
            CustomCombo
        }

        private const int CooldownOnAction = 750;


        private static bool _blocked;

        public static bool AfterRefresher;

        private readonly List<AbilityId> _freeAbilities = new List<AbilityId>
        {
            AbilityId.invoker_alacrity,
            AbilityId.invoker_cold_snap,
            //AbilityId.invoker_ice_wall,
            AbilityId.invoker_forge_spirit
        };

        private readonly Sleeper _invokerSleeper;
        private readonly InvokerCrappahilationPaid _main;
        private readonly MultiSleeper<string> _sleeper;

        public Combo(InvokerCrappahilationPaid main)
        {
            _main = main;
            _sleeper = new MultiSleeper<string>();
            _invokerSleeper = new Sleeper();
            var particleUpdateHandler = UpdateManager.CreateIngameUpdate(0, false, UpdateTargetParticle);
            var comboUpdateHandler = UpdateManager.CreateIngameUpdate(25, false, ComboInAction);
            Config.ComboKey.ValueChanged += (sender, args) =>
            {
                if (args.Value)
                {
                    // Console.WriteLine("Combo start");
                    comboUpdateHandler.IsEnabled = true;
                    particleUpdateHandler.IsEnabled = true;
                }
                else
                {
                    // Console.WriteLine("Combo end");
                    comboUpdateHandler.IsEnabled = false;
                    particleUpdateHandler.IsEnabled = false;
                    Target = null;
                    ParticleManager.RemoveParticle("TargetEffectLine");
                    try
                    {
                        if (_main.Config.ComboPanel.SelectedCombo != null)
                            _main.Config.ComboPanel.SelectedCombo.AbilityInAction = 0;
                    }
                    catch (Exception e)
                    {
                        // InvokerCrappahilationPaid.Log.Error(e);
                    }
                }
            };

            /*UpdateManager.Subscribe(() =>
            {
                var target = _main.Context.TargetSelector.Active.GetTargets().FirstOrDefault();
                if (target != null)
                {
                    var pos = GetIceWallPos((Hero) target);
                    _main.Context.Particle.DrawDangerLine(Me, "qwe", pos);
                }
            });*/

            /*UpdateManager.Subscribe(() =>
            {
                var target = _main.Context.TargetSelector.Active.GetTargets().FirstOrDefault();
                if (target != null)
                    Console.WriteLine($"Angl: {GetDif(Me, target.Position)}");
            },100);*/

            /*_main.Context.Input.RegisterHotkey("1", 'Z', args =>
            {
                

                foreach (var abilityId in Abilities.Tornado.RequiredOrbs)
                {
                    _main.Context.AbilityFactory.GetAbility(abilityId).Ability.UseAbility();
                }
                Abilities.Invoke.Ability.UseAbility();
                /*Abilities.Meteor.Invoke(skip:true);
                Abilities.Tornado.Invoke();
            });*/
            /*_main.Context.Input.RegisterHotkey("2", 'X', args =>
            {
                Abilities.Blast.Invoke(skip: true);
            });*/
        }

        private Config Config => _main.Config;
        public int ExtraMeteorPosition => 150;

        // private IParticleManager ParticleManager => _main.Context.Particle;

        private AbilitiesInCombo Abilities => _main.AbilitiesInCombo;
        private uint ExortLevel => Abilities.Exort.BaseAbility.Level;
        private uint WexLevel => Abilities.Wex.BaseAbility.Level;
        private uint QuasLevel => Abilities.Quas.BaseAbility.Level;

        private ComboTypeEnum GameplayType =>
            _main.Config.ComboPanel.IsAutoComboSelected ? ComboTypeEnum.Auto : ComboTypeEnum.CustomCombo;

        public Unit Target { get; set; }
        private Hero Me => (Hero) _main.Me;
        private Hero9 Me9 => _main.Me9;

        /*private void InvokeThisShit(ActiveAbility ability)
        {
            if (Abilities.Invoke.CanBeCasted)
            {
                var requiredOrbs = (ability as IInvokableAbility)?.RequiredOrbs;
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        _main.Context.AbilityFactory.GetAbility(abilityId).Ability.UseAbility();
                        InvokerCrappahilationPaid.Log.Warn($"Invoke [Sphere: {abilityId}]");
                    }
                    Abilities.Invoke.Ability.UseAbility();
                    InvokerCrappahilationPaid.Log.Warn($"Invoke [Invoke]");
                }
                else
                {
                    InvokerCrappahilationPaid.Log.Error($"Error in Invoke function: {ability.Ability.Id}");
                }
            }
        }*/

        public bool InvokeThisShit(InvokerBaseAbility ability)
        {
            // Console.WriteLine($"Trying to invoke -> {ability.Id}");
            if (_sleeper.IsSleeping($"{ability} shit"))
            {
                // Console.WriteLine($"Invoke [blocked] ({ability})");
                return false;
            }

            if (Abilities.Invoke.BaseAbility.CanBeCasted())
            {
                var requiredOrbs = (ability as IInvokableAbility)?.RequiredOrbs;
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        var sphere = _main.AbilitiesInCombo.Spheres.FirstOrDefault(z => z.BaseAbility.Id == abilityId);
                        if (sphere == null) return false;

                        if (!sphere.BaseAbility.BaseAbility.Cast()) return false;

                        // Console.WriteLine($"Invoke [Sphere: {abilityId}] ({ability})");
                    }

                    var invoked = Abilities.Invoke.BaseAbility.BaseAbility.Cast();
                    if (invoked)
                    {
                        _sleeper.Sleep($"{ability} shit", .200f);
                        // Console.WriteLine($"invoked [{ability}]");
                    }

                    return invoked;
                }

                Console.WriteLine($"Error in Invoke function: {ability.Id}");
                return false;
            }

            // Console.WriteLine($"Invoke [on cd] ({ability})");
            return false;
        }

        private void UpdateTargetParticle()
        {
            if (Target == null || !Target.IsValid || !Target.IsVisible || Me == null || !Me.IsValid || !Me.IsAlive)
            {
                ParticleManager.RemoveParticle("TargetEffectLine");
                return;
            }

            ParticleManager.TargetLineParticle("TargetEffectLine", Me, Target.Position, Color.YellowGreen);
        }

        private void ComboInAction()
        {
            if (!GetTarget())
            {
                if (_sleeper.IsSleeping("moving"))
                    return;

                _sleeper.Sleep("moving", .125f);

                var mousePos = GameManager.MousePosition;
                //TODO: orbwalk actions
                // if (_main.Context.Orbwalker.Active.CanMove())
                // _main.Context.Orbwalker.Active.Move(mousePos);
                if (Me9.CanMove())
                    Me9.Move(mousePos);

                /*foreach (var unit in _main.Updater.Units.Where(x=> x.Unit != null && x.Unit.IsValid && x.CanWork && x.Unit.IsAlive))
                {
                    if (unit.Orbwalker.Active.CanMove())
                        unit.Orbwalker.Active.Move(mousePos);
                }*/

                foreach (var unit in _main.Updater.Units
                    .Where(x => x.Unit != null && x.Unit.IsValid && x.CanWork && x.Unit.IsAlive).Select(z => z.Unit))
                    unit.Move(mousePos);

                return;
            }

            var isInvul = Target.IsInvulnerable();
            if (!_sleeper.IsSleeping("Orbwalker"))
            {
                // _sleeper.Sleep( "Orbwalker", 250);
                /*foreach (var orbwalker in _main.Updater.Units
                    .Where(x => x.Unit != null && x.Unit.IsValid && x.CanWork && x.Unit.IsAlive)
                    .Select(z => z.Orbwalker.Active))
                {
                    if (orbwalker.CanAttack(Target))
                        orbwalker.Attack(Target);
                    
                }*/
                // foreach (var unit in _main.Updater.Units
                //     .Where(x => x.Unit != null && x.Unit.IsValid && x.CanWork && x.Unit.IsAlive)
                //     .Select(z => z.Unit))
                // {
                //     if (!_sleeper.IsSleeping($"archerSlow{unit.Handle}") &&
                //         unit.Name.Contains("npc_dota_necronomicon_archer"))
                //     {
                //         var ability = unit.Spellbook.Spell1;
                //         if (ability.() && ability.CanHit(Target) && !UnitExtensions.IsStunned(Target) &&
                //             Target.MovementSpeed > 280 &&
                //             !UnitExtensions.IsMagicImmune(Target))
                //         {
                //             ability.UseAbility(Target);
                //             _sleeper.Sleep(500, $"archerSlow{unit.Handle}");
                //         }
                //     }
                //
                //     if (isInvul)
                //         unit.Move(Target.Position);
                //     else if (!unit.IsAttacking())
                //         unit.Attack(Target);
                // }
            }

            Modifier tornadoModifier;
            if (InvokerIceWall.InAction)
                return;
            float stunDuration;
            bool isStunned;
            Unit9 target9;
            switch (GameplayType)
            {
                case ComboTypeEnum.Auto:

                    #region AutoCombo

                    if (_sleeper.IsSleeping("CooldownOnAction"))
                        return;
                    if (_sleeper.IsSleeping("Eul") /*|| _invokerSleeper.Sleeping*/)
                        return;

                    tornadoModifier = Target.GetFirstValidModifier("modifier_eul_cyclone",
                        "modifier_obsidian_destroyer_astral_imprisonment_prison",
                        "modifier_shadow_demon_disruption",
                        Abilities.Tornado.TargetModifierName, "modifier_brewmaster_storm_cyclone");
                    target9 = EntityManager9.GetUnit(Target.Handle);
                    if (tornadoModifier == null && !_sleeper.IsSleeping("AfterRefresh"))
                    {
                        stunDuration = target9.GetImmobilityDuration();
                        isStunned = target9.IsStunned;
                        var hasModifiers = target9.HasModifier(Abilities.IceWall.TargetModifierName,
                            Abilities.ColdSnap.TargetModifierName,
                            Abilities.Meteor.TargetModifierName, "modifier_bloodthorn_debuff", "modifier_orchid_malevolence_debuff");
                        var allFineWithTarget = (!isStunned || stunDuration <= 0.5f) /* &&
                                                (!Target.HasAnyModifiers("modifier_bloodthorn_debuff",
                                                    "modifier_orchid_malevolence_debuff")) */ &&
                                                (target9.IsHexed || stunDuration <= 0.5f) &&
                                                !hasModifiers &&
                                                !CheckForEmpNearTarget(target9);
                        if (Abilities.Eul != null && Abilities.Eul.CanBeCasted() && allFineWithTarget && Config.UseEul)
                        {
                            var makesSensesToCastEul =
                                Abilities.SunStrike.BaseAbility.CanBeCasted() || Abilities.Meteor.CanBeCasted() ||
                                WexLevel >= 4 && Abilities.Emp.BaseAbility.CanBeCasted() /*||
                                                   Abilities.IceWall.CanBeCasted*/;

                            if (makesSensesToCastEul)
                            {
                                if (!Abilities.Eul.CanHit(target9))
                                {
                                    //TODO: orbwalk
                                    // if (_main.Context.Orbwalker.Active.CanMove())
                                    // _main.Context.Orbwalker.Active.Move(Target.Position);
                                    break;
                                }

                                Abilities.Eul.UseAbility(target9);
                                _sleeper.Sleep("Eul", Abilities.Eul.GetHitTime(target9) + .250f);
                                return;
                            }
                        }
                        else if (allFineWithTarget && QuasLevel >= 4 && Abilities.Tornado.BaseAbility.CanBeCasted() &&
                                 (Abilities.SunStrike.BaseAbility.CanBeCasted() || Abilities.Meteor.BaseAbility.CanBeCasted() ||
                                  Abilities.Tornado.BaseAbility.GetDamage(target9) >
                                  Target.Health + Target.HealthRegeneration * (Abilities.Tornado.BaseAbility.Duration + 1) ||
                                  WexLevel >= 4 && Abilities.Emp.BaseAbility.CanBeCasted()) && Abilities.Tornado.BaseAbility.CanHit(target9) &&
                                 Target.IsInRange(Me, 1000))
                        {
                            // InvokerCrappahilationPaid.Log.Debug(
                            // $"[Use] [{Abilities.Tornado}] TargetHealth (with regen prediction): {Target.Health + Target.HealthRegeneration * (Abilities.Tornado.Duration + 1)} Damage: {Abilities.Tornado.GetDamage(Target)}");
                            var input = Abilities.Tornado.BaseAbility.GetPredictionInput(target9);
                            var output = Abilities.Tornado.BaseAbility.GetPredictionOutput(input);
                            if (output.HitChance is HitChance.High or HitChance.Medium or HitChance.Low)
                            {
                                if (Abilities.Tornado.IsInvoked)
                                {
                                    //Abilities.Tornado.SafeInvoke(Abilities.SunStrike, Abilities.Meteor);
                                    var casted = Abilities.Tornado.BaseAbility.BaseAbility.Cast(output.TargetPosition);
                                    if (casted)
                                    {
                                        var delay = (float) Abilities.Tornado.BaseAbility.GetHitTime(target9);
                                        // var arrivalTime = output.;
                                        // InvokerCrappahilationPaid.Log.Warn(
                                        // $"[Use][{Abilities.Tornado}] [Delay: {delay}] [ArrivalTime: {arrivalTime}]");

                                        _sleeper.Sleep("Eul", delay * 1 + .500f);
                                        _sleeper.Sleep("PussyCaster", delay * 1 + 0.5f);
                                        return;
                                    }
                                }
                                else
                                {
                                    if (Abilities.SunStrike.BaseAbility.CanBeCasted() && Abilities.SunStrike.IsInvoked &&
                                        Abilities.SunStrike.BaseAbility.AbilitySlot == AbilitySlot.Slot5 &&
                                        !_sleeper.IsSleeping($"Invoked {Abilities.SunStrike}"))
                                    {
                                        if (InvokeThisShit(Abilities.SunStrike))
                                        {
                                            //Abilities.SunStrike.Invoke(skip: true);
                                            _sleeper.Sleep("Eul", .150f);
                                            _sleeper.Sleep($"Invoked {Abilities.SunStrike}", .150f);
                                            //Abilities.Tornado.Invoke(skip: true);
                                        }
                                    }
                                    else if (Abilities.Meteor.CanBeCasted() && Abilities.Meteor.IsInvoked &&
                                             Abilities.Meteor.BaseAbility.AbilitySlot == AbilitySlot.Slot5 &&
                                             !_sleeper.IsSleeping($"Invoked {Abilities.Meteor}"))
                                    {
                                        if (InvokeThisShit(Abilities.Meteor))
                                        {
                                            //Abilities.Meteor.Invoke(skip: true);
                                            _sleeper.Sleep("Eul", .150f);
                                            _sleeper.Sleep($"Invoked {Abilities.Meteor}", .150f);
                                            //Abilities.Tornado.Invoke(skip: true);
                                        }
                                    }
                                    else if (WexLevel >= 4 && Abilities.Emp.BaseAbility.CanBeCasted() && Abilities.Emp.IsInvoked &&
                                             Abilities.Emp.BaseAbility.AbilitySlot == AbilitySlot.Slot5 &&
                                             !_sleeper.IsSleeping($"Invoked {Abilities.Emp}"))
                                    {
                                        if (InvokeThisShit(Abilities.Emp))
                                        {
                                            _sleeper.Sleep("Eul", .150f);
                                            _sleeper.Sleep($"Invoked {Abilities.Emp}", .150f);
                                        }
                                    }

                                    InvokeThisShit(Abilities.Tornado);
                                    return;
                                    /*
                                    var casted = Abilities.Tornado.UseAbility(output.UnitPosition);
                                    if (casted)
                                    {
                                        InvokerCrappahilationPaid.Log.Warn($"[Use][{Abilities.Tornado}]");
                                        _sleeper.Sleep(output.ArrivalTime + 150, "Eul");
                                        _sleeper.Sleep(output.ArrivalTime + 500, "PussyCaster");
                                        return;
                                    }*/
                                }
                            }
                        }
                        else
                        {
                            var abilities = GetAvilableAbilities().ToList();
                            if (abilities.Any())
                            {
                                foreach (var baseAbility in abilities)
                                {
                                    var ability = baseAbility;
                                    if (ability is InvokerIceWall iceWall)
                                    {
                                        if (Target.IsInRange(Me, 550) &&
                                            (target9.IsStunned || Target.MovementSpeed <= 425f))
                                        {
                                            var casted = iceWall.CastAsync(Target);
                                            //_sleeper.Sleep("Eul", .500f);
                                            return;
                                        }
                                        // if (Me9.CanMove())
                                        // {
                                        //     // Me9.Move(target9);
                                        //     return;
                                        // }
                                    }
                                    else
                                    {
                                        ability.UseAbility(ability.Id == AbilityId.invoker_alacrity
                                            ? Me9
                                            : target9);
                                    }
                                }

                                return;
                            }
                        }
                    }

                    if (tornadoModifier != null && !_sleeper.IsSleeping("AfterRefresh"))
                    {
                        if (Abilities.ForgeSpirit.BaseAbility.CanBeCasted() && Abilities.ForgeSpirit.IsInvoked &&
                            Me.IsInAttackRange(Target))
                            Abilities.ForgeSpirit.BaseAbility.UseAbility();

                        if (Abilities.Alacrity.BaseAbility.CanBeCasted() && Abilities.Alacrity.IsInvoked &&
                            Me.IsInAttackRange(Target) &&
                            !Me.HasModifier(Abilities.Alacrity.ModifierName))
                            Abilities.Alacrity.UseAbility(Me9, false, false);

                        var empChecker = Me9.ManaPercentage > 50 || !Abilities.Emp.CanBeCasted();
                        if (Abilities.SunStrike.BaseAbility.BaseAbility.CanBeCasted() && empChecker)
                        {
                            if (Abilities.SunStrike.IsInvoked)
                            {
                                if (tornadoModifier.RemainingTime <= Abilities.SunStrike.BaseAbility.ActivationDelay)
                                {
                                    if (tornadoModifier.RemainingTime >= Abilities.SunStrike.BaseAbility.ActivationDelay - 0.5f)
                                    {
                                        var countForSs = Config.UseCataclysm.Value;
                                        if (!Abilities.SunStrike.IsCataclysmActive || countForSs == 0 ||
                                            !CheckForCataclysm(countForSs))
                                        {
                                            Abilities.SunStrike.BaseAbility.BaseAbility.Cast(Target.Position);
                                        }
                                        else
                                        {
                                            Abilities.SunStrike.BaseAbility.BaseAbility.Cast(Me);
                                        }
                                    }
                                }
                                else
                                {
                                    if (Abilities.SunStrike.AbilitySlot == AbilitySlot.Slot4)
                                    {
                                        if (Abilities.Emp.AbilitySlot != AbilitySlot.Slot5)
                                        {
                                            if (Abilities.Meteor.CanBeCasted())
                                                InvokeThisShit(Abilities.Meteor);
                                            else if (Abilities.Blast.CanBeCasted())
                                                InvokeThisShit(Abilities.Blast);
                                            else if (WexLevel >= 4 && Abilities.Emp.CanBeCasted())
                                                InvokeThisShit(Abilities.Emp);
                                        }

                                        //Abilities.Blast.Invoke(skip: true);
                                    }
                                    else
                                    {
                                        if (!Abilities.Meteor.IsInvoked) InvokeThisShit(Abilities.SunStrike);

                                        return;
                                    }
                                }
                            }
                            else if (Abilities.Invoke.IsReady)
                            {
                                if (Me9.HasAghanimShard || Me9.HasAghanimsScepter)
                                {
                                    if (Abilities.Blast.BaseAbility.AbilitySlot == AbilitySlot.Slot5)
                                    {
                                        if (Abilities.Wex.Level >= 4 && Abilities.Wex.BaseAbility.CanBeCasted())
                                            InvokeThisShit(Abilities.Emp);
                                        else if (Abilities.Blast.CanBeCasted()) InvokeThisShit(Abilities.Blast);
                                    
                                        InvokeThisShit(Abilities.SunStrike);
                                    }
                                    else if (Abilities.Meteor.BaseAbility.AbilitySlot == AbilitySlot.Slot5)
                                    {
                                        if (Abilities.Meteor.CanBeCasted())
                                            InvokeThisShit(Abilities.Meteor);
                                        else if (Abilities.Wex.Level >= 4 && Abilities.Emp.CanBeCasted())
                                            InvokeThisShit(Abilities.Emp);
                                    
                                        InvokeThisShit(Abilities.SunStrike);
                                    }
                                    else
                                    {
                                        Abilities.SunStrike.Invoke();
                                    }
                                }
                                else
                                {
                                    if (Abilities.Meteor.AbilitySlot == AbilitySlot.Slot5)
                                    {
                                        InvokeThisShit(Abilities.Meteor);
                                        InvokeThisShit(Abilities.SunStrike);
                                    }
                                    else
                                    {
                                        Abilities.SunStrike.Invoke();
                                    }
                                }
                            }
                        }
                        if (Abilities.Meteor.BaseAbility.BaseAbility.CanBeCasted() && Abilities.Meteor.BaseAbility.CanHit(target9) && empChecker)
                        {
                            if (tornadoModifier.RemainingTime <= Abilities.Meteor.BaseAbility.ActivationDelay)
                                if (tornadoModifier.RemainingTime >= Abilities.Meteor.BaseAbility.ActivationDelay - 0.5f)
                                {
                                    var targetPos = Target.Position.Extend(Me.Position, ExtraMeteorPosition);
                                    Abilities.Meteor.BaseAbility.BaseAbility.Cast(targetPos);
                                }
                        }
                        else if (Abilities.Blast.BaseAbility.BaseAbility.CanBeCasted() && Abilities.Blast.BaseAbility.CanHit(target9) && empChecker && !_sleeper.IsSleeping("Blasted"))
                        {
                            var hitTime = Math.Max((Abilities.Blast.BaseAbility.GetHitTime(target9) - 100) / 1000f, 0.1);
                            if (tornadoModifier.RemainingTime <= hitTime)
                            {
                                Abilities.Blast.UseAbility(Target.Position);
                                _sleeper.Sleep("Blasted", .750f);
                            }
                        }
                        else if (WexLevel >= 4 && Abilities.Emp.CanBeCasted() && Abilities.Emp.BaseAbility.CanHit(target9))
                        {
                            if (tornadoModifier.RemainingTime <= Abilities.Emp.BaseAbility.ActivationDelay)
                                Abilities.Emp.UseAbility(Target.Position);
                        }
                        else if (Config.UseIceWall && Abilities.IceWall.BaseAbility.CanBeCasted() && Me.IsInRange(Target, 550) &&
                                 (target9.IsStunned || Target.MovementSpeed <= 425f || target9.IsInvulnerable) &&
                                 Abilities.IceWall.Invoke() /* && !Me.IsInRange(Target, 115) &&
                                 Me.IsDirectlyFacing(Target) IsDirectlyFacing(Me,Target.Position,0.065f)*/)
                        {
                            if (Abilities.Invoke.BaseAbility.CanBeCasted())
                            {
                                var asyncCasted = Abilities.IceWall.CastAsync(Target);
                                //_sleeper.Sleep(CooldownOnAction, "CooldownOnAction");
                                return;
                            }
                        }
                    }
                    else if (!_sleeper.IsSleeping("PussyCaster") && !_sleeper.IsSleeping("Casted"))
                    {
                        stunDuration = target9.GetImmobilityDuration();
                        isStunned = target9.IsStunned;
                        Console.WriteLine($"1: {Config.UseIceWall} {Abilities.IceWall.CanBeCasted()} {Me.IsInRange(Target, 550)}");
                        if (!_sleeper.IsSleeping("Blasted") && Abilities.Blast.CanBeCasted() && Abilities.Blast.BaseAbility.CanHit(target9) &&
                            (stunDuration > Abilities.Blast.BaseAbility.GetHitTime(target9) ||
                             target9.HasModifier(Abilities.Meteor.TargetModifierName)))
                        {
                            Console.WriteLine("blast");
                            if (Abilities.Blast.UseAbility(Target.Position))
                            {
                                _sleeper.Sleep("Casted", .250f);
                                _sleeper.Sleep("Blasted", .750f);
                            }
                        }
                        else if (Abilities.Meteor.CanBeCasted() && Abilities.Meteor.BaseAbility.CanHit(target9) &&
                                 (stunDuration > Abilities.Meteor.BaseAbility.ActivationDelay ||
                                  Target.HasAnyModifiers(Abilities.IceWall.TargetModifierName,
                                      "modifier_invoker_deafening_blast_knockback",
                                      "modifier_invoker_cold_snap_freeze")))
                        {
                            var predPos = Target.IsMoving && !Target.IsRotating()
                                ? Target.InFront(150)
                                : Target.Position;
                            Console.WriteLine("meteor");
                            if (Abilities.Meteor.UseAbility(predPos))
                                _sleeper.Sleep("Casted", .250f);
                        }
                        else if (Config.UseIceWall && Abilities.IceWall.CanBeCasted() && Me.IsInRange(Target, 550) &&
                                 Abilities.IceWall.Invoke() /*&&
                                 !Me.IsInRange(Target, 115) && IsDirectlyFacing(Me,Target.Position,0.065f)*/
                            /*Me.IsDirectlyFacing(Target)*/)
                        {
                            Console.WriteLine("ice");
                            // if (Abilities.Invoke.BaseAbility.CanBeCasted())
                            // {
                                var asyncCasted = Abilities.IceWall.CastAsync(Target);
                                //_sleeper.Sleep(CooldownOnAction, "CooldownOnAction");
                                return;
                            // }
                        }
                        else if (Abilities.SunStrike.CanBeCasted() && Abilities.SunStrike.BaseAbility.CanHit(target9) && 
                                 (
                                     (Abilities.SunStrike.IsCataclysmActive && CheckForCataclysm(1)) || (
                                         (stunDuration > Abilities.SunStrike.BaseAbility.ActivationDelay || Target.HasAnyModifiers("modifier_invoker_cold_snap_freeze")) && (stunDuration > Abilities.SunStrike.BaseAbility.ActivationDelay ||
                                             Target.HasAnyModifiers("modifier_invoker_cold_snap_freeze") &&
                                             Target.MovementSpeed <= 280)
                                         )))
                        {
                            Console.WriteLine("ss");
                            if (Abilities.SunStrike.IsCataclysmActive && CheckForCataclysm(1))
                            {
                                if (!Abilities.SunStrike.IsInvoked)
                                    Abilities.SunStrike.Invoke();
                                if (Abilities.SunStrike.BaseAbility.BaseAbility.Cast(Me))
                                    _sleeper.Sleep("Casted", .250f);
                            }
                            else if ((stunDuration > Abilities.SunStrike.BaseAbility.ActivationDelay ||
                                      Target.HasAnyModifiers("modifier_invoker_cold_snap_freeze")))
                            {
                                if (stunDuration > Abilities.SunStrike.BaseAbility.ActivationDelay)
                                {
                                    if (Abilities.SunStrike.UseAbility(Target.Position))
                                        _sleeper.Sleep("Casted", .250f);
                                }
                                else if (Target.HasAnyModifiers("modifier_invoker_cold_snap_freeze") &&
                                         Target.MovementSpeed <= 280)
                                {
                                    var predictPos =
                                        Target.InFront(
                                            Target.IsMoving
                                                ? Target.MovementSpeed / 2f * 1.9f
                                                : Target.MovementSpeed / 2f * 0.8f);

                                    if (Abilities.SunStrike.UseAbility(predictPos))
                                        _sleeper.Sleep("Casted", .250f);
                                }
                            }
                        }
                        else if (Abilities.ColdSnap.CanBeCasted() && Abilities.ColdSnap.BaseAbility.CanHit(target9))
                        {
                            Console.WriteLine("ColdSnap");
                            if (Abilities.ColdSnap.UseAbility(target9))
                                _sleeper.Sleep("Casted", .250f);
                        }
                        else if (WexLevel >= 4 && Abilities.Emp.CanBeCasted() &&
                                 (Target.HasAnyModifiers(Abilities.IceWall.TargetModifierName, Abilities.Tornado.TargetModifierName,
                                     Abilities.ColdSnap.TargetModifierName) || EmpCheckForUnits(Target)))
                        {
                            Console.WriteLine("emp");
                            if (Abilities.Emp.UseAbility(Target.Position))
                                _sleeper.Sleep("Casted", .250f);
                        }
                        else if (Abilities.ForgeSpirit.CanBeCasted() && Me.IsInAttackRange(Target))
                        {
                            Console.WriteLine("forge");
                            if (Abilities.ForgeSpirit.UseAbility())
                                _sleeper.Sleep("Casted", .250f);
                        }
                        else if (Abilities.Alacrity.CanBeCasted() && Me.IsInAttackRange(Target) &&
                                 !Me.HasModifier(Abilities.Alacrity.ModifierName))
                        {
                            Console.WriteLine("alacrity");
                            if (Abilities.Alacrity.UseAbility(Me9, false, false))
                                _sleeper.Sleep("Casted", .250f);
                        }

                        if ((_main.Config.RefresherBehavior.Value == "After Meteor+Blast" ||
                             _main.Config.RefresherBehavior.Value == "In both cases") &&
                            Abilities.Meteor.BaseAbility.BaseAbility.AbilityState == AbilityState.OnCooldown &&
                            Abilities.Blast.BaseAbility.BaseAbility.AbilityState == AbilityState.OnCooldown)
                        {
                            if (Abilities.Refresher != null && Abilities.Refresher.CanBeCasted() &&
                                Me.IsInAttackRange(Target))
                            {
                                Abilities.Refresher.UseAbility();
                                _sleeper.Sleep("AfterRefresh", .1000f);
                                return;
                            }

                            if (Abilities.RefresherShard != null && Abilities.RefresherShard.CanBeCasted() &&
                                Me.IsInAttackRange(Target))
                            {
                                Abilities.RefresherShard.UseAbility();
                                _sleeper.Sleep("AfterRefresh", .1000f);
                                return;
                            }
                        }
                    }

                    break;

                #endregion

                case ComboTypeEnum.CustomCombo:

                    #region CustomCombo

                    if (_sleeper.IsSleeping("CooldownOnAction") /*|| _invokerSleeper.Sleeping*/)
                        return;
                    if (_sleeper.IsSleeping("Eul") /*|| _invokerSleeper.Sleeping*/)
                        return;
                    target9 = EntityManager9.GetUnit(Target.Handle);
                    stunDuration = target9.GetImmobilityDuration();
                    isStunned = target9.IsStunned;
                    var combo = _main.Config.ComboPanel.SelectedCombo;
                    var allAbilities = combo.Items.ToArray();

                    var abilityInAction =
                        _main.AbilitiesInCombo.AllAbilities.Find(x =>
                            x.Id == allAbilities[combo.AbilityInAction]);
                    if (abilityInAction != null)
                    {
                        if (!_sleeper.IsSleeping("Refresh") && abilityInAction.BaseAbility.RemainingCooldown >= 2)
                        {
                            // InvokerCrappahilationPaid.Log.Warn(
                            // $"Skip Ability Cuz cant invoke or CD -> {abilityInAction} {abilityInAction.Ability.Cooldown} InvokeCooldown: {Abilities.Invoke.Ability.Cooldown}");
                            IncComboStage(combo, true);
                            goto After;
//                                return;
                        }

                        tornadoModifier = Target.GetFirstValidModifier("modifier_eul_cyclone",
                            "modifier_obsidian_destroyer_astral_imprisonment_prison",
                            "modifier_shadow_demon_disruption",
                            Abilities.Tornado.TargetModifierName, "modifier_brewmaster_storm_cyclone");
                        bool casted;
                        switch (abilityInAction)
                        {
                            case InvokerAlacrity ability:
                                casted = ability.UseAbility(Me9, false, false);
                                IncComboStage(combo, casted);
                                break;
                            case InvokerChaosMeteor ability:
                                if (ability.BaseAbility.CanHit(target9))
                                {
                                    if (tornadoModifier != null && tornadoModifier.RemainingTime <=
                                        Abilities.Meteor.BaseAbility.ActivationDelay ||
                                        Target.HasAnyModifiers(Abilities.ColdSnap.TargetModifierName,
                                            Abilities.IceWall.TargetModifierName,
                                            Abilities.Blast.TargetModifierName) &&
                                        !Target.HasAnyModifiers(Abilities.Tornado.TargetModifierName))
                                    {
                                        var targetPos = Target.Position.Extend(Me.Position, ExtraMeteorPosition);
                                        casted = ability.UseAbility(targetPos);
                                        // InvokerCrappahilationPaid.Log.Error($"[{ability}] Casted: {casted}");
                                        IncComboStage(combo, casted);
                                    }
                                    else
                                    {
                                        if (!ability.IsInvoked)
                                            if (tornadoModifier != null)
                                                ability.Invoke();
                                    }
                                }

                                break;
                            case InvokerEmp ability:
                                if (ability.BaseAbility.CanHit(target9))
                                {
                                    if (tornadoModifier != null &&
                                        tornadoModifier.RemainingTime <= Abilities.Emp.BaseAbility.ActivationDelay ||
                                        Target.HasAnyModifiers(Abilities.IceWall.TargetModifierName,
                                            Abilities.ColdSnap.TargetModifierName,
                                            Abilities.Tornado.TargetModifierName,
                                            Abilities.Blast.TargetModifierName) || EmpCheckForUnits(Target))
                                    {
                                        if (ability.CanBeCasted())
                                        {
                                            casted = ability.UseAbility(Target.Position);
                                            IncComboStage(combo, casted);
                                        }
                                    }
                                    else
                                    {
                                        if (!ability.IsInvoked)
                                            if (tornadoModifier != null)
                                                ability.Invoke();
                                    }
                                }

                                break;
                            case InvokerDeafeningBlast ability:
                                //TODO check
                                if (ability.BaseAbility.CanHit(target9))
                                {
                                    var hitTime = Math.Max((Abilities.Blast.BaseAbility.GetHitTime(target9) - 100) / 1000f, 0.1);
                                    if (tornadoModifier != null && tornadoModifier.RemainingTime <= hitTime ||
                                        Target.HasAnyModifiers(Abilities.IceWall.TargetModifierName,
                                            Abilities.ColdSnap.TargetModifierName,
                                            Abilities.Meteor.TargetModifierName) &&
                                        !Target.HasAnyModifiers(Abilities.Tornado.TargetModifierName))
                                    {
                                        if (ability.CanBeCasted())
                                        {
                                            casted = ability.UseAbility(Target.Position);
                                            IncComboStage(combo, casted);
                                        }
                                    }
                                    else
                                    {
                                        if (!ability.IsInvoked)
                                            if (tornadoModifier != null)
                                                ability.Invoke();
                                    }
                                }

                                break;
                            case InvokerForgeSpirit ability:
                                if (ability.CanBeCasted())
                                {
                                    casted = ability.UseAbility();
                                    IncComboStage(combo, casted);
                                }

                                break;
                            case InvokerIceWall ability:
                                /*if (ability.CanBeCasted)
                                    casted = CastIceWall();
                                return;
                                break;*/
                                if (ability.CanBeCasted() && Me.IsInRange(Target, 550) && ability.Invoke())
                                {
                                    if (InvokerIceWall.InAction)
                                        return;
                                    var castedAsync = ability.CastAsync(Target);
                                    //_sleeper.Sleep(CooldownOnAction, "CooldownOnAction");
                                    IncComboStage(combo, true);
                                }
                                // if (Me9.CanMove())
                                // {
                                //     Me9.Move(target9);
                                //     return;
                                // }

                                break;
                                if (ability.CanBeCasted() && Me.IsInRange(Target, 250) &&
                                    !Me.IsInRange(Target, 115) &&
                                    IsDirectlyFacing(Me, Target.Position, 0.065f))
                                {
                                    casted = ability.UseAbility();
                                    IncComboStage(combo, casted);
                                }

                                break;
                            case InvokerTornado ability:
                                var input = Abilities.Tornado.BaseAbility.GetPredictionInput(target9);
                                var output = Abilities.Tornado.BaseAbility.GetPredictionOutput(input);
                                // Console.WriteLine($"HitChance: {output.HitChance}");
                                if (output.HitChance is HitChance.Medium or HitChance.High or HitChance.Low)
                                {
                                    casted = ability.UseAbility(output.TargetPosition);
                                    IncComboStage(combo, casted);
                                    if (casted)
                                    {
                                        var delay = (float) Abilities.Tornado.BaseAbility.GetHitTime(target9);
                                        // var arrivalTime = output.ArrivalTime;
                                        // InvokerCrappahilationPaid.Log.Warn(
                                        //     $"[Use][{Abilities.Tornado}] [Delay: {delay}] [ArrivalTime: {arrivalTime}]");

                                        _sleeper.Sleep("Eul", delay * 1 + .5f);
                                        _sleeper.Sleep("PussyCaster", delay * 1 + .5f);
                                        return;
                                    }
                                }

                                break;
                            case InvokerColdSnap ability:
                                if (ability.BaseAbility.CanHit(target9))
                                {
                                    casted = ability.UseAbility(target9);
                                    IncComboStage(combo, casted);
                                }

                                break;
                            case InvokerSunStrike ability:
                                if (tornadoModifier != null && tornadoModifier.RemainingTime <=
                                    Abilities.SunStrike.BaseAbility.ActivationDelay || Target.HasAnyModifiers(
                                        Abilities.IceWall.TargetModifierName,
                                        Abilities.ColdSnap.TargetModifierName,
                                        Abilities.Meteor.TargetModifierName) &&
                                    !Target.HasAnyModifiers(Abilities.Tornado
                                        .TargetModifierName))
                                {
                                    if (ability.CanBeCasted())
                                    {
                                        var countForSs = Config.UseCataclysm.Value;
                                        if (!Abilities.SunStrike.IsCataclysmActive || countForSs == 0 ||
                                            !CheckForCataclysm(countForSs))
                                        {
                                            casted = Abilities.SunStrike.UseAbility(Target.Position);
                                            IncComboStage(combo, casted);
                                        }
                                        else
                                        {
                                            if (!Abilities.SunStrike.IsInvoked)
                                                Abilities.SunStrike.Invoke();
                                            casted = Abilities.SunStrike.BaseAbility.BaseAbility.Cast(Me);
                                            IncComboStage(combo, casted);
                                        }

                                        //casted = ability.UseAbility(Target.Position);
                                        //IncComboStage(combo, casted);
                                    }
                                }
                                else
                                {
                                    if (!ability.IsInvoked)
                                        if (tornadoModifier != null)
                                            ability.Invoke();
                                }

                                break;
                        }
                    }
                    else
                    {
                        if (allAbilities[combo.AbilityInAction] == AbilityId.item_cyclone)
                        {
                            if (Abilities.Eul != null && Abilities.Eul.CanBeCasted() && !Target.HasAnyModifiers(
                                Abilities.IceWall.TargetModifierName,
                                Abilities.ColdSnap.TargetModifierName,
                                Abilities.Meteor.TargetModifierName) && !_sleeper.IsSleeping("EulCd"))
                            {
                                if (Abilities.Eul.CanHit(target9))
                                {
                                    Abilities.Eul.UseAbility(target9);
                                    // UpdateManager.BeginInvoke(() =>
                                    // {
                                    //     if (!Abilities.Eul.CanBeCasted)
                                    //         InvokerCrappahilationPaid.Log.Warn($"[{Abilities.Eul}] Casted:");
                                    // }, 150);

                                    _sleeper.Sleep("Eul", .250f);
                                    _sleeper.Sleep("EulCd", 10);
                                }
                            }
                            else
                            {
                                IncComboStage(combo);
                                // InvokerCrappahilationPaid.Log.Warn($"[{Abilities.Eul}] next Stage cuz null or cd");
                                _sleeper.Sleep("Eul", .110f);
                            }
                        }
                        else
                        {
                            if (Abilities.Refresher != null && Abilities.Refresher.CanBeCasted())
                            {
                                Abilities.Refresher.UseAbility();
                                // InvokerCrappahilationPaid.Log.Warn("[Refreshers] use refresher");
                                _sleeper.Sleep("Refresh", .500f);
                                _sleeper.Sleep("Eul", .110f);
                                SetComboAfterRefresher(combo);
                            }
                            else
                            {
                                if (Abilities.RefresherShard != null && Abilities.RefresherShard.CanBeCasted())
                                {
                                    Abilities.RefresherShard.UseAbility();
                                    // InvokerCrappahilationPaid.Log.Warn("[Refreshers] use refresher shard");
                                    SetComboAfterRefresher(combo);
                                    _sleeper.Sleep("Refresh", .500f);
                                    _sleeper.Sleep("Eul", .110f);
                                }
                                else
                                {
                                    IncComboStage(combo);
                                    // InvokerCrappahilationPaid.Log.Warn(
                                    // $"[Refreshers] next Stage cuz cant find any refresher or refresher on cooldown Null?{Abilities.Refresher != null}");
                                    _sleeper.Sleep("Eul", .110f);
                                }
                            }
                        }
                    }

                    break;

                #endregion
            }

            After:
            target9 = EntityManager9.GetUnit(Target.Handle);
            stunDuration = target9.GetImmobilityDuration();
            isStunned = target9.IsStunned;
            if (!isInvul)
                if (!_sleeper.IsSleeping("CooldownOnAction"))
                {
                    if (Me9.CanUseItems)
                    {
                        if (Abilities.Hex != null && Abilities.Hex.CanBeCasted() && Abilities.Hex.CanHit(target9) &&
                            !Target.HasAnyModifiers(Abilities.Hex.ImmobilityModifierName) && !target9.IsStunned)
                            Abilities.Hex.UseAbility(target9);

                        // if (Abilities.Necronomicon != null && Abilities.Necronomicon.CanBeCasted &&
                        //     Target.IsInRange(Me, 700)) Abilities.Necronomicon.UseAbility();
                        // if (Abilities.Necronomicon2 != null && Abilities.Necronomicon2.CanBeCasted &&
                        //     Target.IsInRange(Me, 700)) Abilities.Necronomicon2.UseAbility();
                        // if (Abilities.Necronomicon3 != null && Abilities.Necronomicon3.CanBeCasted &&
                        //     Target.IsInRange(Me, 700)) Abilities.Necronomicon3.UseAbility();

                        if (Abilities.Orchid != null && Abilities.Orchid.CanBeCasted() &&
                            Abilities.Orchid.CanHit(target9) &&
                            !Target.HasAnyModifiers(Abilities.Orchid.AmplifierModifierName) &&
                            !target9.IsStunned)
                            Abilities.Orchid.UseAbility(target9);

                        if (Abilities.Shiva != null && Abilities.Shiva.CanBeCasted() && Abilities.Shiva.CanHit(target9))
                            Abilities.Shiva.UseAbility();

                        if (Abilities.Bloodthorn != null && Abilities.Bloodthorn.CanBeCasted() &&
                            Abilities.Bloodthorn.CanHit(target9) &&
                            !Target.HasAnyModifiers(Abilities.Bloodthorn.AmplifierModifierName) &&
                            !target9.IsStunned)
                            Abilities.Bloodthorn.UseAbility(target9);

                        if (Abilities.Veil != null && Abilities.Veil.CanBeCasted() &&
                            Abilities.Veil.CanHit(target9) &&
                            !Target.HasAnyModifiers(Abilities.Veil.AmplifierModifierNames))
                        {
                            tornadoModifier = Target.GetFirstValidModifier("modifier_eul_cyclone",
                                "modifier_obsidian_destroyer_astral_imprisonment_prison",
                                "modifier_shadow_demon_disruption",
                                Abilities.Tornado.TargetModifierName, "modifier_brewmaster_storm_cyclone");
                            if (tornadoModifier == null)
                                Abilities.Veil.UseAbility(Target.Position);
                        }

                        if (Abilities.EtherealBlade != null && Abilities.EtherealBlade.CanBeCasted() &&
                            Abilities.EtherealBlade.CanHit(target9) &&
                            Target.HasAnyModifiers(Abilities.Meteor.TargetModifierName))
                            Abilities.EtherealBlade.UseAbility(target9);

                        if (Abilities.Bkb != null && Abilities.Bkb.CanBeCasted() && Me.IsInAttackRange(Target) &&
                            EntityManager.Entities.Count(x =>
                                x.IsValid && x.IsAlive && target9.IsEnemy(Me9) && x.IsVisible && x.IsInRange(Me, 650)) >= 3)
                            Abilities.Bkb.UseAbility();

                        if (_main.Config.RefresherBehavior.Value != "After Meteor+Blast")
                        {
                            if (Abilities.Refresher != null && Abilities.Refresher.CanBeCasted() &&
                                Me.IsInAttackRange(Target) &&
                                Abilities.AllAbilities.Count(x => x.AbilityState == AbilityState.OnCooldown) >=
                                8)
                                Abilities.Refresher.UseAbility();
                            else if (Abilities.RefresherShard != null && Abilities.RefresherShard.CanBeCasted() &&
                                     Me.IsInAttackRange(Target) &&
                                     Abilities.AllAbilities.Count(
                                         x => x.BaseAbility.BaseAbility.AbilityState == AbilityState.OnCooldown) >=
                                     8)
                                Abilities.RefresherShard.UseAbility();
                        }
                    }

                    if (_sleeper.IsSleeping("orbwalker_invoker"))
                        return;
                    _sleeper.Sleep("orbwalker_invoker", .250f);
                    Me9.Attack(target9);
                    // _main.Context.Orbwalker.Active.OrbwalkTo(Target);
                }
        }

        private bool CheckForEmpNearTarget(Unit9 target)
        {
            // Console.WriteLine($"TargetValid: {target.IsValid}");
            // Console.WriteLine($"Abilities.Tornado: {Abilities.Tornado.BaseAbility.IsValid}");
            var emps = _main.Updater.EmpPositions;
            var tornadoTime = Abilities.Tornado.BaseAbility.Duration;
            var delay = (float) Abilities.Tornado.BaseAbility.GetHitTime(target);
            return (from empInfo in emps
                let pos = empInfo.Value
                where target.Distance(pos) <= 675
                select 2.9 - (GameManager.RawGameTime - empInfo.Key)
                into timeLife
                select !(timeLife > tornadoTime + delay)).FirstOrDefault();
        }

        private bool CheckForCataclysm(int countForSs)
        {
            var enemyUndersEul = EntityManager.GetEntities<Hero>().Count(x =>
                x.IsValid && x.IsAlive && x.IsEnemy(Me) && x.IsVisible &&
                IsUnderInvulModifier(x));

            var target9 = EntityManager9.GetUnit(Target.Handle);
            var isTargetStunned = target9.IsStunned;
            return enemyUndersEul >= countForSs || isTargetStunned && target9.GetImmobilityDuration() > 1.0f ||
                   target9.HasModifier(_main.AbilitiesInCombo.Blast.TargetModifierName, _main.AbilitiesInCombo.IceWall.TargetModifierName, _main.AbilitiesInCombo.ColdSnap.TargetModifierName) || true;
        }

        private bool IsUnderInvulModifier(Hero target)
        {
            return target.HasAnyModifiers("modifier_eul_cyclone", Abilities.Tornado.TargetModifierName,
                "modifier_brewmaster_storm_cyclone", "modifier_shadow_demon_disruption",
                "modifier_obsidian_destroyer_astral_imprisonment_prison");
        }

        private bool EmpCheckForUnits(Unit target)
        {
            var enemies =
                EntityManager.GetEntities<Hero>().Where(x =>
                    x.IsAlive && x.IsEnemy(target) && !x.IsIllusion && x.IsInRange(target, 500));
            if (enemies is not Hero[] units) return enemies.Count() >= 3;
            var units9 = units.Select(z => EntityManager9.GetUnit(z.Handle));
            var count = units9.Count(hero9 => Abilities.Emp.BaseAbility.CanHit(hero9));
            return enemies.Count() >= 3 && count >= 3;
        }

        private void IncComboStage(ComboPanel.MyLittleCombo combo, bool casted = true)
        {
            if (_blocked)
                return;
            if (!casted)
                return;
            _blocked = true;
            var count = combo.Items.Count;
            var from = combo.Items[combo.AbilityInAction];
            UpdateManager.BeginInvoke(100, () =>
            {
                combo.AbilityInAction++;
                if (combo.AbilityInAction >= count)
                {
                    if (Config.BackToDynamicCombo)
                    {
                        foreach (var c in _main.Config.ComboPanel.Combos.Where(x => x.Enable || x.Id == -1))
                        {
                            c.IsSelected = c.Id == -1;
                            if (c.Id == -1)
                            {
                                _main.Config.ComboPanel.SelectedCombo = c;
                                c.AbilityInAction = 0;
                            }

                            // InvokerCrappahilationPaid.Log.Warn($"Id: {c.Id} [Selected={c.IsSelected}]");
                        }

                        _main.Config.ComboPanel.IsAutoComboSelected = true;
                        combo.AbilityInAction = 0;
                        // InvokerCrappahilationPaid.Log.Warn(
                        // $"Changed from Custom combo to Dynamic Combo [{GameplayType}]");
                        _blocked = false;
                        return;
                    }

                    combo.AbilityInAction = 0;
                }

                // InvokerCrappahilationPaid.Log.Warn($"Changed from {from} to {combo.Items[combo.AbilityInAction]}");
                _blocked = false;
            });
        }

        private void SetComboAfterRefresher(ComboPanel.MyLittleCombo combo, bool casted = true)
        {
            if (!casted)
                return;
            var time = 50; //(int) Math.Max(GameManager.Ping + 50f, 250);
            //InvokerCrappahilationPaid.Log.Error($"[Time: {time}]");
            UpdateManager.BeginInvoke(time, () =>
            {
                AfterRefresher = true;
                combo.AbilityInAction = combo.NextAbilityAfterRefresher;
                // InvokerCrappahilationPaid.Log.Warn($"Changed to {combo.Items[combo.AbilityInAction]} [Time: {time}]");
            });
        }

        public bool IsDirectlyFacing(Unit source, Vector3 pos, float args = 0.025f)
        {
            var vector1 = pos - source.Position;
            var diff = Math.Abs(Math.Atan2(vector1.Y, vector1.X) - source.RotationRad);
            return diff < args;
        }

        private ActiveAbility GetBestNextSpell()
        {
            return null;
        }

        private List<InvokerBaseAbility> GetAvilableAbilities()
        {
            var abilities = _main.AbilitiesInCombo.AllAbilities.Where(x => x.CanBeCasted() && _freeAbilities.Contains(x.Id) &&
                                                                           x.AbilitySlot is AbilitySlot.Slot4 or AbilitySlot.Slot5)
                .ToList();
            return abilities;
        }

        private bool GetTarget()
        {
            if (Target != null && Target.IsValid && Target.IsAlive) return true;
            // Target = EntityManager.GetEntities<Hero>().Where(x => x.IsEnemy(Me) && x.IsAlive && x.IsVisibleToEnemies).OrderBy(z => z.Distance2D(Me)).FirstOrDefault();
            // var target = EntityManager.GetEntities<Hero>().Where(x =>  x.IsValid && x.IsEnemy(Me) && x.IsAlive && x.IsVisibleToEnemies).OrderBy(z => z.Distance2D(Me)).FirstOrDefault();
            var target9 = EntityManager9.EnemyHeroes.Where(x => x.IsValid && x.IsAlive && x.IsVisible && x.IsEnemy(Me9) && x.Distance(GameManager.MousePosition) <= 500).OrderBy(z => z.Distance(Me9)).FirstOrDefault();
            Target = target9?.BaseUnit;
            try
            {
                if (_main.Config.ComboPanel.SelectedCombo != null)
                    _main.Config.ComboPanel.SelectedCombo.AbilityInAction = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // InvokerCrappahilationPaid.Log.Error(e);
            }

            return false;
        }
    }
}