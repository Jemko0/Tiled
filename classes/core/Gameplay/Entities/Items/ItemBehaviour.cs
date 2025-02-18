using System;
using Tiled.DataStructures;
using Tiled.ID;

namespace Tiled.Gameplay.Items.ItemBehaviours
{
    public interface IItemBehaviour
    {
        void Use(EItem item);
        void UseOnTile(EItem item, int x, int y);
        void UseWithEntity(EItem item, object entity);
    }

    public class DefaultItemBehaviour : IItemBehaviour
    {
        public void Use(EItem item) { }
        public void UseOnTile(EItem item, int x, int y) { }
        public void UseWithEntity(EItem item, object entity) { }
    }

    public static class ItemBehaviorFactory
    {
        public static IItemBehaviour CreateBehavior(EItemType type)
        {
            var template = ItemID.GetItem(type);
            if (template.behaviourType != null)
            {
                return (IItemBehaviour)Activator.CreateInstance(template.behaviourType);
            }
            return new DefaultItemBehaviour();
        }
    }
}
