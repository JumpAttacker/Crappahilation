using System;
using Divine.Renderer;
using Divine.Entity.Entities.Units.Heroes;

namespace TechiesCrappahilationPaid.Features.ViewDamageFromBombs
{
    public abstract class ViewBombCountBase : IViewBombCount, IDisposable
    {
        protected readonly TechiesCrappahilationPaid Main;
        protected readonly ViewManager ViewManager;

        protected ViewBombCountBase(ViewManager main)
        {
            Main = main.Main;
            ViewManager = main;
            ViewManager.EnabledList = ViewManager.GetEnabledAbilities();
        }

        protected Hero Me => Main.Me;

        public void Dispose()
        {
            RendererManager.Draw -= Draw;
        }

        public abstract void Draw();
    }
}