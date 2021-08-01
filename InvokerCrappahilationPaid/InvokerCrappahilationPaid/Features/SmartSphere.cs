using System;
using System.Linq;
using Divine.Entity.Entities;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Components;
using Divine.Entity.Entities.Players;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Extensions;
using Divine.Input;
using Divine.Input.EventArgs;
using Divine.Menu.Items;
using Divine.Numerics;
using Divine.Order;
using Divine.Order.EventArgs;
using Divine.Order.Orders;
using Divine.Order.Orders.Components;
using Divine.Renderer;
using Divine.Update;
using O9K.Core.Helpers;

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
        private readonly MultiSleeper<string> _multySleeper;

        public Button[] Buttons;

        public Sleeper InChanging = new Sleeper();

        public SmartSphere(Config config)
        {
            _config = config;
            var main = _config.Factory.CreateMenu("Smart Sphere");
            Enable = main.CreateSwitcher("Enable", true);
            DisableKey = main.CreateHoldKey("Disable key");
            CheckForModifiers = main.CreateSwitcher("Check for modifiers", true);
            VerySmartSpheres = main.CreateSwitcher("Very smart spheres", true);
            HpSlider = main.CreateSlider("Hp % for VerySmartSpheres", 80, 1, 99);
            VerySmartSpheres.SetTooltip(
                "Will use quas on moving if u have less then 50% hp. And wex on moving if more then 50%");
            //Movable = main.Item("Movable", true);
            PosX = main.CreateSlider("Pos X", 500, 0, 2500);
            PosY = main.CreateSlider("Pos Y", 500, 0, 2500);
            Size = main.CreateSlider("Size", 00, 0, 200);
            DrawingStartPosition = new Vector2(PosX, PosY);
            _iconSize = 50f / 100f * Size;
            _multySleeper = new MultiSleeper<string>();
            Size.ValueChanged += (sender, args) => { _iconSize = 50f / 100f * Size; };

            // if (Enable) Activate();

            UpdateManager.BeginInvoke(500, () => { MaxIcons = config.Main.AbilitiesInCombo.AllAbilities.Count; });

            Buttons = new Button[6];

            Buttons[0] = new Button(AbilityId.invoker_quas, TypeEnum.Attack, false);
            Buttons[1] = new Button(AbilityId.invoker_wex, TypeEnum.Attack, false);
            Buttons[2] = new Button(AbilityId.invoker_exort, TypeEnum.Attack, true);

            Buttons[3] = new Button(AbilityId.invoker_quas, TypeEnum.Move, true);
            Buttons[4] = new Button(AbilityId.invoker_wex, TypeEnum.Move, false);
            Buttons[5] = new Button(AbilityId.invoker_exort, TypeEnum.Move, false);


            Enable.ValueChanged += (sender, args) =>
            {
                if (args.Value)
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
            };*/
        }

        public MenuSlider PosY { get; set; }

        public MenuSlider PosX { get; set; }

        public MenuSwitcher VerySmartSpheres { get; set; }

        public MenuSlider HpSlider { get; set; }

        public MenuSwitcher CheckForModifiers { get; set; }

        public MenuSlider Size { get; set; }

        public MenuHoldKey DisableKey { get; set; }

        public MenuSwitcher Enable { get; set; }


        public Vector2 DrawingStartPosition { get; set; }
        private Hero Me => _config.Main.Me;

        public int MaxIcons { get; set; }


        private void Activate()
        {
            RendererManager.Draw += RendererOnDraw;
            //Entity.OnInt32PropertyChange += OnNetworkActivity;
            OrderManager.OrderAdding += PlayerOnOnExecuteOrder;
            InChanging = new Sleeper();
            if (true)
            {
                InputManager.MouseKeyDown += InputOnMouseClick;
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
                InputManager.MouseKeyDown -= InputOnMouseClick;
                InputManager.MouseMove -= InputOnMouseMove;
                _movable = false;
            }
        }

        private void PlayerOnOnExecuteOrder(OrderAddingEventArgs args)
        {
            /*if (!args.IsPlayerInput)
                return;*/
            if (!args.Order.Units.Any(x => x.Equals(Me)))
                return;
            if (_config.ComboKey || DisableKey)
                return;
            if (Me.IsInvisible() || UnitExtensions.HasAnyModifiers(Me, "modifier_invoker_ghost_walk_self",
                "modifier_rune_invis", "modifier_invisible"))
                return;
            var order = args.Order.Type;
            if (!args.IsCustom)
                if (order == OrderType.Cast)
                {
                    var abilityId = args.Order.Ability?.Id;
                    if (abilityId is AbilityId.invoker_quas or AbilityId.invoker_wex or AbilityId.invoker_exort or AbilityId.invoker_invoke or AbilityId.invoker_ghost_walk)
                        Sleeper.Sleep(1.500f);
                }

            if (Sleeper.IsSleeping || Me.IsSilenced())
                return;
            if (order is OrderType.AttackPosition or OrderType.AttackTarget)
            {
                if (_multySleeper.IsSleeping("attack"))
                    return;
                _multySleeper.Sleep("attack", .250f);
                var activeSphereForAttack =
                    Me.GetAbilityById(Buttons.First(x => x.IsActive && x.Type == TypeEnum.Attack).Id);
                if (activeSphereForAttack.Level > 0)
                {
                    if (CheckForModifiers)
                    {
                        var countOfModifiers =
                            Me.Modifiers.Count(x => x.Name == $"modifier_{activeSphereForAttack.Id}_instance");
                        if (countOfModifiers >= 3) return;
                        for (var i = countOfModifiers; i < 3; i++) activeSphereForAttack.Cast();
                        InChanging.Sleep(.250f);
                    }
                    else
                    {
                        InChanging.Sleep(.250f);
                        activeSphereForAttack.Cast();
                        activeSphereForAttack.Cast();
                        activeSphereForAttack.Cast();
                    }
                }
            }
            else if (order is OrderType.MovePosition or OrderType.MoveTarget)
            {
                if (args.Order.Target != null && args.Order.Target.NetworkName == ClassId.CDOTA_BaseNPC_Healer.ToString())
                    return;

                if (_multySleeper.IsSleeping("move"))
                    return;
                _multySleeper.Sleep("move", .250f);

                var activeSphereForMove =
                    Me.GetAbilityById(Buttons.First(x => x.IsActive && x.Type == TypeEnum.Move).Id);
                if (VerySmartSpheres)
                {
                    if (Me.HealthPercent() <= HpSlider / 100f)
                    {
                        if (_config.Main.AbilitiesInCombo.Quas.BaseAbility.Level > 0)
                            activeSphereForMove = _config.Main.AbilitiesInCombo.Quas.BaseAbility;
                        else
                            activeSphereForMove = _config.Main.AbilitiesInCombo.Wex.BaseAbility;
                    }
                    else
                    {
                        if (_config.Main.AbilitiesInCombo.Wex.BaseAbility.Level > 0)
                            activeSphereForMove = _config.Main.AbilitiesInCombo.Wex.BaseAbility;
                        else
                            activeSphereForMove = _config.Main.AbilitiesInCombo.Quas.BaseAbility;
                    }

                    foreach (var typeButton in Buttons.Where(x => x.Type == TypeEnum.Move))
                        typeButton.IsActive = typeButton.Id == activeSphereForMove.Id;
                }

                if (activeSphereForMove.Level > 0)
                {
                    if (CheckForModifiers)
                    {
                        var countOfModifiers =
                            Me.Modifiers.Count(x => x.Name == $"modifier_{activeSphereForMove.Id}_instance");
                        if (countOfModifiers >= 3) return;
                        for (var i = countOfModifiers; i < 3; i++) activeSphereForMove.Cast();
                        InChanging.Sleep(.250f);
                    }
                    else
                    {
                        InChanging.Sleep(.250f);
                        activeSphereForMove.Cast();
                        activeSphereForMove.Cast();
                        activeSphereForMove.Cast();
                    }
                }
            }

            if (args.IsCustom)
                if (!InChanging.IsSleeping && order == OrderType.Cast)
                {
                    var abilityId = args.Order.Ability?.Id;
                    if (abilityId is AbilityId.invoker_quas or AbilityId.invoker_wex or AbilityId.invoker_exort or AbilityId.invoker_invoke or AbilityId.invoker_ghost_walk)
                    {
                        _multySleeper.Sleep("attack", .250f);
                        _multySleeper.Sleep("move", .250f);
                        //InvokerCrappahilationPaid.Log.Warn($"On Sleep");
                    }
                }
        }

        // private void OnNetworkActivity(Entity sender, Int32PropertyChangeEventArgs args)
        // {
        //     if (sender != Me) return;
        //
        //     if (!args.PropertyName.Equals("m_networkactivity", StringComparison.InvariantCultureIgnoreCase)) return;
        //     var order = args.NewValue;
        //     if (order == 1503 || order == 1504)
        //     {
        //         _config.Main.AbilitiesInCombo.Exort.UseAbility();
        //         _config.Main.AbilitiesInCombo.Exort.UseAbility();
        //         _config.Main.AbilitiesInCombo.Exort.UseAbility();
        //     }
        //     else
        //     {
        //         _config.Main.AbilitiesInCombo.Wex.UseAbility();
        //         _config.Main.AbilitiesInCombo.Wex.UseAbility();
        //         _config.Main.AbilitiesInCombo.Wex.UseAbility();
        //     }
        //
        //     Console.WriteLine(args.NewValue);
        // }

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
            var size = new RectangleF(DrawingStartPosition.X, DrawingStartPosition.Y, _iconSize * 2, _iconSize * 2);

            var isIn = size.Contains(e.Position);
            if (_isMoving && e.MouseKey == MouseKey.Left)
            {
                PosX.Value =  (int) DrawingStartPosition.X;
                PosY.Value =  (int) DrawingStartPosition.Y;
                _isMoving = false;
            }
            else if (e.MouseKey == MouseKey.Left)
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
            RendererManager.DrawFilledRectangle(allRect, new Color(0, 127, 255, 10), new Color(0, 0, 0, 200), 0);
            RendererManager.DrawText( "Attack", attackRectangleF,Color.White, FontFlags.Center, _iconSize * 0.75f);
            RendererManager.DrawText( "Move", movingRectangleF,Color.White, FontFlags.Center, _iconSize * 0.75f);
            attackRectangleF.Width = _iconSize;
            attackRectangleF.X += _iconSize * 2;
            DrawButton(Buttons[0], ref attackRectangleF);
            DrawButton(Buttons[1], ref attackRectangleF);
            DrawButton(Buttons[2], ref attackRectangleF);

            movingRectangleF.Width = _iconSize;
            movingRectangleF.X += _iconSize * 2;
            DrawButton(Buttons[3], ref movingRectangleF);
            DrawButton(Buttons[4], ref movingRectangleF);
            DrawButton(Buttons[5], ref movingRectangleF);

//            renderer.DrawRectangle(allRect, Color.Chartreuse);
        }

        private void DrawButton(Button button, ref RectangleF rect)
        {
            // RendererManager.DrawImage(button.TextureId, rect, button.IsActive ? 1f : 0.2f);
            RendererManager.DrawImage(button.Id, rect, AbilityImageType.Round, true);
            if (button.IsActive)
            {
                RendererManager.DrawCircle(rect.Center, _iconSize / 2, Color.Aqua);
            }
            button.RectangleF = rect;
            /*if (!button.IsActive)
            {
                Renderer.DrawFilledRectangle(rect, Color.Chartreuse, new Color(200, 0, 0, 0), 0);
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
                Type = type;
                IsActive = isActive;
            }

        }
    }
}