using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Menu.Items;
using Divine.Numerics;
using Divine.Renderer;

using TechiesCrappahilationPaid.Helpers;

namespace TechiesCrappahilationPaid.Features
{
    public class SuicideDamage
    {
        private readonly TechiesCrappahilationPaid _main;

        private Hero Me => _main.Me;
        public readonly Dictionary<Hero, Vector2> PlayerPositions;

        public SuicideDamage(TechiesCrappahilationPaid main)
        {
            _main = main;
            var suicide = main.MenuManager.VisualSubMenu.CreateMenu("Suicide");
            ShowSuicideDamage = suicide.CreateSwitcher("Show suicide damage on heroes", true);
            SuicideType = suicide.CreateSwitcher("Suicide Draw type" /*, new StringList("text", "icon")*/);
            var settings = suicide.CreateMenu("Settings");
            PositionX = settings.CreateSlider("extra position x", 0, -100, 100);
            PositionY = settings.CreateSlider("extra position y", 0, -100, 100);
            TextSize = settings.CreateSlider("text size", 13, 5, 25);

            PlayerPositions = new Dictionary<Hero, Vector2>();

            RendererManager.Draw += () =>
            {
                foreach (var enemy in TargetManager.Targets)
                {
                    if (enemy != null && enemy.IsValid)
                        if (PlayerPositions.ContainsKey(enemy))
                        {
                            PlayerPositions[enemy] = CustomHUDInfo.GetHpBarPosition(enemy);
                        }
                        else
                        {
                            PlayerPositions.Add(enemy, CustomHUDInfo.GetHpBarPosition(enemy));
                        }
                }
            };

            RendererManager.Draw += () =>
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
                    // if (SuicideType.Value.SelectedIndex == 0)
                    if (SuicideType.Value)
                    {
                        var damage = Math
                            .Round(_main.Updater.BombDamageManager.DamageDictionary[enemy.HeroId].HealthAfterSuicide, 1)
                            .ToString(CultureInfo.InvariantCulture);
                        var size = RendererManager.MeasureText(damage, TextSize);
                        drawPos.X -= size.X + 5;
                        RendererManager.DrawFilledRectangle(
                            new RectangleF(drawPos.X - 1 + PositionX, drawPos.Y + PositionY, size.X, size.Y),
                            Color.Black,
                            Color.Black, 1);
                        RendererManager.DrawText(damage, drawPos + new Vector2(PositionX, PositionY), Color.White,
                            TextSize);
                    }
                    else
                    {
                        var willDie = _main.Updater.BombDamageManager.DamageDictionary[enemy.HeroId].HeroWillDieSuicide;
                        var rect = new RectangleF(drawPos.X - 26 + PositionX, drawPos.Y + PositionY, TextSize * 2 - 2,
                            TextSize * 2 - 2);
                        RendererManager.DrawImage($"{AbilityId.techies_suicide}_icon", rect);
                        var clr = willDie ? Color.LimeGreen : Color.Red;
                        RendererManager.DrawFilledRectangle(rect,
                            new Color((float) clr.R, clr.G, clr.B, 35),
                            willDie ? Color.LimeGreen : Color.Red, 0.5f);
                    }
                }
            };
        }

        public MenuSlider PositionY { get; set; }

        public MenuSlider PositionX { get; set; }

        public MenuSwitcher SuicideType { get; set; }

        public MenuSwitcher ShowSuicideDamage { get; set; }

        public int TextSize { get; set; }
    }
}