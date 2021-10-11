using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Input;
using Divine.Input.EventArgs;
using Divine.Menu.EventArgs;
using Divine.Menu.Items;
using Divine.Numerics;
using Divine.Renderer;
using Divine.Update;
using InputManager = Divine.Input.InputManager;
using MouseEventArgs = Divine.Input.EventArgs.MouseEventArgs;


namespace InvokerCrappahilationPaid.Features
{
    public class ComboPanel
    {
        private readonly Config _config;
        private Vector2 _drawMousePosition;
        private float _iconSize;
        private bool _isMoving;


        private bool _movable;

        public List<MyLittleCombo> Combos;
        private readonly Menu _main;

        public ComboPanel(Config config)
        {
            _config = config;
            _main = _config.Factory.CreateMenu("Combo Panel");
            Enable = _main.CreateSwitcher("Enable", true);
            Movable = _main.CreateSwitcher("Movable", true);
            PosX = _main.CreateSlider("Pos X", 500, 0, 2500);
            PosY = _main.CreateSlider("Pos Y", 500, 0, 2500);
            Size = _main.CreateSlider("Size", 100, 0, 200);
            DrawingStartPosition = new Vector2(PosX, PosY);

            _iconSize = 50f / 100f * Size;

            Combos = new List<MyLittleCombo>();
            IsAutoComboSelected = true;
            for (var i = 0; i < 5; i++) Combos.Add(new MyLittleCombo(i, this));

            Combos.Add(new MyLittleCombo(-1, this));

            Size.ValueChanged += (sender, args) => { _iconSize = 50f / 100f * Size; };

            // if (Enable) Activate();
            MaxIcons = 0;

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
        }

        public MenuSlider Size { get; set; }

        public MenuSlider PosY { get; set; }

        public MenuSlider PosX { get; set; }

        public MenuSwitcher Movable { get; set; }

        public MenuSwitcher Enable { get; set; }


        public Vector2 DrawingStartPosition { get; set; }

        public MyLittleCombo SelectedCombo { get; set; }

        public bool IsAutoComboSelected { get; set; }


        public int MaxIcons { get; set; }


        private void Activate()
        {
            RendererManager.Draw += RendererOnDraw;
            InputManager.MouseKeyDown += OnComboClickSelecteor;
            if (Movable)
            {
                InputManager.MouseKeyDown += InputOnMouseClick;
                InputManager.MouseMove += InputOnMouseMove;
                _movable = true;
            }
        }

        private void Deactivate()
        {
            RendererManager.Draw -= RendererOnDraw;
            InputManager.MouseKeyDown -= OnComboClickSelecteor;
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
            var mousePos = e.Position;
            var size = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * MaxIcons,
                _iconSize);

            var isIn = size.Contains(mousePos);
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

        private void OnComboClickSelecteor(MouseEventArgs e)
        {
            if (e.MouseKey != MouseKey.Left)
                return;

            var mousePos = e.Position;
            var fullRectangleF = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * Math.Max(6, MaxIcons),
                _iconSize * (1 + Combos.Count(x => x.Enable || x.Id == -1)));
            if (fullRectangleF.Contains(mousePos))
            {
                e.Process = false;
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

                _config.Prepare.Start();
            }
        }

        private void RendererOnDraw()
        {
            if (MaxIcons == 0)
                return;
            var rect = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * Math.Max(6, MaxIcons),
                _iconSize - 2);
            RendererManager.DrawFilledRectangle(rect, new Color(0, 0, 0, 210), new Color(0, 127, 255, 10), 1f);
            rect.Height = (rect.Height + 2) * (1 + Combos.Count(x => x.Enable || x.Id == -1));
            RendererManager.DrawFilledRectangle(rect, new Color(0, 127, 255, 10), new Color(0, 0, 0, 155), 1f);

            rect.Height = _iconSize;
            RendererManager.DrawText("Combo Panel", rect, Color.White, FontFlags.Center, _iconSize * 0.75f);
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
                        RendererManager.DrawRectangle(startRect, Color.DodgerBlue);
                        RendererManager.DrawImage(item, startRect, AbilityImageType.Default, true);
                        if (combo.IsSelected && index == combo.AbilityInAction)
                            RendererManager.DrawFilledRectangle(startRect,
                                new Color(47, 173, 255, 75), new Color(0, 255, 255, 25), 1);
                        index++;
                        startRect.X += _iconSize;
                    }
                }
                else
                {
                    startRect = rect;
                    startRect.Width = _iconSize * Math.Max(6, MaxIcons);
                    RendererManager.DrawText("Dynamic Combo", startRect, Color.White, FontFlags.Center,
                        _iconSize * 0.75f);
                }

                startRect = rect;
                startRect.X += 1;
                startRect.Y += 1;
                startRect.Width = _iconSize * Math.Max(6, MaxIcons);
                startRect.Height -= 2;
                startRect.Width -= 2;
                combo.Rect = startRect;
                if (combo.IsSelected)
                {
                    RendererManager.DrawRectangle(startRect, Color.Fuchsia, 2f);
                }
                else
                {
                    var clr = new Color(50, 50, 50, 50);
                    RendererManager.DrawFilledRectangle(startRect, clr, clr, 1);
                }

                rect.Y += _iconSize;
            }
        }

        public class MyLittleCombo
        {
            private readonly ComboPanel _comboPanel;
            public bool IsSelected;
            public List<AbilityId> Items;
            public string Text;
            public MenuHoldKey Key;
            public MenuSwitcher Enable;

            public MyLittleCombo(int id, ComboPanel comboPanel)
            {
                _comboPanel = comboPanel;
                Id = id;
                Menu main;


                IsSelected = false;
                main = comboPanel._main.CreateMenu($"Combo #{id}");
                Enable = main.CreateSwitcher("Enable", Id == 0);
                Key = main.CreateHoldKey("Key");
                Enable.ValueChanged += OnUpdateToggle;
                Key.ValueChanged += (sender, args) =>
                {
                    if (Key && args.Value)
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
                var list = new List<AbilityId>
                {
                    AbilityId.item_refresher,
                    AbilityId.item_cyclone,
//                    AbilityId.invoker_ghost_walk.ToString()
                };
                list.AddRange(
                    comboPanel._config.Main.AbilitiesInCombo.AllAbilities.Select(ability => ability.Id));

                var dict = list.ToDictionary(x => x, x => true);
                Abilities = main.CreateAbilityToggler("Abilities:", dict, true);
                // AbilitiesPriority = main.CreateAbilityToggler("Priority:", dict.ToDictionary(z => z.Key, z => z.Value), true);
                NextAbilityAfterRefresher = main.CreateSlider("Ability index after refresher", 2, 0, 10);
                Abilities.ValueChanged += OnUpdate;

                UpdateManager.CreateIngameUpdate(500, () => { UpdateItems(); });

                // AbilitiesPriority.ValueChanged += OnUpdate;
                // if (Enable)
                //     UpdateItems(true);

                if (id == -1)
                {
                    main = comboPanel._main.CreateMenu("Dynamic combo");
                    Key = main.CreateHoldKey("Key");
                    Key.ValueChanged += (sender, args) =>
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
                }
            }

            public MenuSlider NextAbilityAfterRefresher { get; set; }

            // public MenuAbilityToggler AbilitiesPriority { get; set; }

            public MenuAbilityToggler Abilities { get; set; }

            public int Id { get; }

            public int AbilityInAction { get; set; }
            public RectangleF Rect { get; set; }


            private void OnUpdateToggle(MenuSwitcher switcher, SwitcherEventArgs switcherEventArgs)
            {
                UpdateItems(switcherEventArgs.Value);
            }

            private void OnUpdate(MenuAbilityToggler toggler, AbilityTogglerEventArgs abilityTogglerEventArgs)
            {
                UpdateItems();
            }


            public void UpdateItems(bool isFirstTime = false)
            {
                if (Abilities == null)
                {
                    return;
                }

                UpdateManager.BeginInvoke(150, () =>
                {
                    Items = new List<AbilityId>();
                    var allAbilities = _comboPanel._config.Main.AbilitiesInCombo.AllAbilities.Select(ability => ability.Id).ToList();
                    allAbilities.AddRange(new[]
                    {
                        AbilityId.item_refresher,
                        AbilityId.item_cyclone
                    });
                    // allAbilities.AddRange(_comboPanel._config.Main.AbilitiesInCombo.AllItems.Where(x => x != null && x.IsValid).Select(z => z.Id));
                    // Console.WriteLine($"allAbilities: {string.Join(';', allAbilities)}");
                    // Console.WriteLine(Abilities!=null);
                    var toAdd = allAbilities.Where(x => Abilities.GetValue(x)).ToList();
                    if (toAdd.Any())
                        Items.AddRange(toAdd);
                    Items = new List<AbilityId>(Items.OrderBy(x => Abilities.GetPriority(x)));
                    if (Enable || isFirstTime)
                    {
                        //var count = 0;
                        _comboPanel.MaxIcons = 0;
                        foreach (var combo in _comboPanel.Combos.Where(x => Items != null && x.Enable && x.Id >= 0))
                        {
                            var localCount = new List<AbilityId>();
                            localCount.AddRange(allAbilities.Where(x => combo.Abilities.GetValue(x)));
                            //_comboPanel.MaxIcons = Math.Max(Enable ? Items.Count : 0, localCount);
                            _comboPanel.MaxIcons = Math.Max(_comboPanel.MaxIcons, localCount.Count);
                            //InvokerCrappahilationPaid.Log.Warn($"[{count++}] Max: {_comboPanel.MaxIcons}");
                        }

                        //InvokerCrappahilationPaid.Log.Warn($"TotalMax for combo# {Id}: {_comboPanel.MaxIcons}");
                    }
                });
                /*if (_comboPanel.MaxIcons < Items.Count)
                    _comboPanel.MaxIcons = Items.Count;*/
            }
        }
    }
}