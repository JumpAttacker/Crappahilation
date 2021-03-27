namespace TechiesCrappahilationPaid.Managers
{
    public class DamageInfo
    {
        public float HealthAfterSuicide;
        public int LandMinesCount;
        public int LandMinesCountMax;
        public int RemoteMinesCount;
        public int RemoteMinesCountMax;

        public DamageInfo(int landMinesCount, int landMinesCountMax, int remoteMinesCountMax, int remoteMinesCount,
            float healthAfterSuicide)
        {
            LandMinesCount = landMinesCount;
            LandMinesCountMax = landMinesCountMax;
            RemoteMinesCountMax = remoteMinesCountMax;
            RemoteMinesCount = remoteMinesCount;
            HealthAfterSuicide = healthAfterSuicide;
        }

        public bool HeroWillDieSuicide => HealthAfterSuicide < 0;

        public void UpdateInfo(int landMinesCount, int landMinesCountMax, int remoteMinesCountMax, int remoteMinesCount,
            float healthAfterSuicide)
        {
            LandMinesCount = landMinesCount;
            LandMinesCountMax = landMinesCountMax;
            RemoteMinesCountMax = remoteMinesCountMax;
            RemoteMinesCount = remoteMinesCount;
            HealthAfterSuicide = healthAfterSuicide;
        }
    }
}