using System;
using System.Windows.Input;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Input;
using Divine.Input.EventArgs;
using Divine.Menu.Items;
using Divine.Numerics;
using Divine.Renderer;
using Divine.Update;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;
using InputManager = Divine.Input.InputManager;
using MouseEventArgs = Divine.Input.EventArgs.MouseEventArgs;

namespace InvokerCrappahilationPaid.Features
{
    public class AbilityPanel
    {
        private readonly Config _config;
        private bool _clickable;
        private Vector2 _drawMousePosition;
        private float _iconSize;
        private bool _invokeOnClick;
        private bool _isMoving;

        private bool _movable;

        public AbilityPanel(Config config)
        {
            _config = config;
            var main = _config.Factory.CreateMenu("Ability panel");
            Enable = main.CreateSwitcher("Enable", true);
            InvokeOnClick = main.CreateSwitcher("Invoke on click", true);
            Clickable = main.CreateSwitcher("Clickable", true);
            Movable = main.CreateSwitcher("Movable", true);
            PosX = main.CreateSlider("Pos X", 500, 0, 2500);
            PosY = main.CreateSlider("Pos Y", 500, 0, 2500);
            Size = main.CreateSlider("Size", 100, 0, 200);
            DrawingStartPosition = new Vector2(PosX, PosY);

            // PosX.ValueChanged += (slider, args) =>
            // {
            //     DrawingStartPosition = new Vector2(args.NewValue, PosY);
            // };
            // PosY.ValueChanged += (slider, args) =>
            // {
            //     DrawingStartPosition = new Vector2(PosX, args.NewValue);
            // };

            _iconSize = 50f / 100f * Size;

            Size.ValueChanged += (sender, args) => { _iconSize = 50f / 100f * Size; };

            // if (Enable) Activate();

            UpdateManager.BeginInvoke(500, () => { MaxIcons = config.Main.AbilitiesInCombo.AllAbilities.Count; });


            Enable.ValueChanged += (sender, args) =>
            {
                if (args.Value)
                    Activate();
                else
                    Deactivate();
            };

            Movable.ValueChanged += (sender, args) =>
            {
                if (args.Value)
                {
                    if (!_movable)
                    {
                        InputManager.MouseKeyDown += InputOnMouseClick;
                        InputManager.MouseMove += InputOnMouseMove;
                        _movable = true;
                    }
                }
                else
                {
                    if (_movable)
                    {
                        InputManager.MouseKeyDown -= InputOnMouseClick;
                        InputManager.MouseMove -= InputOnMouseMove;
                        _movable = false;
                    }
                }
            };

            Clickable.ValueChanged += (sender, args) =>
            {
                if (args.Value)
                {
                    if (!_clickable)
                    {
                        InputManager.MouseKeyDown += InvokeOnClickAction;
                        _clickable = true;
                    }
                }
                else
                {
                    if (_clickable)
                    {
                        InputManager.MouseKeyDown -= InvokeOnClickAction;
                        _clickable = false;
                    }
                }
            };

            InvokeOnClick.ValueChanged += (sender, args) =>
            {
                if (args.Value)
                {
                    if (!_invokeOnClick)
                    {
                        InputManager.MouseKeyDown += ClickableOnClick;
                        _invokeOnClick = true;
                    }
                }
                else
                {
                    if (_invokeOnClick)
                    {
                        InputManager.MouseKeyDown -= ClickableOnClick;
                        _invokeOnClick = false;
                    }
                }
            };
        }

        public MenuSlider Size { get; set; }

        public MenuSlider PosY { get; set; }

        public MenuSlider PosX { get; set; }

        public MenuSwitcher Movable { get; set; }

        public MenuSwitcher Clickable { get; set; }

        public MenuSwitcher InvokeOnClick { get; set; }

        public MenuSwitcher Enable { get; set; }


        public Vector2 DrawingStartPosition { get; set; }


        public int MaxIcons { get; set; }


        private void ClickableOnClick(MouseEventArgs e)
        {
            if (e.MouseKey == MouseKey.Left)
            {
                var size = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y,
                    _iconSize * MaxIcons, _iconSize);
                var isIn = size.Contains(e.Position);
                if (isIn)
                {
                    var count = 0;
                    foreach (var ability in _config.Main.AbilitiesInCombo.AllAbilities)
                    {
                        size = new RectangleF(DrawingStartPosition.X + count * _iconSize, DrawingStartPosition.Y,
                            _iconSize, _iconSize);
                        isIn = size.Contains(e.Position);
                        if (isIn)
                        {
                            var invoAbility = ability as IInvokableAbility;
                            Console.WriteLine($"IsIn for {ability} -> {invoAbility}");
                            invoAbility?.Invoke();
                            break;
                        }

                        count++;
                    }
                }
            }
        }

        private void Activate()
        {
            RendererManager.Draw += RendererOnDraw;

            if (Movable)
            {
                InputManager.MouseKeyDown += InputOnMouseClick;
                InputManager.MouseMove += InputOnMouseMove;
                _movable = true;
            }

            if (Clickable)
            {
                InputManager.MouseKeyDown += ClickableOnClick;
                _clickable = true;
            }

            if (InvokeOnClick)
            {
                InputManager.MouseKeyDown += InvokeOnClickAction;
                _invokeOnClick = true;
            }
        }

        private void InvokeOnClickAction(MouseEventArgs e)
        {
            var rect = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons, _iconSize);
            var pos = e.Position;
            var isIn = rect.Contains(pos);
            if (isIn && e.MouseKey == MouseKey.Left)
            {
                rect.Width = _iconSize;
                var allAbilities = _config.Main.AbilitiesInCombo.AllAbilities;

                foreach (var ability in allAbilities)
                {
                    isIn = rect.Contains(pos);
                    if (isIn)
                        if (ability is IInvokableAbility invoke && invoke.Invoke(skip: true))
                            return;

                    rect.X += _iconSize;
                }
            }
        }

        private void Deactivate()
        {
            RendererManager.Draw -= RendererOnDraw;

            if (_movable)
            {
                InputManager.MouseKeyDown -= InputOnMouseClick;
                InputManager.MouseMove -= InputOnMouseMove;
                _movable = false;
            }
        }

        private void InputOnMouseMove(MouseMoveEventArgs e)
        {
            if (_isMoving)
            {
                var newValue = new Vector2(e.Position.X - _drawMousePosition.X,
                    e.Position.Y - _drawMousePosition.Y);
                newValue.X = Math.Max(PosX.MinValue, Math.Min(PosX.MaxValue, newValue.X));
                newValue.Y = Math.Max(PosY.MinValue, Math.Min(PosY.MaxValue, newValue.Y));
                DrawingStartPosition = newValue;
            }
        }

        private void InputOnMouseClick(MouseEventArgs e)
        {
            var size = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons, _iconSize);

            var isIn = size.Contains(e.Position);
            if (_isMoving && e.MouseKey == MouseKey.Left)
            {
                PosX.Value = (int) DrawingStartPosition.X;
                PosY.Value = (int) DrawingStartPosition.Y;
                _isMoving = false;
            }
            else if (isIn && e.MouseKey == MouseKey.Left)
            {
                var startPos = new Vector2(PosX, PosY);
                _drawMousePosition = e.Position - startPos;
                _isMoving = true;
            }
        }

        private void RendererOnDraw()
        {
            if (MaxIcons == 0)
                return;
            var rect = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons, _iconSize);
            //var rect = new RectangleF(PosX, PosY, iconSize * MaxIcons, iconSize);
            RendererManager.DrawRectangle(rect, Color.Chartreuse);
            rect.Width = _iconSize;
            foreach (var ability in _config.Main.AbilitiesInCombo.AllAbilities)
            {
                RendererManager.DrawImage(ability.Id, rect, AbilityImageType.Default, true);
                switch (ability.AbilityState)
                {
                    case AbilityState.Ready:
                        var key = ((IHaveFastInvokeKey) ability).Key;
                        if (key != Key.None)
                        {
                            RendererManager.DrawFilledRectangle(rect,
                                new Color(0, 0, 0, 125),
                                new Color(0, 0, 0, 55), 1);
                            RendererManager.DrawText($"{key}", rect, Color.White,
                                FontFlags.Center, _iconSize * 0.75f);
                        }

                        break;
                    case AbilityState.NotEnoughMana:
                        RendererManager.DrawFilledRectangle(rect, new Color(0, 0, 0, 100),
                            new Color(255, 0, 90, 150), 1);
                        break;
                    case AbilityState.OnCooldown:
                        RendererManager.DrawFilledRectangle(rect, new Color(1, 0, 0, 100), new Color(0, 0, 0, 200), 1);
                        RendererManager.DrawText(((int) ability.BaseAbility.RemainingCooldown).ToString(), rect, Color.White,
                            FontFlags.Center, _iconSize * 0.75f);
                        break;
                }

                rect.X += _iconSize;
            }
        }
    }
}