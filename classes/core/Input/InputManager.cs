using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        ButtonState lastLMBState = ButtonState.Released;
        ButtonState lastRMBState = ButtonState.Released;
        ButtonState lastMMBState = ButtonState.Released;
        #endregion

        public void Update()
        {
            UpdateMouse();
        }

        public void UpdateMouse()
        {
            #region LMB
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && lastLMBState != ButtonState.Pressed)
            {
                lastLMBState = ButtonState.Pressed;
                var args = new MouseButtonEventArgs(lastLMBState, MouseButtonState.Left, Mouse.GetState().Position);
                
                if(onLeftMousePressed != null)
                {
                    onLeftMousePressed.Invoke(args);
                }
                
            }

            if (Mouse.GetState().LeftButton == ButtonState.Released && lastLMBState != ButtonState.Released)
            {
                lastLMBState = ButtonState.Released;
                var args = new MouseButtonEventArgs(lastLMBState, MouseButtonState.Left, Mouse.GetState().Position);

                if(onLeftMouseReleased != null)
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
