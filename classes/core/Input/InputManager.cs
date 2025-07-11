﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tiled.DataStructures;

namespace Tiled.Input
{
    public class InputManager
    {

        #region MOUSE_INPUT
        public delegate void LMBPressed(MouseButtonEventArgs e);
        public delegate void LMBReleased(MouseButtonEventArgs e);

        public delegate void RMBPressed(MouseButtonEventArgs e);
        public delegate void RMBReleased(MouseButtonEventArgs e);

        public delegate void MMBPressed(MouseButtonEventArgs e);
        public delegate void MMBReleased(MouseButtonEventArgs e);


        public static event LMBPressed onLeftMousePressed;
        public static event LMBReleased onLeftMouseReleased;

        public static event RMBPressed onRightMousePressed;
        public static event RMBReleased onRightMouseReleased;

        public static event MMBPressed onMiddleMousePressed;
        public static event MMBReleased onMiddleMouseReleased;


        public delegate void KeyPressed(Keys key);
        public delegate void KeyReleased(Keys key);

        public static event KeyPressed onKeyPressed;
        public static event KeyReleased onKeyReleased;

        ButtonState lastLMBState = ButtonState.Released;
        ButtonState lastRMBState = ButtonState.Released;
        ButtonState lastMMBState = ButtonState.Released;
        #endregion


        public delegate void MouseWheelAxis(float axis);
        public static event MouseWheelAxis onMouseWheel;

        public static bool mouseHasItem = false;
        public static ContainerItem mouseItem;
        public static int mouseItemIndex = -1;
        private float lastScroll;

        static Dictionary<Keys, bool> pressedKeys = new Dictionary<Keys, bool>();
        public void Update()
        {
            UpdateMouse();
            UpdateKeyboard();
        }

        public void UpdateMouse()
        {
            #region LMB
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && lastLMBState != ButtonState.Pressed)
            {
                lastLMBState = ButtonState.Pressed;
                var args = new MouseButtonEventArgs(lastLMBState, MouseButtonState.Left, Mouse.GetState().Position);

                if (onLeftMousePressed != null)
                {
                    onLeftMousePressed.Invoke(args);
                }

            }

            if (Mouse.GetState().LeftButton == ButtonState.Released && lastLMBState != ButtonState.Released)
            {
                lastLMBState = ButtonState.Released;
                var args = new MouseButtonEventArgs(lastLMBState, MouseButtonState.Left, Mouse.GetState().Position);

                if (onLeftMouseReleased != null)
                {
                    onLeftMouseReleased.Invoke(args);
                }
            }
            #endregion

            //RIGHT MOUSE
            #region RMB
            if (Mouse.GetState().RightButton == ButtonState.Pressed && lastRMBState != ButtonState.Pressed)
            {
                lastRMBState = ButtonState.Pressed;
                var args = new MouseButtonEventArgs(lastRMBState, MouseButtonState.Right, Mouse.GetState().Position);

                if (onRightMousePressed != null)
                {
                    onRightMousePressed.Invoke(args);
                }
            }

            if (Mouse.GetState().RightButton == ButtonState.Released && lastRMBState != ButtonState.Released)
            {
                lastRMBState = ButtonState.Released;
                var args = new MouseButtonEventArgs(lastRMBState, MouseButtonState.Right, Mouse.GetState().Position);

                if (onRightMouseReleased != null)
                {
                    onRightMouseReleased.Invoke(args);
                }
            }
            #endregion

            //MIDDLE MOUSE
            #region MMB
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed && lastMMBState != ButtonState.Pressed)
            {
                lastMMBState = ButtonState.Pressed;
                var args = new MouseButtonEventArgs(lastMMBState, MouseButtonState.Middle, Mouse.GetState().Position);

                if (onMiddleMousePressed != null)
                {
                    onMiddleMousePressed.Invoke(args);
                }
            }

            if (Mouse.GetState().MiddleButton == ButtonState.Released && lastMMBState != ButtonState.Released)
            {
                lastMMBState = ButtonState.Released;
                var args = new MouseButtonEventArgs(lastMMBState, MouseButtonState.Middle, Mouse.GetState().Position);

                if (onMiddleMouseReleased != null)
                {
                    onMiddleMouseReleased.Invoke(args);
                }

            }
            #endregion


            float wheelDelta = Mouse.GetState().ScrollWheelValue - lastScroll;
            //Debug.WriteLine(wheelDelta);
            onMouseWheel?.Invoke(wheelDelta);
            lastScroll = Mouse.GetState().ScrollWheelValue;
        }

        public void UpdateKeyboard()
        {
            Keys[] keys = Keyboard.GetState().GetPressedKeys();

            foreach (var key in keys)
            {
                if (Keyboard.GetState().IsKeyDown(key) && !pressedKeys.ContainsKey(key))
                {
                    pressedKeys[key] = true;
                    onKeyPressed?.Invoke(key);
                }

                if (Keyboard.GetState().IsKeyUp(key) && pressedKeys[key])
                {
                    pressedKeys[key] = false;
                    onKeyReleased?.Invoke(key);
                }
            }
        }

        public static bool TryConvertKeyboardInput(KeyboardState keyboard, KeyboardState oldKeyboard, out char key)
        {
            Keys[] keys = keyboard.GetPressedKeys();
            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

            if (keys.Length > 0 && !oldKeyboard.IsKeyDown(keys[0]))
            {
                switch (keys[0])
                {
                    //Alphabet keys
                    case Keys.A: if (shift) { key = 'A'; } else { key = 'a'; } return true;
                    case Keys.B: if (shift) { key = 'B'; } else { key = 'b'; } return true;
                    case Keys.C: if (shift) { key = 'C'; } else { key = 'c'; } return true;
                    case Keys.D: if (shift) { key = 'D'; } else { key = 'd'; } return true;
                    case Keys.E: if (shift) { key = 'E'; } else { key = 'e'; } return true;
                    case Keys.F: if (shift) { key = 'F'; } else { key = 'f'; } return true;
                    case Keys.G: if (shift) { key = 'G'; } else { key = 'g'; } return true;
                    case Keys.H: if (shift) { key = 'H'; } else { key = 'h'; } return true;
                    case Keys.I: if (shift) { key = 'I'; } else { key = 'i'; } return true;
                    case Keys.J: if (shift) { key = 'J'; } else { key = 'j'; } return true;
                    case Keys.K: if (shift) { key = 'K'; } else { key = 'k'; } return true;
                    case Keys.L: if (shift) { key = 'L'; } else { key = 'l'; } return true;
                    case Keys.M: if (shift) { key = 'M'; } else { key = 'm'; } return true;
                    case Keys.N: if (shift) { key = 'N'; } else { key = 'n'; } return true;
                    case Keys.O: if (shift) { key = 'O'; } else { key = 'o'; } return true;
                    case Keys.P: if (shift) { key = 'P'; } else { key = 'p'; } return true;
                    case Keys.Q: if (shift) { key = 'Q'; } else { key = 'q'; } return true;
                    case Keys.R: if (shift) { key = 'R'; } else { key = 'r'; } return true;
                    case Keys.S: if (shift) { key = 'S'; } else { key = 's'; } return true;
                    case Keys.T: if (shift) { key = 'T'; } else { key = 't'; } return true;
                    case Keys.U: if (shift) { key = 'U'; } else { key = 'u'; } return true;
                    case Keys.V: if (shift) { key = 'V'; } else { key = 'v'; } return true;
                    case Keys.W: if (shift) { key = 'W'; } else { key = 'w'; } return true;
                    case Keys.X: if (shift) { key = 'X'; } else { key = 'x'; } return true;
                    case Keys.Y: if (shift) { key = 'Y'; } else { key = 'y'; } return true;
                    case Keys.Z: if (shift) { key = 'Z'; } else { key = 'z'; } return true;

                    //Decimal keys
                    case Keys.D0: if (shift) { key = ')'; } else { key = '0'; } return true;
                    case Keys.D1: if (shift) { key = '!'; } else { key = '1'; } return true;
                    case Keys.D2: if (shift) { key = '@'; } else { key = '2'; } return true;
                    case Keys.D3: if (shift) { key = '#'; } else { key = '3'; } return true;
                    case Keys.D4: if (shift) { key = '$'; } else { key = '4'; } return true;
                    case Keys.D5: if (shift) { key = '%'; } else { key = '5'; } return true;
                    case Keys.D6: if (shift) { key = '^'; } else { key = '6'; } return true;
                    case Keys.D7: if (shift) { key = '&'; } else { key = '7'; } return true;
                    case Keys.D8: if (shift) { key = '*'; } else { key = '8'; } return true;
                    case Keys.D9: if (shift) { key = '('; } else { key = '9'; } return true;

                    //Decimal numpad keys
                    case Keys.NumPad0: key = '0'; return true;
                    case Keys.NumPad1: key = '1'; return true;
                    case Keys.NumPad2: key = '2'; return true;
                    case Keys.NumPad3: key = '3'; return true;
                    case Keys.NumPad4: key = '4'; return true;
                    case Keys.NumPad5: key = '5'; return true;
                    case Keys.NumPad6: key = '6'; return true;
                    case Keys.NumPad7: key = '7'; return true;
                    case Keys.NumPad8: key = '8'; return true;
                    case Keys.NumPad9: key = '9'; return true;

                    //Special keys
                    case Keys.OemTilde: if (shift) { key = '~'; } else { key = '`'; } return true;
                    case Keys.OemSemicolon: if (shift) { key = ':'; } else { key = ';'; } return true;
                    case Keys.OemQuotes: if (shift) { key = '"'; } else { key = '\''; } return true;
                    case Keys.OemQuestion: if (shift) { key = '?'; } else { key = '/'; } return true;
                    case Keys.OemPlus: if (shift) { key = '+'; } else { key = '='; } return true;
                    case Keys.OemPipe: if (shift) { key = '|'; } else { key = '\\'; } return true;
                    case Keys.OemPeriod: if (shift) { key = '>'; } else { key = '.'; } return true;
                    case Keys.OemOpenBrackets: if (shift) { key = '{'; } else { key = '['; } return true;
                    case Keys.OemCloseBrackets: if (shift) { key = '}'; } else { key = ']'; } return true;
                    case Keys.OemMinus: if (shift) { key = '_'; } else { key = '-'; } return true;
                    case Keys.OemComma: if (shift) { key = '<'; } else { key = ','; } return true;
                    case Keys.Space: key = ' '; return true;
                }
            }

            key = (char)0;
            return false;
        }
    }

        public class MouseButtonEventArgs
        {
            public Point position;
            public ButtonState state;
            public MouseButtonState mouseButton;

            public MouseButtonEventArgs(ButtonState state, MouseButtonState mouseButton, Point position)
            {
                this.position = position;
                this.state = state;
                this.mouseButton = mouseButton;
            }
        }
}
