using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.Gameplay.Items.ItemBehaviours;
using Tiled.Gameplay.Items;

namespace Tiled.Gameplay.Items.ItemBehaviours
{
    public class PickaxeBehaviour : IItemBehaviour
    {
        public void Use(EItem item)
        {
        }

        public void UseOnTile(EItem item, int x, int y)
        {
            throw new NotImplementedException();
        }

        public void UseWithEntity(EItem item, object entity)
        {
        }
    }
}
