using System;
using System.Collections.Generic;
using System.Linq;

using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;

using SharpDX;

using Prediction = Ensage.Common.Prediction;
using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

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
        private readonly MultiSleeper _sleeper;

        public Combo(InvokerCrappahilationPaid main)
        {
            _main = main;
            _sleeper = new MultiSleeper();
            _invokerSleeper = new Sleeper();
            var particleUpdateHandler = UpdateManager.Subscribe(UpdateTargetParticle, 0, false);
            var comboUpdateHandler = UpdateManager.Subscribe(ComboInAction, 25, false);
            Config.ComboKey.PropertyChanged += (sender, args) =>
            {
                if (Config.ComboKey)
                {
                    comboUpdateHandler.IsEnabled = true;
                    particleUpdateHandler.IsEnabled = true;
                }
                else
                {
                    comboUpdateHandler.IsEnabled = false;
                    particleUpdateHandler.IsEnabled = false;
                    Target = null;
                    ParticleManager.Remove("TargetEffectLine");
                    try
                    {
                        if (_main.Config.ComboPanel.SelectedCombo != null)
                            _main.Config.ComboPanel.SelectedCombo.AbilityInAction = 0;
                    }
                    catch (Exception e)
                    {
                        InvokerCrappahilationPaid.Log.Error(e);
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
                    Console.WriteLine($"Angl: {GetDif(Me, target.NetworkPosition)}");
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

        private IParticleManager ParticleManager => _main.Context.Particle;

        private AbilitiesInCombo Abilities => _main.AbilitiesInCombo;
        private uint ExortLevel => Abilities.Exort.Level;
        private uint WexLevel => Abilities.Wex.Level;
        private uint QuasLevel => Abilities.Quas.Level;

        private ComboTypeEnum GameplayType =>
            _main.Config.ComboPanel.IsAutoComboSelected ? ComboTypeEnum.Auto : ComboTypeEnum.CustomCombo;

        public Unit Target { get; set; }
        private Hero Me => (Hero) _main.Context.Owner;

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

        public bool InvokeThisShit(ActiveAbility ability)
        {
            InvokerCrappahilationPaid.Log.Debug($"Trying to invoke -> {ability.Ability.Id}");
            if (_sleeper.Sleeping($"{ability} shit"))
            {
                InvokerCrappahilationPaid.Log.Debug($"Invoke [blocked] ({ability})");
                return false;
            }

            if (Abilities.Invoke.IsReady)
            {
                var requiredOrbs = (ability as IInvokableAbility)?.RequiredOrbs;
                if (requiredOrbs != null)
                {
                    foreach (var abilityId in requiredOrbs)
                    {
                        var sphere = (ActiveAbility) _main.Context.AbilityFactory.GetAbility(abilityId);
                        if (sphere == null) return false;

                        if (!sphere.UseAbility()) return false;

                        InvokerCrappahilationPaid.Log.Debug($"Invoke [Sphere: {abilityId}] ({ability})");
                    }

                    var invoked = Abilities.Invoke.Ability.UseAbility();
                    if (invoked)
                    {
                        _sleeper.Sleep(200, $"{ability} shit");
                        InvokerCrappahilationPaid.Log.Debug($"invoked [{ability}]");
                    }

                    return invoked;
                }

                InvokerCrappahilationPaid.Log.Debug($"Error in Invoke function: {ability.Ability.Id}");
                return false;
            }

            InvokerCrappahilationPaid.Log.Debug($"Invoke [on cd] ({ability})");
            return false;
        }

        private void UpdateTargetParticle()
        {
            if (Target == null || !Target.IsValid || !Target.IsVisible)
            {
                ParticleManager.Remove("TargetEffectLine");
                return;
            }

            ParticleManager.DrawTargetLine(Me, "TargetEffectLine", Target.Position, Color.YellowGreen);
        }

        private void ComboInAction()
        {
            if (!GetTarget())
            {
                if (_sleeper.Sleeping("moving"))
                    return;

                _sleeper.Sleep(125, "moving");

                var mousePos = Game.MousePosition;
                if (_main.Context.Orbwalker.Active.CanMove())
                    _main.Context.Orbwalker.Active.Move(mousePos);

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
            if (!_sleeper.Sleeping("Orbwalker"))
            {
                _sleeper.Sleep(250, "Orbwalker");
                /*foreach (var orbwalker in _main.Updater.Units
                    .Where(x => x.Unit != null && x.Unit.IsValid && x.CanWork && x.Unit.IsAlive)
                    .Select(z => z.Orbwalker.Active))
                {
                    if (orbwalker.CanAttack(Target))
                        orbwalker.Attack(Target);
                    
                }*/
                foreach (var unit in _main.Updater.Units
                    .Where(x => x.Unit != null && x.Unit.IsValid && x.CanWork && x.Unit.IsAlive)
                    .Select(z => z.Unit))
                {
                    if (!_sleeper.Sleeping($"archerSlow{unit.Handle}") &&
                        unit.Name.Contains("npc_dota_necronomicon_archer"))
                    {
                        var ability = unit.Spellbook.Spell1;
                        if (ability.CanBeCasted() && ability.CanHit(Target) && !UnitExtensions.IsStunned(Target) &&
                            Target.MovementSpeed > 280 &&
                            !UnitExtensions.IsMagicImmune(Target))
                        {
                            ability.UseAbility(Target);
                            _sleeper.Sleep(500, $"archerSlow{unit.Handle}");
                        }
                    }

                    if (isInvul)
                        unit.Move(Target.NetworkPosition);
                    else if (!unit.IsAttacking())
                        unit.Attack(Target);
                }
            }

            Modifier tornadoModifier;
            if (Abilities.IceWall.InAction)
                return;
            switch (GameplayType)
            {
                case ComboTypeEnum.Auto:

                    #region AutoCombo

                    if (_sleeper.Sleeping("CooldownOnAction"))
                        return;
                    if (_sleeper.Sleeping("Eul") /*|| _invokerSleeper.Sleeping*/)
                        return;

                    tornadoModifier = Target.FindModifier("modifier_eul_cyclone") ??
                                      Target.FindModifier(Abilities.Tornado.TargetModifierName) ??
                                      Target.FindModifier("modifier_brewmaster_storm_cyclone") ??
                                      Target.FindModifier("modifier_shadow_demon_disruption") ??
                                      Target.FindModifier("modifier_obsidian_destroyer_astral_imprisonment_prison");
                    if (tornadoModifier == null && !_sleeper.Sleeping("AfterRefresh"))
                    {
                        var allFineWithTarget = (!Target.IsStunned(out var stunDuration) || stunDuration <= 0.5f) /* &&
                                                (!Target.HasAnyModifiers("modifier_bloodthorn_debuff",
                                                    "modifier_orchid_malevolence_debuff")) */ &&
                                                (Target.IsHexed(out var hexDuration) || hexDuration <= 0.5f) &&
                                                !Target.HasAnyModifiers(Abilities.IceWall.TargetModifierName,
                                                    Abilities.ColdSnap.TargetModifierName,
                                                    Abilities.Meteor.TargetModifierName, "modifier_bloodthorn_debuff",
                                                    "modifier_orchid_malevolence_debuff") &&
                                                !CheckForEmpNearTarget(Target);
                        if (Abilities.Eul != null && Abilities.Eul.CanBeCasted && allFineWithTarget && Config.UseEul)
                        {
                            var makesSensesToCastEul =
                                Abilities.SunStrike.CanBeCasted || Abilities.Meteor.CanBeCasted ||
                                WexLevel >= 4 && Abilities.Emp.CanBeCasted /*||
                                                   Abilities.IceWall.CanBeCasted*/;

                            if (makesSensesToCastEul)
                            {
                                if (!Abilities.Eul.CanHit(Target))
                                {
                                    if (_main.Context.Orbwalker.Active.CanMove())
                                        _main.Context.Orbwalker.Active.Move(Target.Position);
                                    break;
                                }

                                Abilities.Eul.UseAbility(Target);
                                _sleeper.Sleep(Abilities.Eul.GetHitTime(Target) + 250, "Eul");
                                return;
                            }
                        }
                        else if (allFineWithTarget && QuasLevel >= 5 && Abilities.Tornado.CanBeCasted &&
                                 (Abilities.SunStrike.CanBeCasted || Abilities.Meteor.CanBeCasted ||
                                  Abilities.Tornado.GetDamage(Target) >
                                  Target.Health + Target.HealthRegeneration * (Abilities.Tornado.Duration + 1) ||
                                  WexLevel >= 4 && Abilities.Emp.CanBeCasted) && Abilities.Tornado.CanHit(Target) &&
                                 Target.IsInRange(Me, 1000))
                        {
                            InvokerCrappahilationPaid.Log.Debug(
                                $"[Use] [{Abilities.Tornado}] TargetHealth (with regen prediction): {Target.Health + Target.HealthRegeneration * (Abilities.Tornado.Duration + 1)} Damage: {Abilities.Tornado.GetDamage(Target)}");

                            var input = Abilities.Tornado.GetPredictionInput(Target);
                            var output = Abilities.Tornado.GetPredictionOutput(input);
                            if (output.HitChance == HitChance.VeryHigh || output.HitChance == HitChance.High ||
                                output.HitChance == HitChance.Medium)
                            {
                                if (Abilities.Tornado.IsInvoked)
                                {
                                    //Abilities.Tornado.SafeInvoke(Abilities.SunStrike, Abilities.Meteor);
                                    var casted = Abilities.Tornado.UseAbility(output.UnitPosition);
                                    if (casted)
                                    {
                                        var delay = (float) Abilities.Tornado.Ability.GetHitDelay(Target);
                                        var arrivalTime = output.ArrivalTime;
                                        InvokerCrappahilationPaid.Log.Warn(
                                            $"[Use][{Abilities.Tornado}] [Delay: {delay}] [ArrivalTime: {arrivalTime}]");

                                        _sleeper.Sleep(delay * 1000 + 500, "Eul");
                                        _sleeper.Sleep(delay * 1000 + 500, "PussyCaster");
                                        return;
                                    }
                                }
                                else
                                {
                                    if (Abilities.SunStrike.CanBeCasted && Abilities.SunStrike.IsInvoked &&
                                        Abilities.SunStrike.Ability.AbilitySlot == AbilitySlot.Slot_5 &&
                                        !_sleeper.Sleeping($"Invoked {Abilities.SunStrike}"))
                                    {
                                        if (InvokeThisShit(Abilities.SunStrike))
                                        {
                                            //Abilities.SunStrike.Invoke(skip: true);
                                            _sleeper.Sleep(150, "Eul");
                                            _sleeper.Sleep(150, $"Invoked {Abilities.SunStrike}");
                                            //Abilities.Tornado.Invoke(skip: true);
                                        }
                                    }
                                    else if (Abilities.Meteor.CanBeCasted && Abilities.Meteor.IsInvoked &&
                                             Abilities.Meteor.Ability.AbilitySlot == AbilitySlot.Slot_5 &&
                                             !_sleeper.Sleeping($"Invoked {Abilities.Meteor}"))
                                    {
                                        if (InvokeThisShit(Abilities.Meteor))
                                        {
                                            //Abilities.Meteor.Invoke(skip: true);
                                            _sleeper.Sleep(150, "Eul");
                                            _sleeper.Sleep(150, $"Invoked {Abilities.Meteor}");
                                            //Abilities.Tornado.Invoke(skip: true);
                                        }
                                    }
                                    else if (WexLevel >= 4 && Abilities.Emp.CanBeCasted && Abilities.Emp.IsInvoked &&
                                             Abilities.Emp.Ability.AbilitySlot == AbilitySlot.Slot_5 &&
                                             !_sleeper.Sleeping($"Invoked {Abilities.Emp}"))
                                    {
                                        if (InvokeThisShit(Abilities.Emp))
                                        {
                                            _sleeper.Sleep(150, "Eul");
                                            _sleeper.Sleep(150, $"Invoked {Abilities.Emp}");
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
                            var abilities = GetAviableAbilities().ToList();
                            if (abilities.Any())
                            {
                                foreach (var baseAbility in abilities)
                                {
                                    var ability = (ActiveAbility) baseAbility;
                                    if (ability is InvokerIceWall iceWall)
                                    {
                                        if (Target.IsInRange(Me, 550) &&
                                            (Target.IsStunned(out var dur) || Target.MovementSpeed <= 425f))
                                        {
                                            var casted = iceWall.CastAsync(Target);
                                            //_sleeper.Sleep(500, "Eul");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        ability.UseAbility(ability.Ability.Id == AbilityId.invoker_alacrity
                                            ? Me
                                            : Target);
                                    }
                                }

                                return;
                            }
                        }
                    }

                    if (tornadoModifier != null && !_sleeper.Sleeping("AfterRefresh"))
                    {
                        if (Abilities.ForgeSpirit.CanBeCasted && Abilities.ForgeSpirit.IsInvoked &&
                            Me.IsInAttackRange(Target))
                            Abilities.ForgeSpirit.UseAbility();

                        if (Abilities.Alacrity.CanBeCasted && Abilities.Alacrity.IsInvoked &&
                            Me.IsInAttackRange(Target) &&
                            !UnitExtensions.HasModifier(Me, Abilities.Alacrity.ModifierName))
                            Abilities.Alacrity.UseAbility(Me);

                        var empChecker = Me.GetManaPercent() > 0.5f || !Abilities.Emp.CanBeCasted;
                        if (Abilities.SunStrike.CanBeCasted && empChecker)
                        {
                            if (Abilities.SunStrike.IsInvoked)
                            {
                                if (tornadoModifier.RemainingTime <= Abilities.SunStrike.ActivationDelay)
                                {
                                    if (tornadoModifier.RemainingTime >= Abilities.SunStrike.ActivationDelay - 0.85f)
                                    {
                                        var countForSs = Config.UseCataclysm.Value.Value;
                                        if (!Abilities.SunStrike.IsCataclysmActive || countForSs == 0 ||
                                            !CheckForCataclysm(countForSs))
                                            Abilities.SunStrike.UseAbility(Target.Position);
                                        else
                                        {
                                            if (!Abilities.SunStrike.IsInvoked)
                                                Abilities.SunStrike.Invoke();
                                            Abilities.SunStrike.Ability.UseAbility(Me);
                                        }
                                    }
                                }
                                else
                                {
                                    if (Abilities.SunStrike.Ability.AbilitySlot == AbilitySlot.Slot_4)
                                    {
                                        if (Abilities.Emp.Ability.AbilitySlot != AbilitySlot.Slot_5)
                                        {
                                            if (Abilities.Meteor.CanBeCasted)
                                                InvokeThisShit(Abilities.Meteor);
                                            else if (Abilities.Blast.CanBeCasted)
                                                InvokeThisShit(Abilities.Blast);
                                            else if (WexLevel >= 4 && Abilities.Emp.CanBeCasted)
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
                            else
                            {
                                if (Me.HasAghanimsScepter())
                                {
                                    //if (Abilities.Meteor.CanBeCasted && Abilities.Blast.CanBeCasted)
                                    //{
                                    if (Abilities.Blast.Ability.AbilitySlot == AbilitySlot.Slot_5)
                                    {
                                        if (Abilities.Wex.Level >= 4 && Abilities.Wex.CanBeCasted)
                                            InvokeThisShit(Abilities.Emp);
                                        else if (Abilities.Blast.CanBeCasted) InvokeThisShit(Abilities.Blast);

                                        InvokeThisShit(Abilities.SunStrike);
                                        //Abilities.Blast.Invoke(skip: true);
                                        //Abilities.SunStrike.Invoke(skip: true);
                                    }
                                    else if (Abilities.Meteor.Ability.AbilitySlot == AbilitySlot.Slot_5)
                                    {
                                        if (Abilities.Meteor.CanBeCasted)
                                            InvokeThisShit(Abilities.Meteor);
                                        else if (Abilities.Wex.Level >= 4 && Abilities.Emp.CanBeCasted)
                                            InvokeThisShit(Abilities.Emp);

                                        InvokeThisShit(Abilities.SunStrike);
                                        //Abilities.Meteor.Invoke(skip: true);
                                        //Abilities.SunStrike.Invoke(skip: true);
                                    }
                                    else
                                    {
                                        Abilities.SunStrike.Invoke();
                                    }

                                    //}
                                }
                                else
                                {
                                    if (Abilities.Meteor.Ability.AbilitySlot == AbilitySlot.Slot_5)
                                    {
                                        InvokeThisShit(Abilities.Meteor);
                                        InvokeThisShit(Abilities.SunStrike);
                                        //Abilities.Meteor.Invoke(skip: true);
                                        //Abilities.SunStrike.Invoke(skip: true);
                                    }
                                    else
                                    {
                                        Abilities.SunStrike.Invoke();
                                    }
                                }
                            }
                        }

                        if (Abilities.Meteor.CanBeCasted && Abilities.Meteor.CanHit(Target) && empChecker)
                        {
                            if (tornadoModifier.RemainingTime <= Abilities.Meteor.ActivationDelay)
                                if (tornadoModifier.RemainingTime >= Abilities.Meteor.ActivationDelay - 0.5f)
                                {
                                    var targetPos = Target.Position.Extend(Me.Position, ExtraMeteorPosition);
                                    Abilities.Meteor.UseAbility(targetPos);
                                }
                        }
                        else if (Abilities.Blast.CanBeCasted && Abilities.Blast.CanHit(Target) && empChecker)
                        {
                            var hitTime = Math.Max((Abilities.Blast.GetHitTime(Target) - 100) / 1000f, 0.1);
                            if (tornadoModifier.RemainingTime <= hitTime) Abilities.Blast.UseAbility(Target.Position);
                        }
                        else if (WexLevel >= 4 && Abilities.Emp.CanBeCasted && Abilities.Emp.CanHit(Target))
                        {
                            if (tornadoModifier.RemainingTime <= Abilities.Emp.ActivationDelay)
                                Abilities.Emp.UseAbility(Target.Position);
                        }
                        else if (Config.UseIceWall && Abilities.IceWall.CanBeCasted && Me.IsInRange(Target, 550) &&
                                 (Target.IsStunned(out var dur) || Target.MovementSpeed <= 425f) &&
                                 Abilities.IceWall.Invoke() /* && !Me.IsInRange(Target, 115) &&
                                 Me.IsDirectlyFacing(Target) IsDirectlyFacing(Me,Target.NetworkPosition,0.065f)*/)
                        {
                            if (Abilities.Invoke.CanBeCasted)
                            {
                                var asyncCasted = Abilities.IceWall.CastAsync(Target);
                                //_sleeper.Sleep(CooldownOnAction, "CooldownOnAction");
                                return;
                            }
                        }
                    }
                    else if (!_sleeper.Sleeping("PussyCaster") && !_sleeper.Sleeping("Casted"))
                    {
                        var isStunned = Target.IsStunned(out var stunTime);
                        if (Abilities.Blast.CanBeCasted && Abilities.Blast.CanHit(Target) &&
                            (stunTime > Abilities.Blast.GetHitTime(Target) ||
                             Target.HasAnyModifiers(Abilities.Meteor.TargetModifierName)))
                        {
                            if (Abilities.Blast.UseAbility(Target.Position))
                                _sleeper.Sleep(250, "Casted");
                        }
                        else if (Abilities.Meteor.CanBeCasted && Abilities.Meteor.CanHit(Target) &&
                                 (stunTime > Abilities.Meteor.ActivationDelay ||
                                  Target.HasAnyModifiers(Abilities.IceWall.TargetModifierName,
                                      "modifier_invoker_deafening_blast_knockback",
                                      "modifier_invoker_cold_snap_freeze")))
                        {
                            var predPos = Target.IsMoving && !Target.IsRotating()
                                ? UnitExtensions.InFront(Target, 150)
                                : Target.NetworkPosition;
                            if (Abilities.Meteor.UseAbility(predPos))
                                _sleeper.Sleep(250, "Casted");
                        }
                        else if (Config.UseIceWall && Abilities.IceWall.CanBeCasted && Me.IsInRange(Target, 550) &&
                                 Abilities.IceWall.Invoke() /*&&
                                 !Me.IsInRange(Target, 115) && IsDirectlyFacing(Me,Target.NetworkPosition,0.065f)*/
                            /*Me.IsDirectlyFacing(Target)*/)
                        {
                            if (Abilities.Invoke.CanBeCasted)
                            {
                                var asyncCasted = Abilities.IceWall.CastAsync(Target);
                                //_sleeper.Sleep(CooldownOnAction, "CooldownOnAction");
                                return;
                            }
                        }
                        else if (Abilities.SunStrike.CanBeCasted && Abilities.SunStrike.CanHit(Target) &&
                                 (stunTime > Abilities.SunStrike.ActivationDelay ||
                                  Target.HasAnyModifiers("modifier_invoker_cold_snap_freeze")))
                        {
                            if (stunTime > Abilities.SunStrike.ActivationDelay)
                            {
                                if (Abilities.SunStrike.UseAbility(Target.Position))
                                    _sleeper.Sleep(250, "Casted");
                            }
                            else if (Target.HasAnyModifiers("modifier_invoker_cold_snap_freeze") &&
                                     Target.MovementSpeed <= 280)
                            {
                                var predictPos =
                                    Prediction.InFront(Target,
                                        Target.IsMoving
                                            ? Target.MovementSpeed / 2f * 1.9f
                                            : Target.MovementSpeed / 2f * 0.8f);

                                if (Abilities.SunStrike.UseAbility(predictPos))
                                    _sleeper.Sleep(250, "Casted");
                            }
                        }
                        else if (Abilities.ColdSnap.CanBeCasted && Abilities.ColdSnap.CanHit(Target))
                        {
                            if (Abilities.ColdSnap.UseAbility(Target))
                                _sleeper.Sleep(250, "Casted");
                        }
                        else if (WexLevel >= 4 && Abilities.Emp.CanBeCasted &&
                                 (Target.HasAnyModifiers(Abilities.IceWall.TargetModifierName,
                                      Abilities.ColdSnap.TargetModifierName) || EmpCheckForUnits(Target)))
                        {
                            if (Abilities.Emp.UseAbility(Target.NetworkPosition))
                                _sleeper.Sleep(250, "Casted");
                        }
                        else if (Abilities.ForgeSpirit.CanBeCasted && Me.IsInAttackRange(Target))
                        {
                            if (Abilities.ForgeSpirit.UseAbility())
                                _sleeper.Sleep(250, "Casted");
                        }
                        else if (Abilities.Alacrity.CanBeCasted && Me.IsInAttackRange(Target) &&
                                 !UnitExtensions.HasModifier(Me, Abilities.Alacrity.ModifierName))
                        {
                            if (Abilities.Alacrity.UseAbility(Me))
                                _sleeper.Sleep(250, "Casted");
                        }

                        if ((_main.Config.RefresherBehavior.Value.SelectedIndex == 0 ||
                             _main.Config.RefresherBehavior.Value.SelectedIndex == 2) &&
                            Abilities.Meteor.Ability.AbilityState == AbilityState.OnCooldown &&
                            Abilities.Blast.Ability.AbilityState == AbilityState.OnCooldown)
                        {
                            if (Abilities.Refresher != null && Abilities.Refresher.CanBeCasted &&
                                Me.IsInAttackRange(Target))
                            {
                                Abilities.Refresher.UseAbility();
                                _sleeper.Sleep(1000, "AfterRefresh");
                                return;
                            }

                            if (Abilities.RefresherShard != null && Abilities.RefresherShard.CanBeCasted &&
                                Me.IsInAttackRange(Target))
                            {
                                Abilities.RefresherShard.UseAbility();
                                _sleeper.Sleep(1000, "AfterRefresh");
                                return;
                            }
                        }
                    }

                    break;

                #endregion

                case ComboTypeEnum.CustomCombo:

                    #region CustomCombo

                    if (_sleeper.Sleeping("CooldownOnAction") /*|| _invokerSleeper.Sleeping*/)
                        return;
                    if (_sleeper.Sleeping("Eul") /*|| _invokerSleeper.Sleeping*/)
                        return;
                    var combo = _main.Config.ComboPanel.SelectedCombo;
                    var allAbilities = combo.Items.ToArray();
                    var abilityInAction =
                        _main.AbilitiesInCombo.AllAbilities.Find(x =>
                            x.Ability.Name == allAbilities[combo.AbilityInAction]);
                    if (abilityInAction != null)
                    {
                        if (abilityInAction is IInvokableAbility ablity)
                            if (!_sleeper.Sleeping("Refresh") && abilityInAction.Ability.Cooldown >= 2)
                            {
                                InvokerCrappahilationPaid.Log.Warn(
                                    $"Skip Ability Cuz cant invoke or CD -> {abilityInAction} {abilityInAction.Ability.Cooldown} InvokeCooldown: {Abilities.Invoke.Ability.Cooldown}");
                                IncComboStage(combo, true);
                                goto After;
//                                return;
                            }

                        tornadoModifier = Target.FindModifier("modifier_eul_cyclone") ??
                                          Target.FindModifier(Abilities.Tornado.TargetModifierName) ??
                                          Target.FindModifier("modifier_brewmaster_storm_cyclone") ??
                                          Target.FindModifier("modifier_shadow_demon_disruption") ??
                                          Target.FindModifier("modifier_obsidian_destroyer_astral_imprisonment_prison");
                        bool casted;
                        switch (abilityInAction)
                        {
                            case InvokerAlacrity ability:
                                casted = ability.UseAbility(Me);
                                IncComboStage(combo, casted);
                                break;
                            case InvokerChaosMeteor ability:
                                if (ability.CanHit(Target))
                                {
                                    if (tornadoModifier != null && tornadoModifier.RemainingTime <=
                                        Abilities.Meteor.ActivationDelay ||
                                        Target.HasAnyModifiers(Abilities.ColdSnap.TargetModifierName,
                                            Abilities.IceWall.TargetModifierName,
                                            Abilities.Blast.TargetModifierName) &&
                                        !Target.HasAnyModifiers(Abilities.Tornado.TargetModifierName))
                                    {
                                        var targetPos = Target.Position.Extend(Me.Position, ExtraMeteorPosition);
                                        casted = ability.UseAbility(targetPos);
                                        InvokerCrappahilationPaid.Log.Error($"[{ability}] Casted: {casted}");
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
                                if (ability.CanHit(Target))
                                {
                                    if (tornadoModifier != null &&
                                        tornadoModifier.RemainingTime <= Abilities.Emp.ActivationDelay ||
                                        Target.HasAnyModifiers(Abilities.IceWall.TargetModifierName,
                                            Abilities.ColdSnap.TargetModifierName,
                                            Abilities.Blast.TargetModifierName) || EmpCheckForUnits(Target))
                                    {
                                        if (ability.CanBeCasted)
                                        {
                                            casted = ability.UseAbility(Target.NetworkPosition);
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
                                if (ability.CanHit(Target))
                                {
                                    var hitTime = Math.Max((Abilities.Blast.GetHitTime(Target) - 100) / 1000f, 0.1);
                                    if (tornadoModifier != null && tornadoModifier.RemainingTime <= hitTime ||
                                        Target.HasAnyModifiers(Abilities.IceWall.TargetModifierName,
                                            Abilities.ColdSnap.TargetModifierName,
                                            Abilities.Meteor.TargetModifierName) &&
                                        !Target.HasAnyModifiers(Abilities.Tornado.TargetModifierName))
                                    {
                                        if (ability.CanBeCasted)
                                        {
                                            casted = ability.UseAbility(Target.NetworkPosition);
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
                                if (ability.CanBeCasted)
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
                                if (ability.CanBeCasted && Me.IsInRange(Target, 550) && ability.Invoke())
                                {
                                    var castedAsync = ability.CastAsync(Target);
                                    //_sleeper.Sleep(CooldownOnAction, "CooldownOnAction");
                                    IncComboStage(combo, true);
                                }

                                break;
                                if (ability.CanBeCasted && Me.IsInRange(Target, 250) &&
                                    !Me.IsInRange(Target, 115) &&
                                    IsDirectlyFacing(Me, Target.NetworkPosition, 0.065f))
                                {
                                    casted = ability.UseAbility();
                                    IncComboStage(combo, casted);
                                }

                                break;
                            case InvokerTornado ability:
                                var input = Abilities.Tornado.GetPredictionInput(Target);
                                var output = Abilities.Tornado.GetPredictionOutput(input);
                                if (output.HitChance == HitChance.VeryHigh || output.HitChance == HitChance.High ||
                                    output.HitChance == HitChance.Medium)
                                {
                                    casted = ability.UseAbility(output.UnitPosition);
                                    IncComboStage(combo, casted);
                                    if (casted)
                                    {
                                        var delay = (float) Abilities.Tornado.Ability.GetHitDelay(Target);
                                        var arrivalTime = output.ArrivalTime;
                                        InvokerCrappahilationPaid.Log.Warn(
                                            $"[Use][{Abilities.Tornado}] [Delay: {delay}] [ArrivalTime: {arrivalTime}]");

                                        _sleeper.Sleep(delay * 1000 + 500, "Eul");
                                        _sleeper.Sleep(delay * 1000 + 500, "PussyCaster");
                                        return;
                                    }
                                }

                                break;
                            case InvokerColdSnap ability:
                                if (ability.CanHit(Target))
                                {
                                    casted = ability.UseAbility(Target);
                                    IncComboStage(combo, casted);
                                }

                                break;
                            case InvokerSunStrike ability:
                                if (tornadoModifier != null && tornadoModifier.RemainingTime <=
                                    Abilities.SunStrike.ActivationDelay || Target.HasAnyModifiers(
                                        Abilities.IceWall.TargetModifierName,
                                        Abilities.ColdSnap.TargetModifierName,
                                        Abilities.Meteor.TargetModifierName) &&
                                    !Target.HasAnyModifiers(Abilities.Tornado
                                        .TargetModifierName))
                                {
                                    if (ability.CanBeCasted)
                                    {
                                        var countForSs = Config.UseCataclysm.Value.Value;
                                        if (!Abilities.SunStrike.IsCataclysmActive || countForSs == 0 ||
                                            !CheckForCataclysm(countForSs))
                                        {
                                            casted = Abilities.SunStrike.UseAbility(Target.NetworkPosition);
                                            IncComboStage(combo, casted);
                                        }
                                        else
                                        {
                                            if (!Abilities.SunStrike.IsInvoked)
                                                Abilities.SunStrike.Invoke();
                                            casted = Abilities.SunStrike.Ability.UseAbility(Me);
                                            IncComboStage(combo, casted);
                                        }

                                        //casted = ability.UseAbility(Target.NetworkPosition);
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
                        if (allAbilities[combo.AbilityInAction] == AbilityId.item_cyclone.ToString())
                        {
                            if (Abilities.Eul != null && Abilities.Eul.CanBeCasted && !Target.HasAnyModifiers(
                                    Abilities.IceWall.TargetModifierName,
                                    Abilities.ColdSnap.TargetModifierName,
                                    Abilities.Meteor.TargetModifierName) && !_sleeper.Sleeping("EulCd"))
                            {
                                if (Abilities.Eul.CanHit(Target))
                                {
                                    Abilities.Eul.UseAbility(Target);
                                    UpdateManager.BeginInvoke(() =>
                                    {
                                        if (!Abilities.Eul.CanBeCasted)
                                            InvokerCrappahilationPaid.Log.Warn($"[{Abilities.Eul}] Casted:");
                                    }, 150);

                                    _sleeper.Sleep(250, "Eul");
                                    _sleeper.Sleep(10000, "EulCd");
                                }
                            }
                            else
                            {
                                IncComboStage(combo);
                                InvokerCrappahilationPaid.Log.Warn($"[{Abilities.Eul}] next Stage cuz null or cd");
                                _sleeper.Sleep(110, "Eul");
                            }
                        }
                        else
                        {
                            if (Abilities.Refresher != null && Abilities.Refresher.CanBeCasted)
                            {
                                Abilities.Refresher.UseAbility();
                                InvokerCrappahilationPaid.Log.Warn("[Refreshers] use refresher");
                                _sleeper.Sleep(500, "Refresh");
                                _sleeper.Sleep(110, "Eul");
                                SetComboAfterRefresher(combo);
                            }
                            else
                            {
                                if (Abilities.RefresherShard != null && Abilities.RefresherShard.CanBeCasted)
                                {
                                    Abilities.RefresherShard.UseAbility();
                                    InvokerCrappahilationPaid.Log.Warn("[Refreshers] use refresher shard");
                                    SetComboAfterRefresher(combo);
                                    _sleeper.Sleep(500, "Refresh");
                                    _sleeper.Sleep(110, "Eul");
                                }
                                else
                                {
                                    IncComboStage(combo);
                                    InvokerCrappahilationPaid.Log.Warn(
                                        $"[Refreshers] next Stage cuz cant find any refresher or refresher on cooldown Null?{Abilities.Refresher != null}");
                                    _sleeper.Sleep(110, "Eul");
                                }
                            }
                        }
                    }

                    break;

                #endregion
            }

            After:
            if (!isInvul)
                if (!_sleeper.Sleeping("CooldownOnAction"))
                {
                    if (Me.CanUseItems())
                    {
                        if (Abilities.Hex != null && Abilities.Hex.CanBeCasted && Abilities.Hex.CanHit(Target) &&
                            !Target.HasAnyModifiers(Abilities.Hex.TargetModifierName) && !Target.IsStunned(out _))
                            Abilities.Hex.UseAbility(Target);

                        if (Abilities.Necronomicon != null && Abilities.Necronomicon.CanBeCasted &&
                            Target.IsInRange(Me, 700)) Abilities.Necronomicon.UseAbility();
                        if (Abilities.Necronomicon2 != null && Abilities.Necronomicon2.CanBeCasted &&
                            Target.IsInRange(Me, 700)) Abilities.Necronomicon2.UseAbility();
                        if (Abilities.Necronomicon3 != null && Abilities.Necronomicon3.CanBeCasted &&
                            Target.IsInRange(Me, 700)) Abilities.Necronomicon3.UseAbility();

                        if (Abilities.Orchid != null && Abilities.Orchid.CanBeCasted &&
                            Abilities.Orchid.CanHit(Target) &&
                            !Target.HasAnyModifiers(Abilities.Orchid.TargetModifierName) &&
                            !Target.IsStunned(out _))
                            Abilities.Orchid.UseAbility(Target);

                        if (Abilities.Shiva != null && Abilities.Shiva.CanBeCasted && Abilities.Shiva.CanHit(Target))
                            Abilities.Shiva.UseAbility();

                        if (Abilities.Bloodthorn != null && Abilities.Bloodthorn.CanBeCasted &&
                            Abilities.Bloodthorn.CanHit(Target) &&
                            !Target.HasAnyModifiers(Abilities.Bloodthorn.TargetModifierName) &&
                            !Target.IsStunned(out _))
                            Abilities.Bloodthorn.UseAbility(Target);

                        if (Abilities.Veil != null && Abilities.Veil.CanBeCasted &&
                            Abilities.Veil.CanHit(Target) &&
                            !Target.HasAnyModifiers(Abilities.Veil.TargetModifierName))
                        {
                            tornadoModifier = Target.FindModifier("modifier_eul_cyclone") ??
                                              Target.FindModifier(Abilities.Tornado.TargetModifierName) ??
                                              Target.FindModifier("modifier_brewmaster_storm_cyclone") ??
                                              Target.FindModifier("modifier_shadow_demon_disruption") ??
                                              Target.FindModifier(
                                                  "modifier_obsidian_destroyer_astral_imprisonment_prison");
                            if (tornadoModifier == null)
                                Abilities.Veil.UseAbility(Target.NetworkPosition);
                        }

                        if (Abilities.EtherealBlade != null && Abilities.EtherealBlade.CanBeCasted &&
                            Abilities.EtherealBlade.CanHit(Target) &&
                            Target.HasAnyModifiers(Abilities.Meteor.TargetModifierName))
                            Abilities.EtherealBlade.UseAbility(Target);

                        if (Abilities.Bkb != null && Abilities.Bkb.CanBeCasted && Me.IsInAttackRange(Target) &&
                            EntityManager<Hero>.Entities.Count(x =>
                                x.IsValid && x.IsAlive && x.IsEnemy(Me) && x.IsVisible && x.IsInRange(Me, 650)) >= 3)
                            Abilities.Bkb.UseAbility();

                        if (_main.Config.RefresherBehavior.Value.SelectedIndex == 1 ||
                            _main.Config.RefresherBehavior.Value.SelectedIndex == 2)
                        {
                            if (Abilities.Refresher != null && Abilities.Refresher.CanBeCasted &&
                                Me.IsInAttackRange(Target) &&
                                Abilities.AllAbilities.Count(x => x.Ability.AbilityState == AbilityState.OnCooldown) >=
                                8)
                                Abilities.Refresher.UseAbility();
                            else if (Abilities.RefresherShard != null && Abilities.RefresherShard.CanBeCasted &&
                                     Me.IsInAttackRange(Target) &&
                                     Abilities.AllAbilities.Count(
                                         x => x.Ability.AbilityState == AbilityState.OnCooldown) >=
                                     8)
                                Abilities.RefresherShard.UseAbility();
                        }
                    }

                    if (_sleeper.Sleeping("orbwalker_invoker"))
                        return;
                    _sleeper.Sleep(250, "orbwalker_invoker");
                    _main.Context.Orbwalker.Active.OrbwalkTo(Target);
                }
        }

        private bool CheckForEmpNearTarget(Unit target)
        {
            var emps = _main.Updater.EmpPositions;
            var tornadoTime = Abilities.Tornado.Duration;
            var delay = (float) Abilities.Tornado.Ability.GetHitDelay(Target);
            return (from empInfo in emps
                let pos = empInfo.Value
                where target.IsInRange(pos, 675)
                select 2.9 - (Game.RawGameTime - empInfo.Key)
                into timeLife
                select !(timeLife > tornadoTime + delay)).FirstOrDefault();
        }

        private bool CheckForCataclysm(int countForSs)
        {
            var enemyUndersEul = EntityManager<Hero>.Entities.Count(x =>
                x.IsValid && x.IsAlive && x.IsEnemy(Me) && x.IsVisible &&
                IsUnderInvulModifier(x));
            InvokerCrappahilationPaid.Log.Warn($"[Cataclysm] {enemyUndersEul} >= {countForSs}");
            return enemyUndersEul >= countForSs;
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
                EntityManager<Hero>.Entities.Where(x =>
                    x.IsAlive && x.IsAlly(target) && !x.IsIllusion && x.IsInRange(target, 500));
            if (enemies is Unit[] units)
                return enemies.Count() >= 3 && Abilities.Emp.CanHit(units);
            return enemies.Count() >= 3;
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
            UpdateManager.BeginInvoke(() =>
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

                            InvokerCrappahilationPaid.Log.Warn($"Id: {c.Id} [Selected={c.IsSelected}]");
                        }

                        _main.Config.ComboPanel.IsAutoComboSelected = true;
                        combo.AbilityInAction = 0;
                        InvokerCrappahilationPaid.Log.Warn(
                            $"Changed from Custom combo to Dynamic Combo [{GameplayType}]");
                        _blocked = false;
                        return;
                    }

                    combo.AbilityInAction = 0;
                }

                InvokerCrappahilationPaid.Log.Warn($"Changed from {from} to {combo.Items[combo.AbilityInAction]}");
                _blocked = false;
            }, 100);
        }

        private void SetComboAfterRefresher(ComboPanel.MyLittleCombo combo, bool casted = true)
        {
            if (!casted)
                return;
            var time = 50; //(int) Math.Max(Game.Ping + 50f, 250);
            //InvokerCrappahilationPaid.Log.Error($"[Time: {time}]");
            UpdateManager.BeginInvoke(() =>
            {
                AfterRefresher = true;
                combo.AbilityInAction = combo.NextAbilityAfterRefresher;
                InvokerCrappahilationPaid.Log.Warn($"Changed to {combo.Items[combo.AbilityInAction]} [Time: {time}]");
            }, time);
        }

        public bool IsDirectlyFacing(Unit source, Vector3 pos, float args = 0.025f)
        {
            var vector1 = pos - source.NetworkPosition;
            var diff = Math.Abs(Math.Atan2(vector1.Y, vector1.X) - source.RotationRad);
            return diff < args;
        }

        private ActiveAbility GetBestNextSpell()
        {
            return null;
        }

        private IEnumerable<BaseAbility> GetAviableAbilities()
        {
            var abilities = Me.Spellbook.Spells.Where(x => x.CanBeCasted() && _freeAbilities.Contains(x.Id) &&
                                                           (x.AbilitySlot == AbilitySlot.Slot_4 ||
                                                            x.AbilitySlot == AbilitySlot.Slot_5))
                .Select(z => _main.Context.AbilityFactory.GetAbility(z.Id));
            return abilities;
        }

        private bool GetTarget()
        {
            if (Target != null && Target.IsValid && Target.IsAlive) return true;
            Target = _main.Context.TargetSelector.Active.GetTargets().FirstOrDefault();
            try
            {
                if (_main.Config.ComboPanel.SelectedCombo != null)
                    _main.Config.ComboPanel.SelectedCombo.AbilityInAction = 0;
            }
            catch (Exception e)
            {
                InvokerCrappahilationPaid.Log.Error(e);
            }

            return false;
        }
    }
}