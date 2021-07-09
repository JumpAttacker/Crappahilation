using Divine.Numerics;
using Divine.Renderer;

namespace TechiesCrappahilationPaid.Features.ViewDamageFromBombs
{
    public class ViewOnTopPanel : ViewBombCountBase
    {
        public ViewOnTopPanel(ViewManager main) : base(main)
        {
        }

        public override void Draw()
        {
            RendererManager.DrawText("not implemented yet", new Vector2(25, 150), Color.Red, 35f);
        }
    }
}