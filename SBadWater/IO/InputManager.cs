using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SBadWater.IO
{
    public enum InputKey
    {
        LeftButton,
        RightButton,
    }

    public class InputManager
    {
        public delegate void ButtonPressedHandler(InputKey key, float holdDurationMs);
        public event ButtonPressedHandler OnButtonPressed;

        public delegate void ButtonReleasedHandler(InputKey key);
        public event ButtonReleasedHandler OnButtonReleased;

        public delegate void MouseMovedHandler(MouseState mouseState, MouseState oldMouseState);
        public event MouseMovedHandler OnMouseMoved;

        private MouseState? _mouseState;
        private MouseState? _oldMouseState;

        private readonly Dictionary<InputKey, float> _holdCooldowns = new();
        private readonly Dictionary<InputKey, float> _maxHoldCooldowns = new();
        private readonly Dictionary<InputKey, float> _totalHoldDurations = new();

        public InputManager()
        {

            foreach (InputKey key in Enum.GetValues<InputKey>())
            {
                _maxHoldCooldowns[key] = 200f;
                _holdCooldowns[key] = 0f;
                _totalHoldDurations[key] = 0f;
            }
        }

        public Point? MousePosition => _mouseState?.Position;

        public void ResetCooldown(InputKey key)
        {
            _holdCooldowns[key] = 0f;
        }

        public void Update(float elapsedTimeMs)
        {
            _mouseState = Mouse.GetState();

            if (_oldMouseState.HasValue && _mouseState?.Position != _oldMouseState?.Position)
            {
                OnMouseMoved?.Invoke(_mouseState.Value, _oldMouseState.Value);
            }

            CheckButtonPress(InputKey.LeftButton, _mouseState?.LeftButton, _oldMouseState?.LeftButton, elapsedTimeMs);
            CheckButtonPress(InputKey.RightButton, _mouseState?.RightButton, _oldMouseState?.RightButton, elapsedTimeMs);

            _oldMouseState = _mouseState;
        }

        private void CheckButtonPress(InputKey key, ButtonState? state, ButtonState? oldState, float elapsedTimeMs)
        {
            if (oldState == ButtonState.Pressed)
            {
                if (state == ButtonState.Released)
                {
                    // Clear cooldowns and timers because button was just released
                    _holdCooldowns[key] = 0f;
                    _totalHoldDurations[key] = 0f;
                    OnButtonReleased?.Invoke(key);
                }
                else
                {
                    _holdCooldowns[key] = Math.Max(_holdCooldowns[key] - elapsedTimeMs, 0f);
                    _totalHoldDurations[key] += elapsedTimeMs;
                }
            }

            if (state == ButtonState.Pressed && _holdCooldowns[key] == 0f)
            {
                OnButtonPressed?.Invoke(key, _totalHoldDurations[key]);
                _holdCooldowns[key] = _maxHoldCooldowns[key];
            }
        }

    }
}
