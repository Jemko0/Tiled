﻿using Microsoft.Xna.Framework;

namespace Tiled
{
    public class Rendering
    {
        public static Rectangle GetTileTransform(int x, int y)
        {
            int screenX = (int)(x * World.TILESIZE); // Convert to world space
            int screenY = (int)(y * World.TILESIZE);

            screenX = (int)((screenX - Program.GetGame().localCamera.position.X) * Main.renderScale); // Apply camera and scale together
            screenY = (int)((screenY - Program.GetGame().localCamera.position.Y) * Main.renderScale);

            screenX += Main.screenCenter.X; // Center on screen
            screenY += Main.screenCenter.Y;


            int screenSize = (int)(World.TILESIZE * Main.renderScale) + 1;

            Rectangle renderRect = new Rectangle
            (
                screenX,
                screenY,
                screenSize,
                screenSize
            );

            return renderRect;
        }

        public static Rectangle GetLightTileTransform(int x, int y)
        {
            int screenX = (int)(x * World.TILESIZE); // Convert to world space
            int screenY = (int)(y * World.TILESIZE);

            screenX = (int)((screenX - Program.GetGame().localCamera.position.X) * Main.renderScale); // Apply camera and scale together
            screenY = (int)((screenY - Program.GetGame().localCamera.position.Y) * Main.renderScale);

            screenX += Main.screenCenter.X; // Center on screen
            screenY += Main.screenCenter.Y;


            int screenSize = (int)(World.TILESIZE * Main.renderScale) + 1;

            Rectangle renderRect = new Rectangle
            (
                screenX,
                screenY,
                screenSize,
                screenSize
            );

            return renderRect;
        }

        public static Point ScreenToTile(Point screenLocation)
        {
            Point p = ScreenToWorld(screenLocation);
            p.X /= World.TILESIZE;
            p.Y /= World.TILESIZE;
            return p;
        }

        public static Point ScreenToWorld(Point screenLocation)
        {
            int x = (int)((screenLocation.X - Main.screenCenter.X) / Main.renderScale + Program.GetGame().localCamera.position.X);
            int y = (int)((screenLocation.Y - Main.screenCenter.Y) / Main.renderScale + Program.GetGame().localCamera.position.Y);
            return new Point(x, y);
        }

        public static Rectangle WorldToScreen(Rectangle worldRect)
        {
            // Apply camera offset first
            float screenX = worldRect.X - Program.GetGame().localCamera.position.X;
            float screenY = worldRect.Y - Program.GetGame().localCamera.position.Y;

            // Apply scale
            screenX *= Main.renderScale;
            screenY *= Main.renderScale;

            // Add screen center after scaling
            screenX += Main.screenCenter.X;
            screenY += Main.screenCenter.Y;

            // Scale width and height
            int scaledW = (int)(worldRect.Width * Main.renderScale);
            int scaledH = (int)(worldRect.Height * Main.renderScale);

            return new Rectangle((int)screenX, (int)screenY, scaledW, scaledH);
        }
    }
}
