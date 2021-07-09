using System.Collections.Generic;
using System.Threading.Tasks;

using SharpDX;

namespace InvokerCrappahilationPaid
{
    public class NavMeshHelper
    {
        private const int StaticZ = 256;
        private readonly Dictionary<Unit, uint> _units = new Dictionary<Unit, uint>();
        public NavMeshPathfinding Pathfinding = new NavMeshPathfinding();

        public NavMeshHelper(InvokerCrappahilationPaid main)
        {
            MainUnit = main.Me;

            EntityManager<Unit>.EntityAdded += (sender, unit) => { AddUnitToSystem(unit); };

            UpdateManager.BeginInvoke(() =>
            {
                foreach (var unit in EntityManager<Unit>.Entities) AddUnitToSystem(unit);
            }, 1000);
        }

        public Hero MainUnit { get; set; }

        private void AddUnitToSystem(Unit unit)
        {
            if (_units.ContainsKey(unit) || !unit.IsSpawned || !unit.IsAlive) return;
            var nullable = AddObstacle(unit.NetworkPosition, unit.HullRadius);
            if (!nullable.HasValue) return;
            _units.Add(unit, nullable.Value);
            UpdateManager.BeginInvoke(async () =>
            {
                while (unit.IsValid && unit.IsAlive)
                {
                    UpdateObstacle(nullable.Value, unit.NetworkPosition, unit.HullRadius);
                    await Task.Delay(100);
                }

                _units.Remove(unit);
                RemoveObstacle(nullable.Value);
            });
        }

        public uint? AddObstacle(Vector3 position, float radius, uint? obstacle = null)
        {
            if (obstacle.HasValue)
                RemoveObstacle(obstacle.Value);
            return Pathfinding.AddObstacle(position.SetZ(StaticZ), radius);
        }

        public void UpdateObstacle(uint id, Vector3 position, float radius)
        {
            Pathfinding.UpdateObstacle(id, position.SetZ(StaticZ), radius);
        }

        public void RemoveObstacle(uint id)
        {
            Pathfinding.RemoveObstacle(id);
        }
    }
}