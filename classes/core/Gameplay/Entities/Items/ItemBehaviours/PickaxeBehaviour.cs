using System;
using Tiled.DataStructures;

namespace Tiled.Gameplay.Items.ItemBehaviours
{
    public class PickaxeBehaviour : IItemBehaviour
    {
        public void Use(EItem item)
        {
        }

        public void UseOnTile(EItem item, int x, int y)
        {
            World.BreakTile(x, y, item.Item.pickaxePower, 0);
        }

        public void UseWithEntity(EItem item, object entity)
        {
        }

        public bool CanConsume(EItem item)
        {
            return true;
        }
    }
}
