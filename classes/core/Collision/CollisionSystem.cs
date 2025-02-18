using Microsoft.Xna.Framework;
using System;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.ID;

namespace Tiled.Collision
{
    public class CollisionComponent
    {
        private Entity entity;
        private const float skinWidth = 0.1f;

        public delegate void Hit();
        public event Hit onHit;

        public CollisionComponent(Entity entity)
        {
            SetEntity(entity);
        }

        public void SetEntity(Entity entity)
        {
            this.entity = entity;
        }

        public void Move()
        {
            if (!entity.canCollide)
            {
                entity.position += entity.velocity;
                return;
            }

            float moveX = entity.velocity.X;
            float moveY = entity.velocity.Y;
            MoveWithCollision(ref moveX, ref moveY);
        }

        private void MoveWithCollision(ref float moveX, ref float moveY)
        {
            // Handle X movement first
            if (moveX != 0)
            {
                int direction = Math.Sign(moveX);
                float rayLength = Math.Abs(moveX);

                var rect = entity.GetRectF();
                Vector2[] rayStarts = new Vector2[]
                {
                new Vector2(direction == 1 ? rect.Right : rect.Left, rect.Top + skinWidth),
                new Vector2(direction == 1 ? rect.Right : rect.Left, rect.Top + rect.Height * 0.5f),
                new Vector2(direction == 1 ? rect.Right : rect.Left, rect.Bottom - skinWidth)
                };

                float shortestHit = rayLength;
                bool collision = false;

                foreach (var rayStart in rayStarts)
                {
                    if (CastRay(rayStart, new Vector2(direction, 0), rayLength, out float hitDistance))
                    {
                        shortestHit = Math.Min(shortestHit, hitDistance);
                        collision = true;
                    }
                }

                if (collision)
                {
                    moveX = direction * (Math.Max(0, shortestHit - skinWidth));
                    onHit?.Invoke();
                    entity.velocity.X = 0;
                }
            }

            entity.position.X += moveX;

            // Handle Y movement
            if (moveY != 0)
            {
                int direction = Math.Sign(moveY);
                float rayLength = Math.Abs(moveY);

                var rect = entity.GetRectF();
                Vector2[] rayStarts = new Vector2[]
                {
                new Vector2(rect.Left + skinWidth, direction == 1 ? rect.Bottom : rect.Top),
                new Vector2(rect.Left + rect.Width * 0.5f, direction == 1 ? rect.Bottom : rect.Top),
                new Vector2(rect.Right - skinWidth, direction == 1 ? rect.Bottom : rect.Top)
                };

                float shortestHit = rayLength;
                bool collision = false;

                foreach (var rayStart in rayStarts)
                {
                    if (CastRay(rayStart, new Vector2(0, direction), rayLength, out float hitDistance))
                    {
                        shortestHit = Math.Min(shortestHit, hitDistance);
                        collision = true;
                    }
                }

                if (collision)
                {
                    moveY = direction * (Math.Max(0, shortestHit - skinWidth));
                    entity.velocity.Y = 0;
                }
            }

            entity.position.Y += moveY;
        }

        public Entity? GetCollidingEntity()
        {
            for (int i = 0; i < Main.entities.Count; i++)
            {
                if (entity.GetRectF().IntersectsWith(Main.entities[i].GetRectF()))
                {
                    return Main.entities[i];
                }
            }
            return null;
        }

        private bool CastRay(Vector2 start, Vector2 direction, float length, out float hitDistance)
        {
            hitDistance = length;
            Vector2 end = start + direction * length;

            // Convert world coordinates to tile indices
            int startTileX = (int)Math.Floor(start.X / World.TILESIZE);
            int startTileY = (int)Math.Floor(start.Y / World.TILESIZE);
            int endTileX = (int)Math.Floor(end.X / World.TILESIZE);
            int endTileY = (int)Math.Floor(end.Y / World.TILESIZE);

            // Check each tile along the ray
            int minX = Math.Min(startTileX, endTileX);
            int maxX = Math.Max(startTileX, endTileX);
            int minY = Math.Min(startTileY, endTileY);
            int maxY = Math.Max(startTileY, endTileY);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (World.IsValidIndex(World.tiles, x, y))
                    {
                        ETileType tileType = World.tiles[x, y];
                        Tile data = TileID.GetTile(tileType);
                        if (tileType != ETileType.Air && data.collision)
                        {
                            Rectangle tileRect = new Rectangle(
                                x * World.TILESIZE,
                                y * World.TILESIZE,
                                World.TILESIZE,
                                World.TILESIZE);

                            if (RayIntersectsRect(start, direction, tileRect, out float distance))
                            {
                                if (distance < hitDistance)
                                {
                                    hitDistance = distance;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool RayIntersectsRect(Vector2 origin, Vector2 direction, Rectangle rect, out float distance)
        {
            distance = 0f;
            Vector2 min = new Vector2(rect.Left, rect.Top);
            Vector2 max = new Vector2(rect.Right, rect.Bottom);

            float tmin = float.NegativeInfinity;
            float tmax = float.PositiveInfinity;

            // Check X axis
            if (Math.Abs(direction.X) < float.Epsilon)
            {
                if (origin.X < min.X || origin.X > max.X)
                    return false;
            }
            else
            {
                float invD = 1f / direction.X;
                float t1 = (min.X - origin.X) * invD;
                float t2 = (max.X - origin.X) * invD;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                tmin = Math.Max(tmin, t1);
                tmax = Math.Min(tmax, t2);

                if (tmin > tmax)
                    return false;
            }

            // Check Y axis
            if (Math.Abs(direction.Y) < float.Epsilon)
            {
                if (origin.Y < min.Y || origin.Y > max.Y)
                    return false;
            }
            else
            {
                float invD = 1f / direction.Y;
                float t1 = (min.Y - origin.Y) * invD;
                float t2 = (max.Y - origin.Y) * invD;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                tmin = Math.Max(tmin, t1);
                tmax = Math.Min(tmax, t2);

                if (tmin > tmax)
                    return false;
            }

            distance = tmin;
            return true;
        }

        public bool IsOnGround()
        {
            var rect = entity.GetRectF();
            Vector2[] rayStarts = new Vector2[]
            {
            new Vector2(rect.Left + skinWidth, rect.Bottom),
            new Vector2(rect.Left + rect.Width * 0.5f, rect.Bottom),
            new Vector2(rect.Right - skinWidth, rect.Bottom)
            };

            float groundCheckDistance = 2f;

            foreach (var rayStart in rayStarts)
            {
                if (CastRay(rayStart, Vector2.UnitY, groundCheckDistance, out float hitDistance))
                {
                    return true;
                }
            }

            return false;
        }
    }
}