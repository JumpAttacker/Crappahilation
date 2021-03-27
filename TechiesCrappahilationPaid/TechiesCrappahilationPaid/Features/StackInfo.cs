using System.Linq;
using System.Threading.Tasks;
using Divine;
using SharpDX;
using TechiesCrappahilationPaid.Helpers;

namespace TechiesCrappahilationPaid.Features
{
    public class StackInfo
    {
        private readonly TechiesCrappahilationPaid _main;


        public StackInfo(TechiesCrappahilationPaid main)
        {
            _main = main;
            // inputManager.MouseClick += MouseClick;
            InputManager.MouseKeyUp += MouseClick;
            if (main.MenuManager.DrawStacks)
                RendererManager.Draw += RenderManagerOnDraw;

            main.MenuManager.DrawStacks.ValueChanged += (sender, args) =>
            {
                if (main.MenuManager.DrawStacks)
                {
                    RendererManager.Draw += RenderManagerOnDraw;
                }
                else
                {
                    RendererManager.Draw -= RenderManagerOnDraw;
                }
            };
            ModifierManager.ModifierAdded += (sender) =>
            {
                var bomb = main.Updater.BombManager.RemoteMines.FirstOrDefault(x => x.Owner == sender.Modifier.Owner);
                if (bomb != null)
                {
                    if (sender.Modifier.Name == "modifier_truesight")
                    {
                        bomb.UnderTrueSight = true;
                    }
                }
            };

            ModifierManager.ModifierRemoved += (sender) =>
            {
                var bomb = main.Updater.BombManager.RemoteMines.FirstOrDefault(x => x.Owner == sender.Modifier.Owner);
                if (bomb != null)
                {
                    if (sender.Modifier.Name == "modifier_truesight")
                    {
                        bomb.UnderTrueSight = false;
                    }
                }
            };
        }

        private bool IsButtonClicked { get; set; }

        private void MouseClick(MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.MouseKey == MouseKey.Left)
            {
                mouseEventArgs.Process = false;
                UpdateManager.BeginInvoke(async () =>
                {
                    IsButtonClicked = true;
                    await Task.Delay(50);
                    IsButtonClicked = false;
                });
            }
        }

        private Vector2 GetMousePosition => GameManager.MouseScreenPosition;

        private bool IsIn(RectangleF rect, Vector2 vector2) => rect.Contains((int) vector2.X, (int) vector2.Y);

        private void RenderManagerOnDraw()
        {
            var mousePos = GetMousePosition;
            foreach (var bomb in _main.Updater.BombManager.RemoteMines.Where(x =>
                x.Owner.IsValid && x.Owner.IsAlive && x.Stacker.IsActive))
            {
                if (_main.MenuManager.StackDontDrawSolo && bomb.Stacker.Counter == 1)
                    continue;
                var topPos = CustomHUDInfo.GetHpBarPosition(bomb.Owner);
                if (topPos.IsZero)
                    continue;
                var size = new Vector2(CustomHUDInfo.HpBarSizeX, CustomHUDInfo.HpBarSizeY);
                var text = bomb.Stacker.Counter.ToString();
                var textSize = RendererManager.MeasureText(text, 30);
                var textPos = topPos + new Vector2(size.X / 2 - textSize.X / 2, -size.Y * 2);
                var extraRectangleSizeX = 50;
                var extraRectangleSizeY = textSize.Y / 2;
                var rectWidth = textSize.X + extraRectangleSizeX * 2;
                var boxSize = new Vector2(rectWidth / 5);
                var rectangle = new RectangleF(textPos.X - extraRectangleSizeX, textPos.Y - extraRectangleSizeY,
                    rectWidth + 2, textSize.Y + boxSize.X);

                if (IsIn(rectangle, mousePos))
                {
                    RendererManager.DrawFilledRectangle(rectangle, new SharpDX.Color(200, 10, 10, 10), Color.White, 1);
                    RendererManager.DrawText(text,new Vector2(rectangle.X + rectangle.Width / 2 - textSize.X / 2, rectangle.Y),
                        
                        bomb.UnderTrueSight ? Color.OrangeRed : Color.White, 30);

                    var count = 0;
                    foreach (var target in TargetManager.Targets.GroupBy(x => x.HeroId).ToList())
                    {
                        var boxPos = new Vector2(rectangle.X + count++ * boxSize.X, rectangle.Y + textSize.Y);
                        var boxRect = new RectangleF(boxPos.X + 1, boxPos.Y - 1, boxSize.X, boxSize.Y);
                        RendererManager.DrawTexture(target.Key, new RectangleF(boxPos.X,boxPos.Y, boxSize.X, boxSize.Y));
                        if (bomb.Stacker.DetonateDict.TryGetValue(target.Key, out var isEnable))
                        {
                            RendererManager.DrawRectangle(boxRect, isEnable ? Color.Green : Color.OrangeRed, 1);
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
                            RendererManager.DrawFilledRectangle(boxRect, new SharpDX.Color(100, 50, 50, 50), Color.White, 0);
                        }
                    }
                }
                else
                    RendererManager.DrawText(text, textPos, bomb.UnderTrueSight ? Color.OrangeRed : Color.White, 30);
            }
        }
    }
}