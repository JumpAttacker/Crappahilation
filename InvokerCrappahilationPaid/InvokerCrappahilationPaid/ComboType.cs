using Ensage.Common.Menu;
using Ensage.SDK.Menu;

namespace InvokerCrappahilationPaid
{
    public class ComboType
    {
        private readonly Config _config;

        public ComboType(Config config)
        {
            _config = config;
            var main = _config.Factory.Menu("Gameplay");
            GameplayType = main.Item("Type: ",
                new StringList("Quas + Exort (Damage)" /*, "Wex + Quas (Disable)", "Auto mode"*/));
        }

        public MenuItem<StringList> GameplayType { get; set; }
    }
}