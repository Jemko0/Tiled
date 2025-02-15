﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Tiled.UI;

namespace Tiled.DataStructures
{
    public struct Tile
    {
        public bool render;
        public Texture2D sprite;
        public bool useFrames;
        public uint light;
        public TileNeighbors ignoreNeighbors;
        public int frameSize;
        public int framePadding;
        public uint blockLight;
    }

    public struct TileNeighbors
    {
        public bool R;
        public bool L;
        public bool T;
        public bool B;

        public TileNeighbors()
        {
            R = false;
            L = false;
            T = false;
            B = false;
        }

        public TileNeighbors(int right = 0, int left = 0, int top = 0, int bottom = 0)
        {
            R = right > 0;
            L = left > 0;
            T = top > 0;
            B = bottom > 0;
        }
    }

    public struct Wall
    {
        public bool render;
        public Texture2D sprite;
        public bool useFrames;
        public int frameSize;
        public int framePadding;
    }

    public enum ETileType
    {
        Air = 0,
        Dirt,
        Torch,
    }

    public enum EWallType
    {
        Air = 0,
        Dirt,
    }

    public enum MouseButtonState
    {
        Left,
        Middle,
        Right,
    }

    public class WidgetDestroyArgs
    {
        public Widget destroyedWidget;
        public WidgetDestroyArgs(Widget w)
        {
            destroyedWidget = w;
        }
    }

    public enum AnchorPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum ETextJustification
    {
        Left,
        Center,
        Right,
    }

    public class ButtonPressArgs
    {
        public Point mouseLocation;
        public MouseButtonState buttonState;

        public ButtonPressArgs(Point ml, MouseButtonState mbtnst)
        {
            mouseLocation = ml;
            buttonState = mbtnst;
        }
    }
}
