using Microsoft.Xna.Framework;
using System;
using Tiled.DataStructures;

namespace Tiled.Gameplay.Items.ItemBehaviours
{
    public class AxeBehaviour : IItemBehaviour
    {
        public void Use(EItem item)
        {
        }

        public void UseOnTile(EItem item, int x, int y)
        {
            World.BreakTile(x, y, 0, item.Item.axePower);
        }

        public void UseWithEntity(EItem item, object entity)
        {
        }

        public bool CanConsume(EItem item, Point tile)
        {
            return true;
        }
    }
}
