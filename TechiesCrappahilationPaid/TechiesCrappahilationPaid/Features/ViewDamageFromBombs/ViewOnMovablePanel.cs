using System.Linq;
using Ensage;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer;
using SharpDX;
using Color = System.Drawing.Color;

namespace TechiesCrappahilationPaid.Features.ViewDamageFromBombs
{
    public class ViewOnMovablePanel : ViewBombCountBase
    {
        private readonly ViewManager _main;
        private readonly Vector2 _iconSize;
        private Vector2 PanelPosition => new Vector2(100 + _main.PositionX, 500 + _main.PositionY);

        public ViewOnMovablePanel(ViewManager main) : base(main)
        {
            _main = main;
            _iconSize = new Vector2(25);
        }

        public override void Draw(IRenderer renderer)
        {
            var enemies = EntityManager<Player>.Entities.Where(x =>
                x.IsValid && x.Team != Me.Team && x.Hero != null && x.Hero.IsValid) /*.OrderBy(x => x.Id)*/;
            var enumerable = enemies as Player[] ?? enemies.ToArray();
            var count = enumerable.Count() + 1;
            var currentLoc = PanelPosition;
            var width = _iconSize.X * (ViewManager.EnabledCount * 2 + 1);
            renderer.DrawFilledRectangle(
                new RectangleF(PanelPosition.X, PanelPosition.Y, width, count * _iconSize.Y),
                Color.FromArgb(50, 50, 50, 50),
                Color.Black);
            DrawHorizontalsIcons(renderer, currentLoc, _iconSize, ViewManager.EnabledList);

            foreach (var enemy in enumerable.Select(x => x.Hero).Where(x =>
                Main.Updater.BombDamageManager.DamageDictionary.ContainsKey(x.HeroId)))
            {
                var heroId = enemy.HeroId;
                currentLoc.Y += 25;
                renderer.DrawTexture($"{heroId}_icon",
                    new RectangleF(currentLoc.X, currentLoc.Y, _iconSize.X, _iconSize.Y));
                DrawHorizontalsDamage(renderer, currentLoc, _iconSize, enemy, ViewManager.EnabledList);
            }

            renderer.DrawRectangle(
                new RectangleF(PanelPosition.X, PanelPosition.Y, width, count * _iconSize.Y),
                Color.Black);
        }

        public void DrawHorizontalsIcons(IRenderer renderer, Vector2 startPos, Vector2 size, params AbilityId[] ids)
        {
            var pos = startPos + new Vector2(size.X * 1.5f, 0);
            for (var i = 1; i <= ids.Length; i++)
            {
                renderer.DrawTexture($"{ids[i - 1]}_icon",
                    new RectangleF(pos.X, startPos.Y, _iconSize.X, _iconSize.Y));
                pos += new Vector2(size.X * 2, 0);
            }
        }

        public void DrawHorizontalsDamage(IRenderer renderer, Vector2 startPos, Vector2 size, Hero hero,
            params AbilityId[] ids)
        {
            var pos = startPos + new Vector2(size.X, 0);
            var id = hero.HeroId;
            var fontSize = 14f;
            for (var i = 1; i <= ids.Length; i++)
            {
                var current = 0;
                var max = 0;
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (ids[i - 1])
                {
                    case AbilityId.techies_land_mines:
                        current = Main.Updater.BombDamageManager.DamageDictionary[id].LandMinesCount;
                        max = Main.Updater.BombDamageManager.DamageDictionary[id].LandMinesCountMax;
                        break;

                    case AbilityId.techies_suicide:
                        current = 1;
                        max = 1;
                        break;

                    case AbilityId.techies_remote_mines:
                        current = Main.Updater.BombDamageManager.DamageDictionary[id].RemoteMinesCount;
                        max = Main.Updater.BombDamageManager.DamageDictionary[id].RemoteMinesCountMax;
                        break;
                }

                string text;
                switch (ViewManager.ShowDamageType.Value.SelectedIndex)
                {
                    case 0:
                        text = current.ToString();
                        break;
                    case 1:
                        text = max.ToString();
                        break;
                    default:
                        text = $"{current}/{max}";
                        break;
                }

                if (ids[i - 1] == AbilityId.techies_suicide)
                {
                    text = Main.Updater.BombDamageManager.DamageDictionary[id].HeroWillDieSuicide ? "Yes" : "Nope";
                }

                //font size / 2 / 2
                renderer.DrawText(new RectangleF(pos.X, startPos.Y + (fontSize / 4), _iconSize.X * 2f, _iconSize.Y),
                    text,
                    Color.White, RendererFontFlags.Center, fontSize);

                pos += new Vector2(size.X * 2, 0);
            }
        }
    }
}