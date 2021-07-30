using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Divine;
using Divine.Entity;
using Divine.Entity.Entities;
using Divine.Entity.Entities.Units;
using Divine.Game;
using Divine.Numerics;
using Divine.Particle;
using Divine.Update;

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
                // else if (unit.Name.Contains("npc_dota_necronomicon") && unit != _main.Me && unit.IsControllable &&
                //          _main.Config.UseNecros)
                // {
                //     if (Units.Find(x => x.Unit.Handle == unit.Handle) == null)
                //     {
                //         UpdateManager.BeginInvoke(500, () => { Units.Add(new UnitUnderControl(unit)); });
                //         if (_main.Config.AutoPurge) AutoPurge(unit);
                //     }
                // }
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
                // else if (unit.Name.Contains("npc_dota_necronomicon") && unit != _main.Me && unit.IsControllable &&
                //          _main.Config.UseNecros)
                // {
                //     if (Units.Find(x => x.Unit.Handle == unit.Handle) == null)
                //         UpdateManager.BeginInvoke(500, () => { Units.Add(new UnitUnderControl(unit)); });
                //     if (_main.Config.AutoPurge) AutoPurge(unit);
                // }

                ParticleManager.ParticleAdded += (args) =>
            {
                if (args.Particle.Name == "particles/units/heroes/hero_invoker/invoker_emp.vpcf")
                    UpdateManager.BeginInvoke(10, () =>
                    {
                        var time = GameManager.RawGameTime;
                        EmpPositions.Add(time, args.Particle.GetControlPoint(0));
                        UpdateManager.BeginInvoke(2900, () =>
                        {
                            if (EmpPositions.ContainsKey(time))
                                EmpPositions.Remove(time);
                        });
                    });
            };
        }

        public List<UnitUnderControl> Units { get; set; }


        public class UnitUnderControl
        {
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

        }
    }
}