using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Tiled.DataStructures;
using Tiled.Gameplay.Items.ItemBehaviours;

namespace Tiled.ID
{
    public class TileID
    {
        public static Dictionary<ETileType, Tile> cachedGet = new Dictionary<ETileType, Tile>();
        public static Tile GetTile(ETileType type)
        {
            if (cachedGet.ContainsKey(type))
            {
                return cachedGet[type];
            }

            Tile t = new();
            t.render = true;
            t.sprite = null;
            t.frameSize = 16;
            t.useFrames = true;
            t.framePadding = 2;
            t.ignoreNeighbors = new TileNeighbors(0, 0, 0, 0);
            t.light = 0;
            t.blockLight = 3;
            t.hardness = 10;
            t.minPick = 1;
            t.minAxe = 1;
            t.collision = true;
            
            switch(type)
            {
                case ETileType.Air:
                    t.render = false;
                    t.collision = false;
                    t.blockLight = 1;
                    break;

                case ETileType.Dirt:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/dirt");
                    t.hardness = 4;
                    t.itemDrop = EItemType.DirtBlock;
                    break;

                case ETileType.Grass:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/grass");
                    t.itemDrop = EItemType.DirtBlock;
                    break;

                case ETileType.Stone:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/stone");
                    t.hardness = 32;
                    break;

                case ETileType.Plank:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/plank");
                    break;

                case ETileType.Torch:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/torch");
                    t.hangingOnWalls = true;
                    t.itemDrop = EItemType.Torch;
                    t.hardness = 0;
                    t.blockLight = 0;
                    t.light = 32;
                    t.collision = false;
                    break;
            }

            cachedGet[type] = t;
            return t;
        }
    }

    public class WallID
    {
        public static Wall GetWall(EWallType type)
        {
            Wall w = new();
            w.render = true;
            w.sprite = null;
            w.useFrames = true;
            w.framePadding = 2;
            w.frameSize = 16;
            
            switch (type)
            {
                case EWallType.Air:
                    w.render = false;
                    break;

                case EWallType.Dirt:
                    w.render = true;
                    w.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/dirt");
                    break;
            }

            return w;
        }
    }

    public class EntityID
    {
        public static EntityDef GetEntityInfo(EEntityType type)
        {
            EntityDef entity = new EntityDef();
            entity.sprite = Program.GetGame().Content.Load<Texture2D>("Entities/EntityDefault");

            switch (type)
            {
                case EEntityType.None:
                    entity.name = "None";
                    entity.size = new Vector2(24.0f, 48.0f);
                    break;

                case EEntityType.Entity:
                    entity.name = "default_entity";
                    entity.size = new Vector2(20.0f, 40.0f);
                    break;

                case EEntityType.Player:
                    entity.name = "playername";
                    entity.size = new Vector2(20.0f, 40.0f);
                    break;
            }

            return entity;
        }
    }

    public class ItemID
    {
        public static Dictionary<EItemType, Item> cachedGet = new Dictionary<EItemType, Item>();
        public static Item GetItem(EItemType type)
        {
            if (cachedGet.ContainsKey(type))
            {
                return cachedGet[type];
            }

            Item i = new Item();
            i.name = "Default Item";
            i.size = new Vector2(24, 24);
            i.sprite = Program.GetGame().Content.Load<Texture2D>("Entities/Item/item");
            i.pickaxePower = 0;
            i.axePower = 0;
            i.maxStack = 99;
            i.swingAnimationType = EItemSwingAnimationType.Swing;
            i.autoReuse = true;
            i.useTime = 1.0f;
            i.behaviourType = typeof(DefaultItemBehaviour);  // Default behavior

            switch (type)
            {
                case EItemType.None:
                    i.name = "None";
                    i.size = new Vector2(16, 16);
                    break;
                case EItemType.Base:
                    i.name = "Base";
                    i.size = new Vector2(16, 16);
                    break;

                case EItemType.BasePickaxe:
                    i.name = "Base Pickaxe";
                    i.sprite = Program.GetGame().Content.Load<Texture2D>("Entities/Item/basePickaxe");
                    i.useTime = 0.8f;
                    i.maxStack = 1;
                    i.pickaxePower = 4;
                    i.size = new Vector2(24, 24);
                    i.behaviourType = typeof(PickaxeBehaviour);
                    break;

                case EItemType.DirtBlock:
                    i.name = "Dirt";
                    i.consumable = true;
                    i.sprite = Program.GetGame().Content.Load<Texture2D>("Entities/Item/dirtBlock");
                    i.useTime = 0.2f;
                    i.maxStack = 999;
                    i.size = new Vector2(16, 16);
                    i.behaviourType = typeof(PlaceTileBehaviour);
                    i.placeTile = ETileType.Dirt;
                    break;

                case EItemType.Torch:
                    i.name = "Torch";
                    i.consumable = true;
                    i.sprite = Program.GetGame().Content.Load<Texture2D>("Entities/Item/torch");
                    i.useTime = 0.2f;
                    i.maxStack = 99;
                    i.size = new Vector2(16, 16);
                    i.behaviourType = typeof(PlaceTileBehaviour);
                    i.placeTile = ETileType.Torch;
                    break;

                case EItemType.Bomb:
                    i.name = "Bomb";
                    i.consumable = true;
                    i.projectile = EProjectileType.Bomb;
                    i.useTime = 0.5f;
                    i.behaviourType = typeof(ProjectileThrowBehaviour);
                    break;

            }

            cachedGet[type] = i;
            return i;
        }
    }

    public class BreakTextureID
    {
        public static Rectangle? GetTextureFrame(sbyte tileHealth, sbyte hardness)
        {

            if(tileHealth == -128)
            {
                return null;
            }

            float breakage = (float)tileHealth / hardness;
            
            if(breakage < 0.25f)
            {
                return new Rectangle(186, 0, 62, 62);
            }

            if (breakage < 0.5f)
            {
                return new Rectangle(124, 0, 62, 62);
            }

            if (breakage < 0.75f)
            {
                return new Rectangle(62, 0, 62, 62);
            }

            if (breakage < 1f)
            {
                return new Rectangle(0, 0, 62, 62);
            }

            return null;
        }
    }

    public class ProjectileID
    {
        public static Projectile GetProjectile(EProjectileType type)
        {
            Projectile p = new Projectile();

            p.name = "projectile";
            p.sprite = Program.GetGame().Content.Load<Texture2D>("Entities/Projectile/BaseProjectile");
            p.initVelocity = new(5.0f, 5.0f);
            p.size = new(16, 16);

            switch(type)
            {
                case EProjectileType.Bullet:
                    p.name = "bullet";
                    p.size = new(16, 16);
                    break;

                case EProjectileType.Bomb:
                    p.size = new(48, 48);
                    break;
            }

            return p;
        }
    }
}
