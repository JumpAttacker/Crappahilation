using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;
using Ensage.SDK.Renderer;
using SharpDX;
using Color = System.Drawing.Color;

namespace TechiesCrappahilationPaid.Features
{
    public class SuicideDamage
    {
        private readonly TechiesCrappahilationPaid _main;

        private Hero Me => _main.Me;
        public readonly Dictionary<Hero, Vector2> PlayerPositions;
        private IRenderManager Renderer => _main.Context.RenderManager;

        public SuicideDamage(TechiesCrappahilationPaid main)
        {
            _main = main;
            var suicide = main.MenuManager.VisualSubMenu.Menu("Suicide");
            ShowSuicideDamage = suicide.Item("Show suicide damage on heroes", true);
            SuicideType = suicide.Item("Suicide Draw type", new StringList("text", "icon"));
            var settings = suicide.Menu("Settings");
            PositionX = settings.Item("extra position x", new Slider(0, -100, 100));
            PositionY = settings.Item("extra position y", new Slider(0, -100, 100));
            TextSize = settings.Item("text size", new Slider(13, 5, 25));

            PlayerPositions = new Dictionary<Hero, Vector2>();

            Drawing.OnDraw += args =>
            {
                foreach (var enemy in TargetManager.Targets)
                {
                    if (enemy != null && enemy.IsValid)
                        if (PlayerPositions.ContainsKey(enemy))
                        {
                            PlayerPositions[enemy] = HUDInfo.GetHPbarPosition(enemy);
                        }
                        else
                        {
                            PlayerPositions.Add(enemy, HUDInfo.GetHPbarPosition(enemy));
                        }
                }
            };

            Renderer.Draw += (renderer) =>
            {
                if (!ShowSuicideDamage.Value) return;
                foreach (var g in PlayerPositions.ToList())
                {
                    var enemy = g.Key;
                    if (!_main.Updater.BombDamageManager.DamageDictionary.ContainsKey(enemy.HeroId) ||
                        !enemy.IsVisible || !enemy.IsAlive)
                        continue;
                    var pos = g.Value;
//                    var w2s = Drawing.WorldToScreen(pos);
                    if (pos.IsZero)
                        continue;
                    var drawPos = pos;
                    if (SuicideType.Value.SelectedIndex == 0)
                    {
                        var damage = Math
                            .Round(_main.Updater.BombDamageManager.DamageDictionary[enemy.HeroId].HealthAfterSuicide, 1)
                            .ToString(CultureInfo.InvariantCulture);
                        var size = renderer.MeasureText(damage, TextSize);
                        drawPos.X -= size.X + 5;
                        renderer.DrawFilledRectangle(
                            new RectangleF(drawPos.X - 1 + PositionX, drawPos.Y + PositionY, size.X, size.Y),
                            Color.Black,
                            Color.FromArgb(155, Color.Black));
                        renderer.DrawText(drawPos + new Vector2(PositionX, PositionY), damage, Color.White, TextSize);
                    }
                    else
                    {
                        var willDie = _main.Updater.BombDamageManager.DamageDictionary[enemy.HeroId].HeroWillDieSuicide;
                        var rect = new RectangleF(drawPos.X - 26 + PositionX, drawPos.Y + PositionY, TextSize * 2 - 2,
                            TextSize * 2 - 2);
                        renderer.DrawTexture($"{AbilityId.techies_suicide}_icon", rect);
                        renderer.DrawFilledRectangle(rect,
                            Color.FromArgb(35, willDie ? Color.LimeGreen : Color.Red),
                            willDie ? Color.LimeGreen : Color.Red, 0.5f);
                    }
                }
            };
        }

        public MenuItem<Slider> TextSize { get; set; }

        public MenuItem<Slider> PositionY { get; set; }

        public MenuItem<Slider> PositionX { get; set; }

        public MenuItem<StringList> SuicideType { get; set; }

        public MenuItem<bool> ShowSuicideDamage { get; set; }
    }
}