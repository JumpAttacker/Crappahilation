using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Divine;

using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker;
using Ensage.SDK.Service;
using SharpDX;

namespace InvokerCrappahilationPaid
{
    public class Updater
    {
        private readonly InvokerCrappahilationPaid _main;

        public Dictionary<float, Vector3> EmpPositions = new Dictionary<float, Vector3>();

        public Updater(InvokerCrappahilationPaid main)
        {
            _main = main;
            Units = new List<UnitUnderControl>();
            EntityManager.EntityAdded += (sender) =>
            {
                var unit = sender.Entity as Unit;
                if (unit == null)
                    return;
                if (unit.Name == "npc_dota_invoker_forged_spirit" && _main.Config.UseForges)
                {
                    if (Units.Find(x => x.Unit.Handle == unit.Handle) == null)
                        UpdateManager.BeginInvoke(500, () => { Units.Add(new UnitUnderControl(unit)); });
                }
                else if (unit.Name.Contains("npc_dota_necronomicon") && unit != _main.Me && unit.IsControllable &&
                         _main.Config.UseNecros)
                {
                    if (Units.Find(x => x.Unit.Handle == unit.Handle) == null)
                    {
                        UpdateManager.BeginInvoke(500, () => { Units.Add(new UnitUnderControl(unit)); });
                        if (_main.Config.AutoPurge) AutoPurge(unit);
                    }
                }
            };
            EntityManager.EntityRemoved += (sender) =>
            {
                var unit = sender.Entity as Unit;
                if (unit == null)
                    return;
                var find = Units.Find(x => x.Unit.Handle == unit.Handle);
                if (find != null) Units.Remove(find);
            };
            foreach (var unit in EntityManager.GetEntities<Unit>())
                if (unit.Name == "npc_dota_invoker_forged_spirit")
                {
                    if (Units.Find(x => x.Unit.Handle == unit.Handle) == null) Units.Add(new UnitUnderControl(unit));
                }
                else if (unit.Name.Contains("npc_dota_necronomicon") && unit != _main.Me && unit.IsControllable &&
                         _main.Config.UseNecros)
                {
                    if (Units.Find(x => x.Unit.Handle == unit.Handle) == null)
                        UpdateManager.BeginInvoke(500, () => { Units.Add(new UnitUnderControl(unit)); });
                    if (_main.Config.AutoPurge) AutoPurge(unit);
                }

            Entity.OnParticleEffectAdded += (sender, args) =>
            {
                if (args.Name == "particles/units/heroes/hero_invoker/invoker_emp.vpcf")
                    UpdateManager.BeginInvoke(10, () =>
                    {
                        var time = GameManager.RawGameTime;
                        EmpPositions.Add(time, args.ParticleEffect.GetControlPoint(0));
                        UpdateManager.BeginInvoke(2900, () =>
                        {
                            if (EmpPositions.ContainsKey(time))
                                EmpPositions.Remove(time);
                        });
                    });
            };
        }

        public List<UnitUnderControl> Units { get; set; }

        private void AutoPurge(Unit unit)
        {
            var sleeper = new Sleeper();
            UpdateManager.BeginInvoke(async () =>
            {
                while (unit != null && unit.IsValid && unit.IsAlive)
                {
                    var spell = unit.Spellbook.Spell1;
                    if (spell.CanBeCasted() && !sleeper.Sleeping)
                    {
                        var target = _main.Config.Main.Combo.Target ??
                                     _main.Config.Main.Context.TargetSelector?.Active.GetTargets().FirstOrDefault();
                        if (target != null && spell.CanHit(target))
                        {
                            spell.UseAbility(target);
                            sleeper.Sleep(500);
                        }
                    }

                    await Task.Delay(500);
                }
            }, 100);
        }

        public class UnitUnderControl
        {
            public IOrbwalkerManager Orbwalker;
            public Unit Unit;

            public UnitUnderControl(Unit unit)
            {
                Unit = unit;
                ///////////
                CanWork = true;
                ///////////
                /*Context = new EnsageServiceContext(unit);
                Context.TargetSelector.Activate();
                CanWork = false;
                var orbwalker = Context.GetExport<IOrbwalkerManager>().Value;

                UpdateManager.BeginInvoke(async () =>
                {
                    orbwalker.Activate();

                    orbwalker.Settings.DrawHoldRange.Value = false;
                    orbwalker.Settings.DrawRange.Value = false;

                    Orbwalker = orbwalker;

                    await Task.Delay(150);

                    try
                    {
                        Context.TargetSelector.Deactivate();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    CanWork = true;
                }, 150);*/
            }

            public bool CanWork { get; set; }

            public EnsageServiceContext Context { get; set; }
        }
    }
}