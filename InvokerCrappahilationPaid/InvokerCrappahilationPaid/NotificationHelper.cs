﻿using System.Collections.Generic;
using System.Linq;
using Divine.Core.Helpers;
using Divine.Core.Managers.Unit;
using Divine.Core.Entities;
using Ensage;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer;
using SharpDX;
using Color = System.Drawing.Color;

namespace InvokerCrappahilationPaid
{
    public class NotificationHelper
    {
        private readonly InvokerCrappahilationPaid _main;

        public List<Notification> Notifications;

        public NotificationHelper(InvokerCrappahilationPaid main)
        {
            _main = main;
            Renderer = main.Context.RenderManager;
            var startPosition = HUDInfo.GetCustomTopPanelPosition(0, Team.Radiant) +
                                new Vector2(0, HUDInfo.TopPanelSizeY);

            Notifications = new List<Notification>();
            var sizeX = HUDInfo.TopPanelSizeX;
            var sizeY = HUDInfo.TopPanelSizeY;
            Renderer.Draw += renderer =>
            {
                foreach (var notification in Notifications)
                    if (notification.IsActive)
                    {
                        var rect = new RectangleF(notification.StartPosition.X, notification.StartPosition.Y, sizeX,
                            notification.CurrentPosition.Y - notification.StartPosition.Y);
                        renderer.DrawFilledRectangle(rect, Color.FromArgb(155, 50, 50, 50),
                            Color.FromArgb(255, 0, 0, 0));
                        if (notification.State == Notification.StateType.Staying)
                        {
                            var r = new RectangleF(rect.X, rect.Y + rect.Height - sizeX, sizeX, sizeX);
                            renderer.DrawTexture(notification.TextureId, r);
                        }
                    }
            };

            UpdateManager.Subscribe(() =>
            {
                foreach (var notification in Notifications)
                    if (notification.IsActive)
                        notification.Move();

                Notifications.RemoveAll(x => !x.IsActive);
            }, 25);

            /*_main.Context.Input.RegisterHotkey("Toster", Key.S, args =>
            {
                Notificate(_main.Me, AbilityId.invoker_sun_strike);
            });*/
        }

        private IRenderManager Renderer { get; }

        public Notification Notificate(Hero hero, AbilityId id, float time)
        {
            var find = Notifications.FirstOrDefault(x => x.Hero.Equals(hero));
            if (find != null)
                return find;
            var n = new Notification(hero,
                HUDInfo.GetTopPanelPosition((CHero) UnitManager.GetUnitByHandle(hero.Handle)), id, time);
            Notifications.Add(n);
            return n;
        }

        public void Deactivate(Hero enemy)
        {
            var notification = Notifications.FirstOrDefault(x => x.Hero.Equals(enemy));
            if (notification != null) notification.State = Notification.StateType.Down;
        }

        public class Notification
        {
            public enum StateType
            {
                Up,
                Down,
                Staying
            }

            public Vector2 CurrentPosition;
            public bool IsActive;
            public float MaxSize;
            public Vector2 StartPosition;
            public StateType State;
            public string TextureId;
            public float Time;

            public Notification(Hero hero, Vector2 startPosition, AbilityId textureId, float maxTime)
            {
                Hero = hero;
                MaxTime = maxTime;
                State = StateType.Up;
                IsActive = true;
                StartPosition = startPosition + new Vector2(0, HUDInfo.TopPanelSizeY);
                CurrentPosition = StartPosition;
                TextureId = textureId.ToString();
                MaxSize = StartPosition.Y + HUDInfo.TopPanelSizeY * 2.2f;
            }

            public Hero Hero { get; }
            public float MaxTime { get; }

            public void Move()
            {
                switch (State)
                {
                    case StateType.Up:
                        CurrentPosition.Y += 5;
                        if (CurrentPosition.Y >= MaxSize)
                        {
                            State = StateType.Staying;
                            Time = Game.RawGameTime;
                        }

                        break;
                    case StateType.Down:
                        CurrentPosition.Y -= 5;
                        if (CurrentPosition.Y <= StartPosition.Y) IsActive = false;
                        break;
                    case StateType.Staying:
                        if (MaxTime > 0)
                            if (Game.RawGameTime - Time >= MaxTime)
                                State = StateType.Down;
                        break;
                }
            }
        }
    }
}