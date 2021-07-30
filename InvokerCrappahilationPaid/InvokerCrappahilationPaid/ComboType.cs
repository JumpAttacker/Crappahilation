using Divine.Menu.Items;

namespace InvokerCrappahilationPaid
{
    public class ComboType
    {
        private readonly Config _config;

        public ComboType(Config config)
        {
            _config = config;
            var main = _config.Factory.CreateMenu("Gameplay");
            GameplayType = main.CreateSelector("Type", new[] {"Quas + Exort (Damage)"});
        }

        public MenuSelector GameplayType { get; set; }
    }
}