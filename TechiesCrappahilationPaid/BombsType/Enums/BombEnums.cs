namespace TechiesCrappahilationPaid.BombsType.Enums
{
    public static class BombEnums
    {
        public enum BombStatus
        {
            WillDetonate = 14,
            Idle = 6
        }

        public enum BombTypes
        {
            RemoveMine,
            StasisTrap,
            LandMine
        }

        public enum SpawnStatus
        {
            IsActive = 1500,
            NotActive = 1534
        }
    }
}