using System;
using System.Windows.Input;

using Ensage.Common.Menu;
using Ensage.SDK.Helpers;
using Ensage.SDK.Input;
using Ensage.SDK.Menu;
using Ensage.SDK.Renderer;
using InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker;
using SharpDX;
using Color = System.Drawing.Color;

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
            var main = _config.Factory.Menu("Ability panel");
            Enable = main.Item("Enable", true);
            InvokeOnClick = main.Item("Invoke on click", true);
            Clickable = main.Item("Clickable", true);
            Movable = main.Item("Movable", true);
            PosX = main.Item("Pos X", new Slider(500, 0, 2500));
            PosY = main.Item("Pos Y", new Slider(500, 0, 2500));
            Size = main.Item("Size", new Slider(100, 0, 200));
            DrawingStartPosition = new Vector2(PosX, PosY);

            _iconSize = 50f / 100f * Size;

            Size.PropertyChanged += (sender, args) => { _iconSize = 50f / 100f * Size; };

            if (Enable) Activate();

            UpdateManager.BeginInvoke(() => { MaxIcons = config.Main.AbilitiesInCombo.AllAbilities.Count; }, 500);


            Enable.PropertyChanged += (sender, args) =>
            {
                if (Enable)
                    Activate();
                else
                    Deactivate();
            };

            Movable.PropertyChanged += (sender, args) =>
            {
                if (Movable)
                {
                    if (!_movable)
                    {
                        InputManager.MouseClick += InputOnMouseClick;
                        InputManager.MouseMove += InputOnMouseMove;
                        _movable = true;
                    }
                }
                else
                {
                    if (_movable)
                    {
                        InputManager.MouseClick -= InputOnMouseClick;
                        InputManager.MouseMove -= InputOnMouseMove;
                        _movable = false;
                    }
                }
            };

            Clickable.PropertyChanged += (sender, args) =>
            {
                if (Clickable)
                {
                    if (!_clickable)
                    {
                        InputManager.MouseClick += InvokeOnClickAction;
                        _clickable = true;
                    }
                }
                else
                {
                    if (_clickable)
                    {
                        InputManager.MouseClick -= InvokeOnClickAction;
                        _clickable = false;
                    }
                }
            };

            InvokeOnClick.PropertyChanged += (sender, args) =>
            {
                if (InvokeOnClick)
                {
                    if (!_invokeOnClick)
                    {
                        InputManager.MouseClick += ClickableOnClick;
                        _invokeOnClick = true;
                    }
                }
                else
                {
                    if (_invokeOnClick)
                    {
                        InputManager.MouseClick -= ClickableOnClick;
                        _invokeOnClick = false;
                    }
                }
            };
        }

        private IRenderManager Renderer => _config.Main.Context.RenderManager;
        private IInputManager InputManager => _config.Main.Context.Input;

        public MenuItem<bool> InvokeOnClick { get; set; }

        public Vector2 DrawingStartPosition { get; set; }

        public MenuItem<bool> Movable { get; set; }

        public int MaxIcons { get; set; }

        public MenuItem<Slider> PosX { get; set; }
        public MenuItem<Slider> PosY { get; set; }
        public MenuItem<Slider> Size { get; set; }

        public MenuItem<bool> Enable { get; set; }
        public MenuItem<bool> Clickable { get; }

        private void ClickableOnClick(object sender, MouseEventArgs e)
        {
            if ((e.Buttons & MouseButtons.LeftUp) == MouseButtons.LeftUp)
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
                            Console.WriteLine($"IsIn for {ability}");
                            var invoAbility = ability as IInvokableAbility;
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
            Renderer.Draw += RendererOnDraw;

            if (Movable)
            {
                InputManager.MouseClick += InputOnMouseClick;
                InputManager.MouseMove += InputOnMouseMove;
                _movable = true;
            }

            if (Clickable)
            {
                InputManager.MouseClick += ClickableOnClick;
                _clickable = true;
            }

            if (InvokeOnClick)
            {
                InputManager.MouseClick += InvokeOnClickAction;
                _invokeOnClick = true;
            }
        }

        private void InvokeOnClickAction(object sender, MouseEventArgs e)
        {
            var rect = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons, _iconSize);
            var pos = e.Position;
            var isIn = rect.Contains(pos);
            if (isIn && (e.Buttons & MouseButtons.LeftUp) == MouseButtons.LeftUp)
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
            Renderer.Draw -= RendererOnDraw;

            if (_movable)
            {
                InputManager.MouseClick -= InputOnMouseClick;
                InputManager.MouseMove -= InputOnMouseMove;
                _movable = false;
            }
        }

        private void InputOnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMoving)
            {
                var newValue = new Vector2(e.Position.X - _drawMousePosition.X,
                    e.Position.Y - _drawMousePosition.Y);
                newValue.X = Math.Max(PosX.Value.MinValue, Math.Min(PosX.Value.MaxValue, newValue.X));
                newValue.Y = Math.Max(PosY.Value.MinValue, Math.Min(PosY.Value.MaxValue, newValue.Y));
                DrawingStartPosition = newValue;
            }
        }

        private void InputOnMouseClick(object sender, MouseEventArgs e)
        {
            var size = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons, _iconSize);

            var isIn = size.Contains(e.Position);
            if (_isMoving && (e.Buttons & MouseButtons.LeftUp) == MouseButtons.LeftUp)
            {
                PosX.Item.SetValue(new Slider((int) DrawingStartPosition.X, 0, 2500));
                PosY.Item.SetValue(new Slider((int) DrawingStartPosition.Y, 0, 2500));
                _isMoving = false;
            }
            else if (isIn && (e.Buttons & MouseButtons.LeftDown) == MouseButtons.LeftDown)
            {
                var startPos = new Vector2(PosX, PosY);
                _drawMousePosition = e.Position - startPos;
                _isMoving = true;
            }
        }

        private void RendererOnDraw(IRenderer renderer)
        {
            if (MaxIcons == 0)
                return;
            var rect = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons, _iconSize);
            //var rect = new RectangleF(PosX, PosY, iconSize * MaxIcons, iconSize);
            renderer.DrawRectangle(rect, Color.Chartreuse);
            rect.Width = _iconSize;
            foreach (var ability in _config.Main.AbilitiesInCombo.AllAbilities)
            {
                renderer.DrawTexture(ability.Ability.Id.ToString(), rect);
                switch (ability.Ability.AbilityState)
                {
                    case AbilityState.Ready:
                        var key = ((IHaveFastInvokeKey) ability).Key;
                        if (key != Key.None)
                        {
                            renderer.DrawFilledRectangle(rect,
                                Color.FromArgb(55, 0, 0, 0), Color.FromArgb(125, 0, 0, 0));
                            renderer.DrawText(rect, $"{key}", Color.White,
                                RendererFontFlags.Center, _iconSize * 0.75f);
                        }

                        break;
                    case AbilityState.NotEnoughMana:
                        renderer.DrawFilledRectangle(rect,
                            Color.FromArgb(150, 0, 90, 255), Color.FromArgb(100, 0, 0, 0));
                        break;
                    case AbilityState.OnCooldown:
                        renderer.DrawFilledRectangle(rect, Color.FromArgb(200, 0, 0, 0), Color.FromArgb(100, 0, 0, 0));
                        renderer.DrawText(rect, ((int) ability.Ability.Cooldown).ToString(), Color.White,
                            RendererFontFlags.Center, _iconSize * 0.75f);
                        break;
                }

                rect.X += _iconSize;
            }
        }
    }
}