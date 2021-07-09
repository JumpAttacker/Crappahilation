using System;

using SharpDX;

namespace InvokerCrappahilationPaid
{
    public static class Extensions
    {
        public static float Dot(this Vector3 a, Vector3 b)
        {
            return (float) Math.Cos(a.AngleBetween(b));
        }

        public static float Distance2D(this Entity entity)
        {
            var entityPosition = entity.NetworkPosition;
            return (float) Math.Sqrt(Math.Pow(entityPosition.X, 2) + Math.Pow(entityPosition.Y, 2));
        }

        public static float Distance2D(this Vector3 entityPosition)
        {
            return (float) Math.Sqrt(Math.Pow(entityPosition.X, 2) + Math.Pow(entityPosition.Y, 2));
        }

        public static float GetManaPercent(this Hero unit)
        {
            return unit.Mana / unit.MaximumMana;
        }
    }
}