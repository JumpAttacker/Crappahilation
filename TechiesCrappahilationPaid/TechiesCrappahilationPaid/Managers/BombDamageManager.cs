using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Abilities.npc_dota_hero_ember_spirit;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using TechiesCrappahilationPaid.BombsType;
using TechiesCrappahilationPaid.Helpers;
using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

namespace TechiesCrappahilationPaid.Managers
{
    public class BombDamageManager
    {
        private readonly Updater _updater;
        public readonly Dictionary<HeroId, DamageInfo> DamageDictionary = new Dictionary<HeroId, DamageInfo>();
        private readonly MultiSleeper _multiSleeper = new MultiSleeper();

        public BombDamageManager(Updater updater)
        {
            _updater = updater;
            var me = updater._main.Me;
            DamageUpdater = UpdateManager.Subscribe(() =>
            {
                foreach (var enemy in EntityManager<Hero>.Entities.Where(x =>
                    x.IsValid && x.IsVisible && x.IsAlive && x.IsEnemy(me) && !x.IsIllusion))
                {
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
            }, 100);
            var inActionSleeper = new MultiSleeper();
            UpdateManager.BeginInvoke(async () =>
            {
                while (true)
                {
                    var passedDelay = false;
                    starting:
                    var isForce = _updater.ForceStaff != null && _updater.ForceStaff.CanBeCasted;
                    var enemies = EntityManager<Hero>.Entities.Where(x =>
                        x.IsValid && x.IsVisible && x.IsAlive && x.IsEnemy(me) && !x.IsIllusion &&
                        !UnitExtensions.IsMagicImmune(x) &&
                        _updater._main.MenuManager.Targets.Value.IsEnabled(x.HeroId.ToString()));
                    foreach (var enemy in enemies)
                    {
                        var handle = enemy.Handle;
                        if (inActionSleeper.Sleeping(handle))
                            continue;
                        if (UnitExtensions.HasModifiers(enemy, new[]
                            {
                                "modifier_shredder_timber_chain", "modifier_storm_spirit_ball_lightning",
                                "modifier_item_aeon_disk_buff", "modifier_eul_cyclone",
                                "modifier_ember_spirit_sleight_of_fist_caster",
                                "modifier_ember_spirit_sleight_of_fist_caster_invulnerability",
                                "modifier_brewmaster_primal_split", "modifier_brewmaster_primal_split_delay",
                                "modifier_earth_spirit_rolling_boulder_caster", "modifier_morphling_waveform",
                                "modifier_phoenix_icarus_dive", "modifier_ursa_enrage"
                            },
                            false))
                            continue;
                        if (!enemy.CanDie(!_updater._main.MenuManager.DetonateOnAegis))
                            continue;
                        var itemBlink = enemy.GetItemById(AbilityId.item_blink);
                        if (itemBlink != null)
                        {
                            if (itemBlink.CooldownLength - itemBlink.Cooldown <= 1 && itemBlink.CooldownLength > 10)
                            {
                                continue;
                            }
                        }

                        var startManaCalc = 0f;
                        var threshold = 0f;
                        var heroId = enemy.HeroId;
                        var isDusa = heroId == HeroId.npc_dota_hero_medusa;
                        var isAbba = heroId == HeroId.npc_dota_hero_abaddon;
                        var health = (float) enemy.Health;
                        if (isDusa)
                        {
                            startManaCalc = enemy.Mana;
                            var shield = UnitExtensions.GetAbilityById(enemy, AbilityId.medusa_mana_shield);
                            if (shield.IsToggled)
                                threshold = shield.GetAbilityData("damage_per_mana");
                        }
                        else if (heroId == HeroId.npc_dota_hero_ember_spirit)
                        {
                            if (EmberShield == null)
                                EmberShield =
                                    new ember_spirit_flame_guard(
                                        UnitExtensions.GetAbilityById(enemy, AbilityId.ember_spirit_flame_guard));
                            if (enemy.HasAnyModifiers(EmberShield.ModifierName))
                            {
                                var extraLife = EmberShield.Ability.GetAbilityData("absorb_amount");
                                var talant = UnitExtensions.GetAbilityById(enemy,
                                    AbilityId.special_bonus_unique_ember_spirit_1);
                                extraLife += talant?.Level > 0
                                    ? talant.GetAbilityData("value")
                                    : 0;
                                health += extraLife;
                            }
                        }
                        else if (isAbba)
                        {
                        }


                        var raindrop = enemy.GetItemById(AbilityId.item_infused_raindrop);
                        if (raindrop != null && raindrop.CanBeCasted())
                        {
                            var extraHealth = raindrop.GetAbilityData("magic_damage_block");
                            health += extraHealth;
                        }

                        var refraction = enemy.GetModifierByName("modifier_templar_assassin_refraction_absorb");
                        var blockCount = refraction?.StackCount;
                        var graveKeeper = enemy.GetModifierByName("modifier_visage_gravekeepers_cloak");
                        var graveKeeperCount = graveKeeper?.StackCount;
                        var aeon = enemy.GetItemById(AbilityId.item_aeon_disk);
                        var breakHealthForAeon = enemy.MaximumHealth * .8f;
                        var predictedPosition = enemy.Predict(250f);
                        var pos = _updater._main.MenuManager.UsePrediction ? predictedPosition : enemy.Position;
                        var detList = new List<RemoteMine>();
                        var bombs = updater.BombManager.RemoteMines.Where(x =>
                            x.IsActive && x.Position.IsInRange(pos, 420)).ToList();
                        var underStasisTrap = enemy.HasAnyModifiers("modifier_techies_stasis_trap_stunned");
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
                                    (float) (UnitExtensions.GetAbilityById(enemy,
                                                     AbilityId.visage_gravekeepers_cloak)
                                                 .GetAbilitySpecialData("damage_reduction") *
                                             graveKeeperCount);
                                TechiesCrappahilationPaid.Log.Warn(
                                    $"Block:  {damage * (percentBlock / 100)}({percentBlock} %). Left blocks: {graveKeeperCount} " +
                                    $"Damage changed: {damage} -> {damage - damage * (percentBlock / 100)}");
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
                                    Game.ExecuteCommand($"dota_camera_set_lookatpos {consolePosition}");
                                }

                                if (_updater._main.MenuManager.DelayOnDetonate.Value.Value > 0 && !passedDelay && !underStasisTrap)
                                {
                                    if (!_multiSleeper.Sleeping("heroAfterForce" + heroId))
                                    {
                                        TechiesCrappahilationPaid.Log.Warn(
                                            "delay on detonation start for " +
                                            _updater._main.MenuManager.DelayOnDetonate.Value.Value);
                                        await Task.Delay(_updater._main.MenuManager.DelayOnDetonate.Value.Value);
                                        TechiesCrappahilationPaid.Log.Warn("delay end");
                                        passedDelay = true;
                                        goto starting;
                                    }
                                }

                                await Task.Delay((int) (25 + Game.Ping));
                                if (_updater._main.MenuManager.DetonateAllInOnce && !aeonByPass)
                                {
                                    if (_updater._main.MenuManager.UseFocusedDetonation)
                                    {
                                        var minesToDetonate = bombs.FirstOrDefault();
                                        if (minesToDetonate != null)
                                            _updater._main.FocusedDetonate.UseAbility(minesToDetonate.Position);
                                    }
                                    else
                                    {
                                        foreach (var mine in bombs)
                                        {
                                            Player.SelectEntity(mine.Owner, true);
                                        }

                                        await Task.Delay((int) (25 + Game.Ping));
                                        foreach (var mine in bombs)
                                        {
                                            mine.Owner.Spellbook.Spell1.UseAbility();
                                            mine.IsActive = false;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var mine in detList)
                                    {
                                        Player.SelectEntity(mine.Owner, true);
                                    }

                                    await Task.Delay((int) (25 + Game.Ping));
                                    foreach (var mine in detList)
                                    {
                                        mine.Owner.Spellbook.Spell1.UseAbility();
                                        mine.IsActive = false;
                                    }
                                }

                                inActionSleeper.Sleep(extraDetonateTime + Game.Ping + 1500, handle);


                                ////TODO: delay on detonation
                                goto EndOfActions;
                            }
                        }

                        if (isDusa)
                        {
                            startManaCalc = enemy.Mana;
                        }

                        if (isForce && _updater.ForceStaff.CanHit(enemy) && !Prediction.IsTurning(enemy))
                        {
                            var isLinken = UnitExtensions.IsLinkensProtected(enemy);
                            if (isLinken && (_updater.Eul == null || !_updater.Eul.CanBeCasted))
                            {
                                continue;
                            }

                            var afterForcePos = Prediction.InFront(enemy, 600);
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
                                    if (isLinken)
                                        _updater.Eul.UseAbility(enemy);
                                    _updater.ForceStaff.UseAbility(enemy);
                                    _multiSleeper.Sleep(500, "heroAfterForce" + heroId);
                                }
                            }
                        }

                        EndOfActions: ;
                    }

                    await Task.Delay(1);
                }
            });
        }

        public ember_spirit_flame_guard EmberShield { get; set; }

        public IUpdateHandler DamageUpdater { get; set; }

        public int CalcLandMineCount(Hero target, bool isCurrentLife = true)
        {
            var life = isCurrentLife ? target.Health : target.MaximumHealth;
            var damageAfterFirstBomb = _updater._main.LandMine.GetDamage(target);
            if (damageAfterFirstBomb <= 0)
                return 0;
            if (target.HeroId == HeroId.npc_dota_hero_medusa)
            {
                var shield = UnitExtensions.GetAbilityById(target, AbilityId.medusa_mana_shield);
                if (shield.IsToggled)
                {
                    var treshold =
                        shield.GetAbilityData("damage_per_mana");
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
                var shield = UnitExtensions.GetAbilityById(target, AbilityId.medusa_mana_shield);
                if (shield.IsToggled)
                {
                    var treshold =
                        shield.GetAbilityData("damage_per_mana");
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
                var shield = UnitExtensions.GetAbilityById(target, AbilityId.medusa_mana_shield);
                if (shield.IsToggled)
                {
                    var treshold =
                        shield.GetAbilityData("damage_per_mana");
                    DamageCalcHelpers.CalcDamageForDusa(ref damageAfterFirstBomb, target, treshold);
                }
            }
            else if (target.HeroId == HeroId.npc_dota_hero_visage)
            {
                var graveKeeper = target.GetModifierByName("modifier_visage_gravekeepers_cloak");
                var graveKeeperCount = graveKeeper?.StackCount;
                if (!(graveKeeperCount > 0)) return life - damageAfterFirstBomb;
                var percentBlock =
                    (float) (UnitExtensions.GetAbilityById(target,
                                 AbilityId.visage_gravekeepers_cloak).GetAbilitySpecialData("damage_reduction") *
                             graveKeeperCount);
                damageAfterFirstBomb -= damageAfterFirstBomb * (percentBlock / 100);
            }

            return life - damageAfterFirstBomb;
        }
    }
}