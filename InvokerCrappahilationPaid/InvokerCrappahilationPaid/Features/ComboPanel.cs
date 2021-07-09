using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
<<<<<<< HEAD

=======
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893
using SharpDX;

using Color = System.Drawing.Color;

namespace InvokerCrappahilationPaid.Features
{
    public class ComboPanel
    {
        private readonly Config _config;
        private readonly MenuFactory _main;
        private Vector2 _drawMousePosition;
        private float _iconSize;
        private bool _isMoving;


        private bool _movable;

        public List<MyLittleCombo> Combos;

        public ComboPanel(Config config)
        {
            _config = config;
            _main = _config.Factory.Menu("Combo Panel");
            Enable = _main.Item("Enable", true);
            Movable = _main.Item("Movable", true);
            PosX = _main.Item("Pos X", new Slider(500, 0, 2500));
            PosY = _main.Item("Pos Y", new Slider(500, 0, 2500));
            Size = _main.Item("Size", new Slider(100, 0, 200));
            DrawingStartPosition = new Vector2(PosX, PosY);

            _iconSize = 50f / 100f * Size;

            Combos = new List<MyLittleCombo>();
            IsAutoComboSelected = true;
            for (var i = 0; i < 5; i++) Combos.Add(new MyLittleCombo(i, this));

            Combos.Add(new MyLittleCombo(-1, this));

            Size.PropertyChanged += (sender, args) => { _iconSize = 50f / 100f * Size; };

            if (Enable) Activate();
            MaxIcons = 0;

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
        }

        private IRenderManager Renderer => _config.Main.Context.RenderManager;
        private IInputManager InputManager => _config.Main.Context.Input;

        public Vector2 DrawingStartPosition { get; set; }

        public MyLittleCombo SelectedCombo { get; set; }

        public bool IsAutoComboSelected { get; set; }

        public MenuItem<bool> Movable { get; set; }

        public int MaxIcons { get; set; }

        public MenuItem<Slider> PosX { get; set; }
        public MenuItem<Slider> PosY { get; set; }
        public MenuItem<Slider> Size { get; set; }

        public MenuItem<bool> Enable { get; set; }

        private void Activate()
        {
            Renderer.Draw += RendererOnDraw;
            InputManager.MouseClick += OnComboClickSelecteor;
            if (Movable)
            {
                InputManager.MouseClick += InputOnMouseClick;
                InputManager.MouseMove += InputOnMouseMove;
                _movable = true;
            }
        }

        private void Deactivate()
        {
            Renderer.Draw -= RendererOnDraw;
            InputManager.MouseClick -= OnComboClickSelecteor;
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
            var mousePos = e.Position;
            var size = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons,
                _iconSize);

            var isIn = size.Contains(mousePos);
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

        private void OnComboClickSelecteor(object sender, MouseEventArgs e)
        {
            if ((e.Buttons & MouseButtons.LeftUp) != MouseButtons.LeftUp)
                return;
            var mousePos = e.Position;
            var fullRectangleF = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons,
                _iconSize * (1 + Combos.Count(x => x.Enable || x.Id == -1)));
            if (fullRectangleF.Contains(mousePos))
                foreach (var combo in Combos.Where(x => x.Enable || x.Id == -1))
                    if (combo.Rect.Contains(mousePos))
                    {
                        //InvokerCrappahilationPaid.Log.Warn($"Click on Panel #{combo.Id}");
                        Combos.ForEach(littleCombo => littleCombo.IsSelected = false);
                        combo.IsSelected = true;
                        IsAutoComboSelected = combo.Id == -1;
                        SelectedCombo = combo;
                        combo.AbilityInAction = 0;
                        break;
                    }
        }

        private void RendererOnDraw(IRenderer renderer)
        {
            if (MaxIcons == 0)
                return;
            var rect = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons,
                _iconSize - 2);
            renderer.DrawFilledRectangle(rect, Color.FromArgb(10,127,255,0), Color.FromArgb(210, 0, 0, 0), 1f);
            rect.Height = (rect.Height + 2) * (1 + Combos.Count(x => x.Enable || x.Id == -1));
            renderer.DrawFilledRectangle(rect, Color.FromArgb(10,127,255,0), Color.FromArgb(155, 0, 0, 0), 1f);

            rect.Height = _iconSize;
            renderer.DrawText(rect, "Combo Panel", Color.White, RendererFontFlags.Center, _iconSize * 0.75f);
            rect.Width = _iconSize;
            rect.Y += _iconSize;

            foreach (var combo in Combos.Where(x => x.Enable || x.Id == -1))
            {
                var startRect = rect;
                if (combo.Id != -1)
                {
                    var index = 0;
                    foreach (var item in combo.Items)
                    {
                        renderer.DrawRectangle(startRect, Color.DodgerBlue);
                        renderer.DrawTexture(item, startRect);
                        if (combo.IsSelected && index == combo.AbilityInAction)
                            renderer.DrawFilledRectangle(startRect, Color.FromArgb(25,255,255,0),
                                Color.FromArgb(75, 173, 255, 47));
                        index++;
                        startRect.X += _iconSize;
                    }
                }
                else
                {
                    startRect = rect;
                    startRect.Width = _iconSize * MaxIcons;
                    renderer.DrawText(startRect, "Dynamic Combo", Color.White, RendererFontFlags.Left,
                        _iconSize * 0.75f);
                }

                startRect = rect;
                startRect.X += 1;
                startRect.Y += 1;
                startRect.Width = _iconSize * MaxIcons;
                startRect.Height -= 2;
                startRect.Width -= 2;
                combo.Rect = startRect;
                if (combo.IsSelected)
                {
                    renderer.DrawRectangle(startRect, Color.Fuchsia, 2f);
                }
                else
                {
                    var clr = Color.FromArgb(50, 50, 50, 50);
                    renderer.DrawFilledRectangle(startRect, clr, clr);
                }

                rect.Y += _iconSize;
            }
        }

        public class MyLittleCombo
        {
            private readonly ComboPanel _comboPanel;
            public MenuItem<bool> Enable;
            public bool IsSelected;
            public List<string> Items;
            public string Text;

            public MyLittleCombo(int id, ComboPanel comboPanel)
            {
                _comboPanel = comboPanel;
                Id = id;
                MenuFactory main;
                if (id == -1)
                {
                    main = comboPanel._main.Menu("Dynamic combo");
                    Key = main.Item("Key", new KeyBind('0'));
                    Key.PropertyChanged += (sender, args) =>
                    {
                        if (Key)
                        {
                            /*foreach (var combo in _comboPanel.Combos.Where(x => x.Enable || x.Id == -1))
                            {
                                combo.IsSelected = combo == this;
                            }*/
                            _comboPanel.Combos.ForEach(littleCombo => littleCombo.IsSelected = false);
                            IsSelected = true;
                            comboPanel.IsAutoComboSelected = true;
                            comboPanel.SelectedCombo = this;
                            AbilityInAction = 0;
                        }
                    };
                    IsSelected = true;
                    return;
                }

                IsSelected = false;
                main = comboPanel._main.Menu($"Combo #{id}");
                Enable = main.Item("Enable", Id == 0);
                Key = main.Item("Key", new KeyBind('0'));
                Enable.PropertyChanged += OnUpdate;
                Key.PropertyChanged += (sender, args) =>
                {
                    if (Key && Enable)
                    {
                        /*foreach (var combo in _comboPanel.Combos.Where(x => x.Enable || x.Id == -1))
                        {
                            combo.IsSelected = combo == this;
                        }*/
                        _comboPanel.Combos.ForEach(littleCombo => littleCombo.IsSelected = false);
                        IsSelected = true;
                        comboPanel.IsAutoComboSelected = false;
                        comboPanel.SelectedCombo = this;
                        AbilityInAction = 0;
                    }
                };
                var list = new List<string>
                {
                    AbilityId.item_refresher.ToString(),
                    AbilityId.item_cyclone.ToString(),
//                    AbilityId.invoker_ghost_walk.ToString()
                };
                UpdateManager.BeginInvoke(() =>
                {
                    list.AddRange(
                        comboPanel._config.Main.AbilitiesInCombo.AllAbilities.Select(ability =>
                            ability.Ability.Id.ToString()));

                    var dict = list.ToDictionary(x => x, x => true);
                    Abilities = main.Item("Abilities:", new AbilityToggler(dict));
                    AbilitiesPriority = main.Item("Priority:", new PriorityChanger(list));
                    NextAbilityAfterRefresher = main.Item("Ability index after refresher", new Slider(2, 0, 10));
                    Abilities.PropertyChanged += OnUpdate;
                    AbilitiesPriority.PropertyChanged += OnUpdate;
                    if (Enable)
                        UpdateItems(true);
                }, 500);
            }

            public int Id { get; }

            public MenuItem<KeyBind> Key { get; set; }

            public MenuItem<Slider> NextAbilityAfterRefresher { get; set; }

            public MenuItem<PriorityChanger> AbilitiesPriority { get; set; }

            public MenuItem<AbilityToggler> Abilities { get; set; }
            public RectangleF Rect { get; set; }
            public int AbilityInAction { get; set; }


            private void OnUpdate(object o, PropertyChangedEventArgs args)
            {
                var isBoolChanged = o.ToString() == "Ensage.SDK.Menu.MenuItem`1[System.Boolean]";
                var firstTime = isBoolChanged && !Enable;
                //InvokerCrappahilationPaid.Log.Warn($"{o} {o.ToString() == "Ensage.SDK.Menu.MenuItem`1[System.Boolean]"}");
                UpdateItems(firstTime);
            }


            public void UpdateItems(bool isFirstTime = false)
            {
                Items = new List<string>();
                Items.AddRange(Abilities.Value.Dictionary.Select(z => z.Key).Where(x => Abilities.Value.IsEnabled(x)));
                Items = new List<string>(Items.OrderByDescending(x => AbilitiesPriority.Value.GetPriority(x)));
                UpdateManager.BeginInvoke(() =>
                {
                    if (Enable || isFirstTime)
                    {
                        //var count = 0;
                        _comboPanel.MaxIcons = 0;
                        foreach (var combo in _comboPanel.Combos.Where(x => Items != null && x.Enable && x.Id >= 0))
                        {
                            var localCount = new List<string>();
                            localCount.AddRange(combo.Abilities.Value.Dictionary.Select(z => z.Key)
                                .Where(x => combo.Abilities.Value.IsEnabled(x)));
                            //_comboPanel.MaxIcons = Math.Max(Enable ? Items.Count : 0, localCount);
                            _comboPanel.MaxIcons = Math.Max(_comboPanel.MaxIcons, localCount.Count);
                            //InvokerCrappahilationPaid.Log.Warn($"[{count++}] Max: {_comboPanel.MaxIcons}");
                        }

                        //InvokerCrappahilationPaid.Log.Warn($"TotalMax for combo# {Id}: {_comboPanel.MaxIcons}");
                    }
                }, 500);
                /*if (_comboPanel.MaxIcons < Items.Count)
                    _comboPanel.MaxIcons = Items.Count;*/
            }
        }
    }
}