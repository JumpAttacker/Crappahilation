using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;

namespace TechiesCrappahilationPaid.Managers
{
    public sealed class DamageChecker
    {
        public delegate void AghanimStatus(bool dropped);

        private readonly Updater _updater;

        public DamageChecker(Updater updater)
        {
            _updater = updater;

            UpdateManager.Subscribe(Callback, 1000);
        }

        private Hero Me => _updater._main.Me;

        private Item Aghanim { get; set; }

        public event AghanimStatus OnAghanimStatusChanging = delegate { };

        private void Callback()
        {
            if (Aghanim != null && Aghanim.IsValid)
            {
                Aghanim = Me.GetItemById(AbilityId.item_ultimate_scepter);
                if (Aghanim == null) OnOnAghanimStatusChanging(true);
                return;
            }

            if (Aghanim != null && !Aghanim.IsValid) OnOnAghanimStatusChanging(true);
            Aghanim = Me.GetItemById(AbilityId.item_ultimate_scepter);
            if (Aghanim != null && Aghanim.IsValid)
                OnOnAghanimStatusChanging(false);
        }

        private void OnOnAghanimStatusChanging(bool dropped)
        {
            TechiesCrappahilationPaid.Log.Warn($"[AghanimStatus] {(dropped ? "Dropped" : "TakeItBoy")}");
            OnAghanimStatusChanging?.Invoke(dropped);
        }
    }
}