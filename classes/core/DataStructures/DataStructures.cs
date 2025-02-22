using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Tiled.Gameplay.Items;
using Tiled.UI;

namespace Tiled.DataStructures
{
    public struct Tile
    {
        public bool render;
        public Texture2D sprite;

        /// <summary>
        /// if false, will ignore tile frames, and also will be ignored by other tiles
        /// </summary>
        public bool useFrames;
 
        public bool collision;

        /// <summary>
        /// determines if this tile only hangs on walls, like torches
        /// </summary>
        public bool hangingOnWalls;

        /// <summary>
        /// strength of light that the tile emits by itself
        /// </summary>
        public uint light;

        /// <summary>
        /// used for neighbor checking in tileFrames
        /// </summary>
        public TileNeighbors ignoreNeighbors;
        public int frameSize;
        public int framePadding;

        /// <summary>
        /// how much light does this tile block? (default = 3)
        /// </summary>
        public uint blockLight;

        public sbyte hardness;
        public EItemType itemDrop;
        public sbyte minPick;
        public sbyte minAxe;
    }

    public struct Item
    {
        public string name;
        public Texture2D sprite;
        public Vector2 size;
        public ushort maxStack;
        public float useTime;
        public bool consumable;
        public sbyte pickaxePower;
        public sbyte axePower;
        public bool autoReuse;
        public ETileType placeTile;
        public EProjectileType projectile;
        public Type behaviourType;
        public EItemSwingAnimationType swingAnimationType;
    }
    
    public struct Projectile
    {
        public string name;
        public Texture2D sprite;
        public Vector2 size;
        public Vector2 initVelocity;
    }

    public enum EProjectileType
    {
        None = 0,
        Bullet,
        Bomb,
    }

    public struct ContainerItem
    {
        public EItemType type;
        public ushort stack;
        public ContainerItem()
        {
            type = EItemType.None;
            stack = 0;
        }

        public ContainerItem(EItemType type, ushort stack)
        {
            this.type = type;
            this.stack = stack;
        }
        public static ContainerItem empty => new ContainerItem();
    }

    public enum EItemType
    {
        None,
        Base,
        BasePickaxe,
        DirtBlock,
        Torch,
        Bomb,
    }

    public enum EItemSwingAnimationType
    {
        None,
        Swing,
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

    public struct EntityDef
    {
        public string name;
        public Texture2D sprite;
        public Vector2 size;
    }

    public enum ETileType
    {
        Air = 0,
        Dirt,
        Grass,
        Stone,
        Plank,
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

    public enum EEntityType
    {
        None = 0,
        Entity,
        Player,
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

    public delegate void ActionMappingPress(ActionMappingArgs e);
    public delegate void ActionMappingRelease(ActionMappingArgs e);

    public class ActionMappingArgs
    {
        public Keys key;

        public ActionMappingArgs(Keys key)
        {
            this.key = key;
        }
    }

    public class ActionMapping
    {
        public Keys keyboardKey;
        public event ActionMappingPress onActionMappingPressed;
        public event ActionMappingRelease onActionMappingReleased;
        public ActionMapping(Keys key)
        {
            keyboardKey = key;
        }

        public void InvokePressed(ActionMappingArgs args)
        {
            if(onActionMappingPressed == null)
            {
                return;
            }
            onActionMappingPressed.Invoke(args);
        }

        public void InvokeReleased(ActionMappingArgs args)
        {
            if(onActionMappingReleased == null)
            {
                return;
            }
            onActionMappingReleased.Invoke(args);
        }
    }

    public class ItemSwingArgs
    {
        public EItemType type;
        public ItemSwingArgs(EItemType type)
        {
            this.type = type;
        }
    }

    public enum ENetMode
    {
        Standalone,
        Server,
        Client,
    }
}
