﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;

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
                    break;

                case ETileType.Grass:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/grass");
                    break;

                case ETileType.Stone:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/stone");
                    break;

                case ETileType.Plank:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/plank");
                    break;

                case ETileType.Torch:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/torch");
                    t.hangingOnWalls = true;
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
        public static Item GetItem(EItemType type)
        {
            Item i = new Item();
            i.name = "Default Item";
            i.size = new Vector2(24, 24);
            i.sprite = Program.GetGame().Content.Load<Texture2D>("Entities/Item/item");
            i.pickaxePower = 0.0f;
            i.axePower = 0.0f;
            i.maxStack = 99;
            i.swingAnimationType = EItemSwingAnimationType.None;

            switch(type)
            {
                case EItemType.Base:
                    i.name = "Base";
                    i.size = new Vector2(16, 16);
                    break;

                case EItemType.BasePickaxe:
                    i.name = "Base Pickaxe";
                    i.size = new Vector2(16, 16);
                    break;
            }

            return i;
        }
    }
}
