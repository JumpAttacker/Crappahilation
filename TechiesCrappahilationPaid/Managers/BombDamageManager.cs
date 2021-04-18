using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Divine;
using Divine.SDK.Extensions;
using Divine.SDK.Helpers;
using Divine.SDK.Prediction;
using Divine.SDK.Prediction.Collision;
using O9K.Core.Entities.Heroes;
using SharpDX;
using TechiesCrappahilationPaid.BombsType;
using TechiesCrappahilationPaid.Helpers;

namespace TechiesCrappahilationPaid.Managers
{
    public class BombDamageManager
    {
        private readonly Updater _updater;

        public readonly Dictionary<HeroId, DamageInfo> DamageDictionary = new Dictionary<HeroId, DamageInfo>();
        // private readonly MultiSleeper<string> _multiSleeper;

        public BombDamageManager(Updater updater)
        {
            _updater = updater;
            var me = updater._main.Me;
            DamageUpdater = UpdateManager.CreateUpdate(100, () =>
            {
                // O9K.Core.Managers.Entity.EntityManager9.EnemyHeroes
                foreach (var enemy in EntityManager.GetEntities<Hero>().Where(x =>
                    x.IsValid && x.IsVisible && x.IsAlive && x.IsEnemy(me) && !x.IsIllusion))
                {
                    // var t = new O9K.Core.Entities.Heroes.Hero9(enemy);
                    var id = enemy.HeroId;
                    var landMineCount = CalcLandMineCount(enemy);
                    var landMineCountMax = CalcLandMineCount(enemy, false);
                    var removeMineCount = CalcRemoteMineCount(enemy);
                    var removeMineCountMax = CalcRemoteMineCount(enemy, false);
                    var healthAfterSuicide = CalcHealthAfterSuicide(enemy);

                    if (DamageDictionary.TryGetValue(id, out var info))
                        info.UpdateInfo(landMineCount, landMineCountMax, removeMineCountMax, removeMineCount,
                            healthAfterSuicide);
                    else
                        DamageDictionary.Add(id,
                            new DamageInfo(landMineCount, landMineCountMax, removeMineCountMax, removeMineCount,
                                healthAfterSuicide));
                }
            });
            // var inActionSleeper = new MultiSleeper<uint>();
            UpdateManager.BeginInvoke(async () =>
            {
                while (true)
                {
                    var passedDelay = false;
                    starting:
                    var isForce = _updater.ForceStaff != null && _updater.ForceStaff.CanBeCasted();
                    var enemies = O9K.Core.Managers.Entity.EntityManager9.EnemyHeroes.OfType<Hero9>().Where(x =>
                        x.IsValid && x.IsAlive && x.IsVisible && !x.IsIllusion && !x.IsMagicImmune && x.CanDie);
                    // var enemies = EntityManager.GetEntities<Hero>().Where(x =>
                    //     x.IsValid && x.IsVisible && x.IsAlive && x.IsEnemy(me) && !x.IsIllusion &&
                    //     !x.IsMagicImmune()/* &&
                    //     _updater._main.MenuManager.Targets.GetValue(x.HeroId)*/);
                    foreach (var enemy in enemies)
                    {
                        var handle = enemy.Handle;

                        if (MultiSleeper<uint>.Sleeping(handle))
                            continue;
                        if (enemy.HasModifier(new[]
                        {
                            "modifier_shredder_timber_chain", "modifier_storm_spirit_ball_lightning",
                            "modifier_item_aeon_disk_buff", "modifier_eul_cyclone",
                            "modifier_ember_spirit_sleight_of_fist_caster",
                            "modifier_ember_spirit_sleight_of_fist_caster_invulnerability",
                            "modifier_brewmaster_primal_split", "modifier_brewmaster_primal_split_delay",
                            "modifier_earth_spirit_rolling_boulder_caster", "modifier_morphling_waveform",
                            "modifier_phoenix_icarus_dive", "modifier_ursa_enrage"
                        }))
                            continue;
                        if (enemy.CanReincarnate && !_updater._main.MenuManager.DetonateOnAegis)
                        {
                            continue;
                        }

                        var itemBlink = enemy.Abilities.FirstOrDefault(x => x.Id == AbilityId.item_blink);
                        if (itemBlink != null)
                        {
                            if (itemBlink.Cooldown - itemBlink.RemainingCooldown <= 1 && itemBlink.Cooldown > 10)
                            {
                                continue;
                            }
                        }

                        var startManaCalc = 0f;
                        var threshold = 0f;
                        var heroId = enemy.Id;
                        var isDusa = heroId == HeroId.npc_dota_hero_medusa;
                        var isAbba = heroId == HeroId.npc_dota_hero_abaddon;
                        var health = (float) enemy.Health;
                        if (isDusa)
                        {
                            startManaCalc = enemy.Mana;
                            var shield = enemy.GetAbilityById(AbilityId.medusa_mana_shield);
                            if (shield.IsToggled)
                                threshold = shield.GetAbilitySpecialData("damage_per_mana");
                        }
                        else if (heroId == HeroId.npc_dota_hero_ember_spirit)
                        {
                            if (EmberShield == null)
                                EmberShield =
                                    enemy.GetAbilityById(AbilityId.ember_spirit_flame_guard);
                            if (enemy.HasModifier("modifier_ember_spirit_flame_guard"))
                            {
                                var extraLife = EmberShield.GetAbilitySpecialData("absorb_amount");
                                var talant = enemy.GetAbilityById(AbilityId.special_bonus_unique_ember_spirit_1);
                                extraLife += talant?.Level > 0
                                    ? talant.GetAbilitySpecialData("value")
                                    : 0;
                                health += extraLife;
                            }
                        }
                        else if (isAbba)
                        {
                        }


                        var raindrop = enemy.Abilities.FirstOrDefault(x => x.Id == AbilityId.item_infused_raindrop);
                        if (raindrop != null && raindrop.CanBeCasted())
                        {
                            var extraHealth = raindrop.BaseAbility.GetAbilitySpecialData("magic_damage_block");
                            health += extraHealth;
                        }

                        var blockCount = enemy.GetModifierStacks("modifier_templar_assassin_refraction_absorb");
                        var graveKeeper = enemy.GetModifier("modifier_visage_gravekeepers_cloak");
                        var graveKeeperCount = graveKeeper?.StackCount;
                        var aeon = enemy.Abilities.FirstOrDefault(x => x.Id == AbilityId.item_aeon_disk);
                        var breakHealthForAeon = enemy.MaximumHealth * .7f;
                        var input = new PredictionInput
                        {
                            Owner = me,
                            AreaOfEffect = false,
                            CollisionTypes = CollisionTypes.None,
                            Delay = 0.25f,
                            Speed = float.MaxValue,
                            Range = float.MaxValue,
                            Radius = 420,
                            PredictionSkillshotType = PredictionSkillshotType.SkillshotCircle
                        };
                        input = input.WithTarget(enemy.BaseHero);
                        var prediction = PredictionManager.GetPrediction(input);
                        // ParticleManager.CircleParticle("123", prediction.CastPosition, 150, Color.Red);
                        var predictedPosition = prediction.CastPosition;
                        var pos = _updater._main.MenuManager.UsePrediction ? predictedPosition : enemy.Position;
                        var detList = new List<RemoteMine>();
                        var bombs = updater.BombManager.RemoteMines.Where(x =>
                            x.IsActive && x.Position.IsInRange(pos, 420)).ToList();
                        var underStasisTrap = enemy.HasModifier("modifier_techies_stasis_trap_stunned");
                        foreach (var remoteMine in bombs.Where(x =>
                            x.StackerMain == null && x.Stacker.DetonateDict.TryGetValue(heroId, out var isEnable) &&
                            isEnable ||
                            x.StackerMain != null &&
                            x.StackerMain.Stacker.DetonateDict.TryGetValue(heroId, out var isEnable2) && isEnable2))
                        {
                            var damage = _updater._main.RemoteMine.GetDamage(remoteMine.Damage, enemy);
                            if (isDusa)
                                DamageCalcHelpers.CalcDamageForDusa(ref damage, ref startManaCalc, threshold);
                            detList.Add(remoteMine);
                            if (blockCount > 0)
                            {
                                blockCount--;
                            }
                            else if (graveKeeperCount > 0)
                            {
                                var percentBlock =
                                    (float) (enemy.GetAbilityById(AbilityId.visage_gravekeepers_cloak)
                                                 .GetAbilitySpecialData("damage_reduction") *
                                             graveKeeperCount);
                                // TechiesCrappahilationPaid.Log.Warn(
                                //     $"Block:  {damage * (percentBlock / 100)}({percentBlock} %). Left blocks: {graveKeeperCount} " +
                                //     $"Damage changed: {damage} -> {damage - damage * (percentBlock / 100)}");
                                health -= damage - damage * (percentBlock / 100);
                                graveKeeperCount--;
                            }
                            else
                            {
                                health -= damage;
                            }

                            var extraDetonateTime = 0.25f * detList.Count;
                            var aeonByPass = aeon != null && aeon.CanBeCasted() && health < breakHealthForAeon;


                            if (health + enemy.HealthRegeneration * extraDetonateTime < 0 || aeonByPass)
                            {
                                if (_updater._main.MenuManager.CameraMove)
                                {
                                    var heroPos = enemy.Position;
                                    var consolePosition = $"{heroPos.X} {heroPos.Y}";
                                    GameManager.ExecuteCommand($"dota_camera_set_lookatpos {consolePosition}");
                                }

                                if (_updater._main.MenuManager.DelayOnDetonate.Value > 0 && !passedDelay &&
                                    !underStasisTrap)
                                {
                                    if (!MultiSleeper<string>.Sleeping("heroAfterForce" + heroId))
                                    {
                                        // TechiesCrappahilationPaid.Log.Warn(
                                        //     "delay on detonation start for " +
                                        //     _updater._main.MenuManager.DelayOnDetonate.Value.Value);
                                        await Task.Delay(_updater._main.MenuManager.DelayOnDetonate.Value);
                                        // TechiesCrappahilationPaid.Log.Warn("delay end");
                                        passedDelay = true;
                                        goto starting;
                                    }
                                }

                                await Task.Delay((int) (25 + GameManager.Ping));
                                if (_updater._main.MenuManager.DetonateAllInOnce && !aeonByPass)
                                {
                                    if (_updater._main.MenuManager.UseFocusedDetonation)
                                    {
                                        var minesToDetonate = bombs.FirstOrDefault();
                                        if (minesToDetonate != null)
                                            _updater._main.FocusedDetonate.Cast(minesToDetonate.Position);
                                    }
                                    else
                                    {
                                        foreach (var mine in bombs)
                                        {
                                            mine.Owner.Select(true);
                                        }

                                        await Task.Delay((int) (25 + GameManager.Ping));
                                        // OrderManager.CreateOrder(OrderType.Cast, bombs.Select(x => x.Owner), 0,
                                        //     bombs.FirstOrDefault().Owner.Spellbook.Spell1.AbilityIndex, Vector3.Zero,
                                        //     false, false, false);
                                        foreach (var mine in bombs)
                                        {
                                            mine.Owner.Spellbook.Spell1.Cast();
                                            mine.IsActive = false;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var mine in detList)
                                    {
                                        mine.Owner.Select(true);
                                    }

                                    await Task.Delay((int) (25 + GameManager.Ping));
                                    // OrderManager.CreateOrder(OrderType.Cast, detList.Select(x => x.Owner).FirstOrDefault(), 0,
                                    //     detList.FirstOrDefault().Owner.Spellbook.Spell1.AbilityIndex, Vector3.Zero,
                                    //     false, false, false);
                                    foreach (var mine in detList)
                                    {
                                        mine.Owner.Spellbook.Spell1.Cast();
                                        mine.IsActive = false;
                                    }
                                }

                                MultiSleeper<uint>.Sleep(handle, extraDetonateTime + GameManager.Ping + 1500);


                                ////TODO: delay on detonation
                                goto EndOfActions;
                            }
                        }

                        if (isDusa)
                        {
                            startManaCalc = enemy.Mana;
                        }

                        if (isForce && _updater.ForceStaff.CanHit(enemy) && !enemy.IsRotating)
                        {
                            var isLinken = enemy.IsLinkensProtected;
                            if (isLinken && (_updater.Eul == null || !_updater.Eul.CanBeCasted()))
                            {
                                continue;
                            }

                            health = enemy.Health;
                            var afterForcePos = enemy.BaseEntity.InFrontSuper(600);
                            foreach (var remoteMine in updater
                                .BombManager.RemoteMines
                                .Where(x => x.IsActive &&
                                            x.Position.IsInRange(
                                                afterForcePos,
                                                420) &&
                                            (
                                                x.StackerMain == null &&
                                                x.Stacker.DetonateDict.TryGetValue(heroId, out var isEnable) &&
                                                isEnable ||
                                                x.StackerMain != null &&
                                                x.StackerMain.Stacker.DetonateDict.TryGetValue(heroId,
                                                    out var isEnable2) && isEnable2
                                            )))
                            {
                                var damage = _updater._main.RemoteMine.GetDamage(remoteMine.Damage, enemy);
                                if (isDusa)
                                    DamageCalcHelpers.CalcDamageForDusa(ref damage, ref startManaCalc, threshold);
                                detList.Add(remoteMine);
                                health -= damage;
                                if (health + enemy.HealthRegeneration * 0.25f * detList.Count < 0)
                                {
                                    if (MultiSleeper<string>.Sleeping("force" + heroId))
                                        continue;
                                    if (isLinken)
                                        _updater.Eul.UseAbility(enemy);
                                    _updater.ForceStaff.UseAbility(enemy);

                                    if (enemy.CanBecomeMagicImmune && _updater.Hex != null &&
                                        _updater.Hex.CanHit(enemy) &&
                                        _updater.Hex.CanBeCasted())
                                    {
                                        _updater.Hex.UseAbility(enemy);
                                    }

                                    MultiSleeper<string>.Sleep("heroAfterForce" + heroId, 500);
                                    MultiSleeper<string>.Sleep("force" + heroId, 500);
                                }
                            }
                        }

                        EndOfActions: ;
                    }

                    await Task.Delay(1);
                }
            });
        }

        public Ability EmberShield { get; set; }

        public UpdateHandler DamageUpdater { get; set; }

        public int CalcLandMineCount(Hero target, bool isCurrentLife = true)
        {
            var life = isCurrentLife ? target.Health : target.MaximumHealth;
            var damageAfterFirstBomb = _updater._main.LandMine.GetDamage(target);
            if (damageAfterFirstBomb <= 0)
                return 0;
            if (target.HeroId == HeroId.npc_dota_hero_medusa)
            {
                var shield = target.GetAbilityById(AbilityId.medusa_mana_shield);
                if (shield.IsToggled)
                {
                    var treshold =
                        shield.GetAbilitySpecialData("damage_per_mana");
                    DamageCalcHelpers.CalcDamageForDusa(ref damageAfterFirstBomb, target, treshold);
                }
            }

            var count = (int) Math.Ceiling(life / damageAfterFirstBomb);
            return count;
        }

        public int CalcRemoteMineCount(Hero target, bool isCurrentLife = true)
        {
            var life = isCurrentLife ? target.Health : target.MaximumHealth;
            var damageAfterFirstBomb = _updater._main.RemoteMine.GetDamage(target);
            if (damageAfterFirstBomb <= 0)
                return 0;
            if (target.HeroId == HeroId.npc_dota_hero_medusa)
            {
                var shield = target.GetAbilityById(AbilityId.medusa_mana_shield);
                if (shield.IsToggled)
                {
                    var treshold =
                        shield.GetAbilitySpecialData("damage_per_mana");
                    DamageCalcHelpers.CalcDamageForDusa(ref damageAfterFirstBomb, target, treshold);
                }
            }


            var count = (int) Math.Ceiling(life / damageAfterFirstBomb);
            return count;
        }

        public float CalcHealthAfterSuicide(Hero target, bool isCurrentLife = true)
        {
            var life = isCurrentLife ? target.Health : target.MaximumHealth;
            var damageAfterFirstBomb = _updater._main.Suicide.GetDamage(target);
            if (damageAfterFirstBomb <= 0)
                return 0;
            if (target.HeroId == HeroId.npc_dota_hero_medusa)
            {
                var shield = target.GetAbilityById(AbilityId.medusa_mana_shield);
                if (shield.IsToggled)
                {
                    var treshold =
                        shield.GetAbilitySpecialData("damage_per_mana");
                    DamageCalcHelpers.CalcDamageForDusa(ref damageAfterFirstBomb, target, treshold);
                }
            }
            else if (target.HeroId == HeroId.npc_dota_hero_visage)
            {
                var graveKeeper = target.GetModifierByName("modifier_visage_gravekeepers_cloak");
                var graveKeeperCount = graveKeeper?.StackCount;
                if (!(graveKeeperCount > 0)) return life - damageAfterFirstBomb;
                var percentBlock =
                    (float) (target.GetAbilityById(AbilityId.visage_gravekeepers_cloak)
                                 .GetAbilitySpecialData("damage_reduction") *
                             graveKeeperCount);
                damageAfterFirstBomb -= damageAfterFirstBomb * (percentBlock / 100);
            }

            return life - damageAfterFirstBomb;
        }
    }
}