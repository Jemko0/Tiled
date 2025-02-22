using Microsoft.Xna.Framework.Input;
using System;
using Tiled.DataStructures;

namespace Tiled.Gameplay.Items.ItemBehaviours
{
    public class PlaceTileBehaviour : IItemBehaviour
    {
        public bool CanConsume(EItem item)
        {
            var t = Rendering.ScreenToTile(Mouse.GetState().Position);
            return World.IsValidForTilePlacement(t.X, t.Y);
        }

        public void Use(EItem item)
        {
        }

        public void UseOnTile(EItem item, int x, int y)
        {
            if(World.IsValidForTilePlacement(x, y))
            {
                World.SetTile(x, y, item.Item.placeTile, false);
            }
        }

        public void UseWithEntity(EItem item, object entity)
        {
        }
    }
}
