using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Net.Mime;
using Tiled.Events;
using Tiled.Collision;
using Tiled.DataStructures;
using Tiled.Gameplay.Components;
using Tiled.ID;
using Tiled.Interfaces;

namespace Tiled.Gameplay
{
    public class Entity : IDisposable, IDamageable
    {
        public Vector2 position;
        public Vector2 size;
        public Vector2 velocity;
        public bool canCollide = true;
        public Texture2D entitySprite;
        public CollisionComponent collision;
        protected int frameSlotSizeX = 32;
        protected int frameSlotSizeY = 48;
        public float rotation = 0.0f;
        public Vector2 rotOrigin = new(0, 0);
        public int netID = -1;
        public EEntityType entityType;
        public bool centerSprite = false;
        public HealthComponent healthComponent;
        public bool facingLeft;
        public int direction;
        public uint light = 0;
        public Entity()
        {
            RegisterCollisionComponent();
            RegisterHealthComponent();
        }

        public void RegisterCollisionComponent()
        {
            collision = new CollisionComponent(this);
        }

        public void RegisterHealthComponent()
        {
            healthComponent = new HealthComponent(100, 100, 0);
        }

        public static T NewEntity<T>(params object?[]? args) where T : Entity
        {
            T newEntity = (T)Activator.CreateInstance(typeof(T), args);
            newEntity.Initialize(EEntityType.None);
            Main.RegisterEntity(newEntity);
            newEntity.Begin();
            return newEntity;
        }

        public void Destroy()
        {
            if (Main.netMode == ENetMode.Standalone)
            {
                LocalDestroy();
                return;
            }

            if(netID == -1)
            {
                Debug.WriteLine("netID was -1, assuming this is a Locally spawned Entity");
                LocalDestroy();
                return;
            }

            
#if TILEDSERVER
            Main.netServer.ServerDestroyEntity(netID);
#else
            Main.netClient.ClientRequestDestroyEntity(netID);
#endif
        }

        public virtual void Destroyed()
        {

        }


        public void LocalDestroy()
        {
            Main.UnregisterEntity(this);
            Destroyed();
            Dispose();
        }

        /// <summary>
        /// use this to setup action mappings etc...
        /// </summary>
        /// <param name="playerController"></param>
        public virtual void Possessed(Controller playerController)
        {
            return;
        }

        /// <summary>
        /// handles sprite and hitbox, ONLY OVERRIDE IF YOU KNOW WHAT YOU ARE DOING
        /// </summary>
        /// <param name="type"></param>
        public virtual void Initialize(EEntityType type)
        {
            EntityDef e = EntityID.GetEntityInfo(type);
            size = e.size;
            entitySprite = e.sprite;
            entityType = type;
        }

        public virtual System.Drawing.RectangleF GetRectF()
        {
            return new System.Drawing.RectangleF(position.X, position.Y, size.X, size.Y);
        }

        public virtual Rectangle GetRect()
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        }

        public virtual void Begin()
        {

        }

        /// <summary>
        /// default update handles facing left/right
        /// </summary>
        public virtual void Update()
        {
            if(velocity.X != 0.0f)
            {
                facingLeft = velocity.X < 0.0f;
                direction = facingLeft ? -1 : 1;
            }

            if(light != 0)
            {
                Lighting.QueueLightUpdate((int)(position.X / World.TILESIZE), (int)(position.Y / World.TILESIZE));
            }
        }

        public virtual Rectangle? GetFrame()
        {
            return null;
        }

        public virtual void MovementUpdate()
        {
            if(collision == null)
            {
                return;
            }
            if (canCollide)
            {
                collision.Move();
            }
            else
            {
                position += velocity;
            }
        }

        public virtual void Draw(ref SpriteBatch sb)
        {
            Color finalColor = Color.White;

            if(!World.IsValidIndex(World.lightMap, (int)(position.X / World.TILESIZE), (int)(position.Y / World.TILESIZE)))
            {
                finalColor = Color.Black;
                goto draw;
            }

            finalColor.A = 255;

        draw:
            SpriteEffects flip = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Rectangle drawRect = GetRect();
            if(centerSprite)
            {
                drawRect.Location = drawRect.Center;
            }
            sb.Draw(entitySprite, Rendering.WorldToScreen(drawRect), GetFrame(), finalColor, rotation, rotOrigin, flip, 0u);
            //sb.Draw(Program.GetGame().Content.Load<Texture2D>("Entities/debug"), Rendering.WorldToScreen(GetRect()), null, Color.White, 0.0f, new(0,0), SpriteEffects.None, 1.0f);
        }

        public virtual void Dispose()
        {
            position = Vector2.Zero;
            velocity = Vector2.Zero;
            //entitySprite.Dispose();
            EventHelper.RemoveAllEventHandlers(this);
            collision = null;
        }

        public virtual void ApplyDamage(uint damage, int fromNetID)
        {
            return;
        }
    }
}