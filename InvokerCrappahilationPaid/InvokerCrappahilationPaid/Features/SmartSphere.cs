using System;
using System.Linq;
<<<<<<< HEAD

=======
using Divine;
using O9K.Core.Entities.Heroes;
>>>>>>> e5540ca6453d07fa19eccaaee870d87217e5a893
using SharpDX;

using Color = System.Drawing.Color;
using InputManager = Divine.Core.Managers.Input.InputManager;

namespace InvokerCrappahilationPaid.Features
{
    public class SmartSphere
    {
        public enum TypeEnum
        {
            Attack,
            Move
        }

        private readonly Config _config;
        public readonly Sleeper Sleeper;
        private Vector2 _drawMousePosition;
        private float _iconSize;
        private bool _isMoving;

        private bool _movable;
        private readonly MultiSleeper _multySleeper;

        public Button[] Buttons;

        public Sleeper InChanging = new Sleeper();

        public SmartSphere(Config config)
        {
            _config = config;
            var main = _config.Factory.CreateMenu("Smart Sphere");
            Enable = main.CreateSwitcher("Enable", true);
            DisableKey = main.CreateToggleKey("Disable key");
            CheckForModifiers = main.CreateSwitcher("Check for modifiers", true);
            VerySmartSpheres = main.CreateSwitcher("Very smart spheres", true);
            HpSlider = main.CreateSlider("Hp % for VerySmartSpheres", 80, 1, 99);
            VerySmartSpheres.Item.SetTooltip(
                "Will use quas on moving if u have less then 50% hp. And wex on moving if more then 50%");
            //Movable = main.Item("Movable", true);
            PosX = main.CreateSlider("Pos X",500, 0, 2500);
            PosY = main.CreateSlider("Pos Y", 500, 0, 2500);
            Size = main.CreateSlider("Size", 100, 0, 200);
            DrawingStartPosition = new Vector2(PosX, PosY);
            _iconSize = 50f / 100f * Size;
            _multySleeper = new MultiSleeper();
            Size.PropertyChanged += (sender, args) => { _iconSize = 50f / 100f * Size; };

            if (Enable) Activate();

            UpdateManager.BeginInvoke(() => { MaxIcons = config.Main.AbilitiesInCombo.AllAbilities.Count; }, 500);

            Buttons = new Button[6];

            Buttons[0] = new Button(AbilityId.invoker_quas, TypeEnum.Attack, false);
            Buttons[1] = new Button(AbilityId.invoker_wex, TypeEnum.Attack, false);
            Buttons[2] = new Button(AbilityId.invoker_exort, TypeEnum.Attack, true);

            Buttons[3] = new Button(AbilityId.invoker_quas, TypeEnum.Move, true);
            Buttons[4] = new Button(AbilityId.invoker_wex, TypeEnum.Move, false);
            Buttons[5] = new Button(AbilityId.invoker_exort, TypeEnum.Move, false);


            Enable.PropertyChanged += (sender, args) =>
            {
                if (Enable)
                    Activate();
                else
                    Deactivate();
            };
            Sleeper = new Sleeper();
            /*Movable.PropertyChanged += (sender, args) =>
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
            };*/
        }

        private InputManager InputManager => InputManager;

        public Vector2 DrawingStartPosition { get; set; }
        private Hero9 Me => _config.Main.Me;

        public int MaxIcons { get; set; }



        private void Activate()
        {
            RendererManager.Draw += RendererOnDraw;
            //Entity.OnInt32PropertyChange += OnNetworkActivity;
            OrderManager.OrderAdding += PlayerOnOnExecuteOrder;
            InChanging = new Sleeper();
            if (true)
            {
                InputManager.MouseClick += InputOnMouseClick;
                InputManager.MouseMove += InputOnMouseMove;
                _movable = true;
            }
        }

        private void Deactivate()
        {
            RendererManager.Draw -= RendererOnDraw;
            OrderManager.OrderAdding -= PlayerOnOnExecuteOrder;
            if (_movable)
            {
                InputManager.MouseClick -= InputOnMouseClick;
                InputManager.MouseMove -= InputOnMouseMove;
                _movable = false;
            }
        }

        private void PlayerOnOnExecuteOrder(OrderAddingEventArgs orderAddingEventArgs)
        {
            /*if (!args.IsPlayerInput)
                return;*/
            if (!args.Entities.Any(x => x.Equals(Me)))
                return;
            if (_config.ComboKey || DisableKey)
                return;
            if (Me.IsInvisible() || UnitExtensions.HasAnyModifiers(Me, "modifier_invoker_ghost_walk_self",
                    "modifier_rune_invis", "modifier_invisible"))
                return;
            var order = args.OrderId;
            if (args.IsPlayerInput)
                if (order == OrderId.Ability)
                {
                    var abilityId = args.Ability.Id;
                    if (abilityId == AbilityId.invoker_quas || abilityId == AbilityId.invoker_wex ||
                        abilityId == AbilityId.invoker_exort || abilityId == AbilityId.invoker_invoke ||
                        abilityId == AbilityId.invoker_ghost_walk)
                        Sleeper.Sleep(1500);
                }

            if (Sleeper.Sleeping || Me.IsSilenced())
                return;
            if (order == OrderId.AttackLocation || order == OrderId.AttackTarget)
            {
                if (_multySleeper.Sleeping("attack"))
                    return;
                _multySleeper.Sleep(250, "attack");
                var activeSphereForAttack =
                    Me.GetAbilityById(Buttons.First(x => x.IsActive && x.Type == TypeEnum.Attack).Id);
                if (activeSphereForAttack.CanBeCasted())
                {
                    if (CheckForModifiers)
                    {
                        var countOfModifiers =
                            Me.Modifiers.Count(x => x.Name == $"modifier_{activeSphereForAttack.Id}_instance");
                        if (countOfModifiers >= 3) return;
                        for (var i = countOfModifiers; i < 3; i++) activeSphereForAttack.UseAbility();
                        InChanging.Sleep(250);
                    }
                    else
                    {
                        InChanging.Sleep(250);
                        activeSphereForAttack.UseAbility();
                        activeSphereForAttack.UseAbility();
                        activeSphereForAttack.UseAbility();
                    }
                }
            }
            else if (order == OrderId.MoveLocation || order == OrderId.MoveTarget)
            {
                if (args.Target != null && args.Target.NetworkName == ClassId.CDOTA_BaseNPC_Healer.ToString())
                    return;

                if (_multySleeper.Sleeping("move"))
                    return;
                _multySleeper.Sleep(250, "move");

                var activeSphereForMove =
                    Me.GetAbilityById(Buttons.First(x => x.IsActive && x.Type == TypeEnum.Move).Id);
                if (VerySmartSpheres)
                {
                    if (UnitExtensions.HealthPercent(Me) <= HpSlider / 100f)
                    {
                        if (_config.Main.AbilitiesInCombo.Quas.Level > 0)
                            activeSphereForMove = _config.Main.AbilitiesInCombo.Quas;
                        else
                            activeSphereForMove = _config.Main.AbilitiesInCombo.Wex;
                    }
                    else
                    {
                        if (_config.Main.AbilitiesInCombo.Wex.Level > 0)
                            activeSphereForMove = _config.Main.AbilitiesInCombo.Wex;
                        else
                            activeSphereForMove = _config.Main.AbilitiesInCombo.Quas;
                    }

                    foreach (var typeButton in Buttons.Where(x => x.Type == TypeEnum.Move))
                        typeButton.IsActive = typeButton.Id == activeSphereForMove.Id;
                }

                if (activeSphereForMove.CanBeCasted())
                {
                    if (CheckForModifiers)
                    {
                        var countOfModifiers =
                            Me.Modifiers.Count(x => x.Name == $"modifier_{activeSphereForMove.Id}_instance");
                        if (countOfModifiers >= 3) return;
                        for (var i = countOfModifiers; i < 3; i++) activeSphereForMove.UseAbility();
                        InChanging.Sleep(250);
                    }
                    else
                    {
                        InChanging.Sleep(250);
                        activeSphereForMove.UseAbility();
                        activeSphereForMove.UseAbility();
                        activeSphereForMove.UseAbility();
                    }
                }
            }

            if (!args.IsPlayerInput)
                if (!InChanging.Sleeping && order == OrderId.Ability)
                {
                    var abilityId = args.Ability.Id;
                    if (abilityId == AbilityId.invoker_quas || abilityId == AbilityId.invoker_wex ||
                        abilityId == AbilityId.invoker_exort || abilityId == AbilityId.invoker_invoke ||
                        abilityId == AbilityId.invoker_ghost_walk)
                    {
                        _multySleeper.Sleep(250, "attack");
                        _multySleeper.Sleep(250, "move");
                        //InvokerCrappahilationPaid.Log.Warn($"On Sleep");
                    }
                }
        }

        private void OnNetworkActivity(Entity sender, Int32PropertyChangeEventArgs args)
        {
            if (sender != Me) return;

            if (!args.PropertyName.Equals("m_networkactivity", StringComparison.InvariantCultureIgnoreCase)) return;
            var order = args.NewValue;
            if (order == 1503 || order == 1504)
            {
                _config.Main.AbilitiesInCombo.Exort.UseAbility();
                _config.Main.AbilitiesInCombo.Exort.UseAbility();
                _config.Main.AbilitiesInCombo.Exort.UseAbility();
            }
            else
            {
                _config.Main.AbilitiesInCombo.Wex.UseAbility();
                _config.Main.AbilitiesInCombo.Wex.UseAbility();
                _config.Main.AbilitiesInCombo.Wex.UseAbility();
            }

            Console.WriteLine(args.NewValue);
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
            var size = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * 2, _iconSize * 2);

            var isIn = size.Contains(e.Position);
            if (_isMoving && (e.Buttons & MouseButtons.LeftUp) == MouseButtons.LeftUp)
            {
                PosX.Item.SetValue(new Slider((int) DrawingStartPosition.X, 0, 2500));
                PosY.Item.SetValue(new Slider((int) DrawingStartPosition.Y, 0, 2500));
                _isMoving = false;
            }
            else if ((e.Buttons & MouseButtons.LeftDown) == MouseButtons.LeftDown)
            {
                if (isIn)
                {
                    var startPos = new Vector2(PosX, PosY);
                    _drawMousePosition = e.Position - startPos;
                    _isMoving = true;
                }
                else
                {
                    foreach (var button in Buttons)
                        if (button.RectangleF.Contains(e.Position))
                        {
                            foreach (var typeButton in Buttons.Where(x => x.Type == button.Type))
                                typeButton.IsActive = false;
                            button.IsActive = true;
                            break;
                        }
                }
            }
        }

        private void RendererOnDraw()
        {
            if (MaxIcons == 0)
                return;
            var attackRectangleF =
                new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * 5, _iconSize);
            var movingRectangleF = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y + _iconSize,
                _iconSize * 5, _iconSize);
            var allRect = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * 5, _iconSize * 2);


            attackRectangleF.Width = _iconSize * 2;
            movingRectangleF.Width = _iconSize * 2;
            renderer.DrawFilledRectangle(allRect, Color.FromArgb(10,127,255,0), Color.FromArgb(200, 0, 0, 0), 0);
            renderer.DrawText(attackRectangleF, "Attack", Color.White,
                RendererFontFlags.Center, _iconSize * 0.75f);
            renderer.DrawText(movingRectangleF, "Move", Color.White,
                RendererFontFlags.Center, _iconSize * 0.75f);
            attackRectangleF.Width = _iconSize;
            attackRectangleF.X += _iconSize * 2;
            DrawButton(renderer, Buttons[0], ref attackRectangleF);
            DrawButton(renderer, Buttons[1], ref attackRectangleF);
            DrawButton(renderer, Buttons[2], ref attackRectangleF);

            movingRectangleF.Width = _iconSize;
            movingRectangleF.X += _iconSize * 2;
            DrawButton(renderer, Buttons[3], ref movingRectangleF);
            DrawButton(renderer, Buttons[4], ref movingRectangleF);
            DrawButton(renderer, Buttons[5], ref movingRectangleF);

//            renderer.DrawRectangle(allRect, Color.Chartreuse);
        }

        private void DrawButton(IRenderer renderer, Button button, ref RectangleF rect)
        {
            renderer.DrawTexture(button.TextureId, rect, opacity: button.IsActive ? 1f : 0.2f);
            button.RectangleF = rect;
            /*if (!button.IsActive)
            {
                Renderer.DrawFilledRectangle(rect, Color.Chartreuse, Color.FromArgb(200, 0, 0, 0), 0);
            }*/
            rect.X += _iconSize;
        }

        public class Button
        {
            public AbilityId Id;
            public bool IsActive;
            public RectangleF RectangleF;
            public TypeEnum Type;

            public Button(AbilityId id, TypeEnum type, bool isActive)
            {
                Id = id;
                TextureId = id.ToString();
                Type = type;
                IsActive = isActive;
            }

            public string TextureId { get; set; }
        }
    }
}