using Ensage.SDK.Renderer;
using SharpDX;
using Color = System.Drawing.Color;

namespace TechiesCrappahilationPaid.Features.ViewDamageFromBombs
{
    public class ViewOnTopPanel : ViewBombCountBase
    {
        public ViewOnTopPanel(ViewManager main) : base(main)
        {
        }

        public override void Draw(IRenderer renderer)
        {
            renderer.DrawText(new Vector2(25, 150), "not implemented yet", Color.Red, 35f);
        }
    }
}