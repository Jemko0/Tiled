using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.Gameplay;
using Tiled.UI.UserWidgets;
using Tiled.UI.Widgets;

namespace Tiled.UI
{
    //Ugly UI Code
    public class HUD
    {
        protected SpriteBatch spriteBatch;
        protected GraphicsDeviceManager graphicsDeviceManager;
        public static List<Widget> activeWidgets = new List<Widget>();
        public static float DPIScale = 1.0f;
        private RasterizerState _rasterizerState;

        public HUD(SpriteBatch sb, GraphicsDeviceManager gdm)
        {
            spriteBatch = sb;
            graphicsDeviceManager = gdm;
            Program.GetGame().Window.ClientSizeChanged += MainWindowResized;
            Init();
            InvalidateLayout();
        }

        private void MainWindowResized(object sender, EventArgs e)
        {
            InvalidateLayout();
        }

        private void InvalidateLayout()
        {
            GetDPIScale();
            RescaleWidgets();
        }

        /// <summary>
        /// Creates a new widget and adds it to the active widget list, args are usually (ownerHUD)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T CreateWidget<T>(params object?[]? args) where T : Widget
        {
            T newWidget = (T)Activator.CreateInstance(typeof(T), args);
            newWidget.onWidgetDestroyed += HUDElementDestroyed;
            newWidget.Construct();
            activeWidgets.Add(newWidget);
            return newWidget;
        }

        public void ClearAllWidgets()
        {
            foreach(var widget in activeWidgets)
            {
                widget.DestroyWidget();
            }

            activeWidgets.Clear();
        }

        internal void Init()
        {
#if !TILEDSERVER
            var title = CreateWidget<UWTitle>(this);
            title.SetGeometry(new Vector2(1920, 1080), DataStructures.AnchorPosition.Center);

            _rasterizerState = new RasterizerState();
            _rasterizerState.ScissorTestEnable = true;
            _rasterizerState.MultiSampleAntiAlias = false;
            _rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
#else
            var text = CreateWidget<WText>(this);
            text.SetGeometry(new Vector2(1920, 1080), DataStructures.AnchorPosition.Center);
            text.fontScale = 3.0f;
            text.justification = DataStructures.ETextJustification.Center;
            text.text = "SERVER INSTANCE";
#endif
        }

        private void GetDPIScale()
        {
            DPIScale = Program.GetGame().Window.ClientBounds.Height / 1080.0f;
        }

        private void RescaleWidgets()
        {
            foreach (var widget in activeWidgets)
            {
                widget.ScaleGeometry();
            }
        }

        private static void HUDElementDestroyed(DataStructures.WidgetDestroyArgs e)
        {
            e.destroyedWidget.Dispose();
        }
        
        public void DrawWidgets()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rasterizerState);

            // Only draw widgets that don't have a parent (root widgets)
            for(int i = 0; i < activeWidgets.Count; i++)
            {
                if (activeWidgets[i].parent == null && !activeWidgets[i].disposed)
                {
                    activeWidgets[i].Draw(ref spriteBatch);
                }
            }

            spriteBatch.End();
        }
    }
}
