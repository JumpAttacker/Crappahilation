using System.Collections.Generic;
<<<<<<< HEAD
using System.Linq;
using System.Threading.Tasks;

using Divine;

=======
using Divine;
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893
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
            // Units = new List<UnitUnderControl>();
            // EntityManager.EntityAdded += (sender) =>
            // {
            //     var unit = sender.Entity as Unit;
            //     if (unit == null)
            //         return;
            //     if (unit.Name == "npc_dota_invoker_forged_spirit" && _main.Config.UseForges)
            //     {
            //         if (Units.Find(x => x.Unit.Handle == unit.Handle) == null)
            //             UpdateManager.BeginInvoke(500, () => { Units.Add(new UnitUnderControl(unit)); });
            //     }
            // };
            // EntityManager.EntityRemoved += (sender) =>
            // {
            //     var unit = sender.Entity as Unit;
            //     if (unit == null)
            //         return;
            //     var find = Units.Find(x => x.Unit.Handle == unit.Handle);
            //     if (find != null) Units.Remove(find);
            // };
            // foreach (var unit in EntityManager.GetEntities<Unit>())
            //     if (unit.Name == "npc_dota_invoker_forged_spirit")
            //     {
            //         if (Units.Find(x => x.Unit.Handle == unit.Handle) == null) Units.Add(new UnitUnderControl(unit));
            //     }

            ParticleManager.ParticleAdded += (args) =>
            {
                var particle = args.Particle;
                
                if (particle.Name == "particles/units/heroes/hero_invoker/invoker_emp.vpcf")
                    UpdateManager.BeginInvoke(10, () =>
                    {
                        var time = GameManager.RawGameTime;
                        var key = "EMP" + time;
                        ParticleManager.CircleParticle(key, particle.Position, 675, Color.Purple);
                        UpdateManager.BeginInvoke(2900, () =>
                        {
                            ParticleManager.RemoveParticle(key);
                        });
                    });
            };
        }
    }
}