using System;
using Tiled.DataStructures;

namespace Tiled.Gameplay.Items.ItemBehaviours
{
    public class PlaceTileBehaviour : IItemBehaviour
    {
        public void Use(EItem item)
        {
        }

        public void UseOnTile(EItem item, int x, int y)
        {
            if(!World.HasDirectNeighbors(x, y))
            {
                World.SetTile(x, y, item.Item.placeTile);
            }
        }

        public void UseWithEntity(EItem item, object entity)
        {
        }
    }
}
