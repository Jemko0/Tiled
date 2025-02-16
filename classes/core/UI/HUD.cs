using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.UI.UserWidgets;

namespace Tiled.UI
{
    public class HUD
    {
        protected SpriteBatch spriteBatch;
        protected GraphicsDeviceManager GDM;
        public static List<Widget> activeWidgets = new List<Widget>();
        public static float DPIScale = 1.0f;

        public HUD(SpriteBatch sb, GraphicsDeviceManager gdm)
        {
            spriteBatch = sb;
            GDM = gdm;
            Program.GetGame().Window.ClientSizeChanged += MainWindowResized;
            Init();
            Recalc();
        }

        private void MainWindowResized(object sender, EventArgs e)
        {
            Recalc();
        }

        private void Recalc()
        {
            GetDPIScale();
            ScaleWidgets();
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

        internal void Init()
        {
            var title = CreateWidget<UWTitle>(this);
            title.SetGeometry(new Vector2(1920, 1080), DataStructures.AnchorPosition.Center);

            //CreateWidget<UWMessage>(this, "There was an error erroring an error.");
        }

        private void GetDPIScale()
        {
            DPIScale = Program.GetGame().Window.ClientBounds.Height / 1080.0f;
        }

        private void ScaleWidgets()
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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap);
            foreach (Widget w in activeWidgets.ToList())
            {
                w.Draw(ref spriteBatch);
            }
            spriteBatch.End();
        }
    }
}
