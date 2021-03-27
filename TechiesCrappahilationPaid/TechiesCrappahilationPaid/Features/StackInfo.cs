using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.SDK.Helpers;
using Ensage.SDK.Input;
using Ensage.SDK.Renderer;
using SharpDX;
using Color = System.Drawing.Color;

namespace TechiesCrappahilationPaid.Features
{
    public class StackInfo
    {
        private readonly TechiesCrappahilationPaid _main;


        public StackInfo(TechiesCrappahilationPaid main)
        {
            _main = main;
            var inputManager = main.Context.Input;
            inputManager.MouseClick += MouseClick;
            if (main.MenuManager.DrawStacks)
                main.Context.RenderManager.Draw += RenderManagerOnDraw;

            main.MenuManager.DrawStacks.PropertyChanged += (sender, args) =>
            {
                if (main.MenuManager.DrawStacks)
                {
                    main.Context.RenderManager.Draw += RenderManagerOnDraw;
                }
                else
                {
                    main.Context.RenderManager.Draw -= RenderManagerOnDraw;
                }
            };

            Unit.OnModifierAdded += (sender, args) =>
            {
                var bomb = main.Updater.BombManager.RemoteMines.FirstOrDefault(x => x.Owner == sender);
                if (bomb != null)
                {
                    if (args.Modifier.Name == "modifier_truesight")
                    {
                        bomb.UnderTrueSight = true;
                    }
                }
            };

            Unit.OnModifierRemoved += (sender, args) =>
            {
                var bomb = main.Updater.BombManager.RemoteMines.FirstOrDefault(x => x.Owner == sender);
                if (bomb != null)
                {
                    if (args.Modifier.Name == "modifier_truesight")
                    {
                        bomb.UnderTrueSight = false;
                    }
                }
            };
        }

        private bool IsButtonClicked { get; set; }

        private void MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.Buttons & MouseButtons.LeftDown) == MouseButtons.LeftDown)
            {
                UpdateManager.BeginInvoke(async () =>
                {
                    IsButtonClicked = true;
                    await Task.Delay(50);
                    IsButtonClicked = false;
                });
            }
        }

        private Vector2 GetMousePosition => Game.MouseScreenPosition;

        private bool IsIn(RectangleF rect, Vector2 vector2) => rect.Contains((int) vector2.X, (int) vector2.Y);

        private void RenderManagerOnDraw(IRenderer renderer)
        {
            var mousePos = GetMousePosition;
            foreach (var bomb in _main.Updater.BombManager.RemoteMines.Where(x =>
                x.Owner.IsValid && x.Owner.IsAlive && x.Stacker.IsActive))
            {
                if (_main.MenuManager.StackDontDrawSolo && bomb.Stacker.Counter == 1)
                    continue;
                var topPos = HUDInfo.GetHPbarPosition(bomb.Owner);
                if (topPos.IsZero)
                    continue;
                var size = new Vector2((float) HUDInfo.GetHPBarSizeX(bomb.Owner),
                    (float) HUDInfo.GetHpBarSizeY(bomb.Owner));
                var text = bomb.Stacker.Counter.ToString();
                var textSize = renderer.MeasureText(text, 30);
                var textPos = topPos + new Vector2(size.X / 2 - textSize.X / 2, -size.Y * 2);
                var extraRectangleSizeX = 50;
                var extraRectangleSizeY = textSize.Y / 2;
                var rectWidth = textSize.X + extraRectangleSizeX * 2;
                var boxSize = new Vector2(rectWidth / 5);
                var rectangle = new RectangleF(textPos.X - extraRectangleSizeX, textPos.Y - extraRectangleSizeY,
                    rectWidth + 2, textSize.Y + boxSize.X);

                if (IsIn(rectangle, mousePos))
                {
                    renderer.DrawFilledRectangle(rectangle, Color.FromArgb(200, 10, 10, 10), Color.White, 1);
                    renderer.DrawText(new Vector2(rectangle.X + rectangle.Width / 2 - textSize.X / 2, rectangle.Y),
                        text,
                        bomb.UnderTrueSight ? Color.OrangeRed : Color.White, 30);

                    var count = 0;
                    foreach (var target in TargetManager.Targets.GroupBy(x => x.HeroId).ToList())
                    {
                        var boxPos = new Vector2(rectangle.X + count++ * boxSize.X, rectangle.Y + textSize.Y);
                        var boxRect = new RectangleF(boxPos.X + 1, boxPos.Y - 1, boxSize.X, boxSize.Y);
                        renderer.DrawTexture(target.Key + "_icon",
                            boxPos, boxSize);
                        if (bomb.Stacker.DetonateDict.TryGetValue(target.Key, out var isEnable))
                        {
                            renderer.DrawRectangle(boxRect, isEnable ? Color.Green : Color.OrangeRed, 1);
                        }
                        else
                        {
                            bomb.Stacker.DetonateDict.Add(target.Key, true);
                        }

                        if (IsIn(boxRect, mousePos))
                        {
                            if (IsButtonClicked)
                            {
                                IsButtonClicked = false;
                                bomb.Stacker.DetonateDict[target.Key] = !bomb.Stacker.DetonateDict[target.Key];
                            }
                        }
                        else
                        {
                            renderer.DrawFilledRectangle(boxRect, Color.FromArgb(100, 50, 50, 50), Color.White, 0);
                        }
                    }
                }
                else
                    renderer.DrawText(textPos, text, bomb.UnderTrueSight ? Color.OrangeRed : Color.White, 30);
            }
        }
    }
}