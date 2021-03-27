using System;
using Ensage;
using Ensage.SDK.Renderer;

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
        protected IRenderManager Renderer => Main.Context.RenderManager;

        public void Dispose()
        {
            Renderer.Draw -= Draw;
        }

        public abstract void Draw(IRenderer renderer);
    }
}