using System;
using System.Linq;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.Gameplay.Items;
using Tiled.ID;

namespace Tiled.Inventory
{
    public class Container
    {
        public int containerSize = 49;
        public ContainerItem[] items;
        public Entity? entityCarrier;
        public Container(int size)
        {
            containerSize = size;
            items = new ContainerItem[containerSize];
        }

        public bool Add(ContainerItem item)
        {
            if(IsContainerFull())
            {
                return false;
            }

            if(WillContainerBeFull(item))
            {
                int lastAvailableSlot = FindItem(item.type);
                
                if (lastAvailableSlot != -1)
                {
                    ushort itemMaxStack = ItemID.GetItem(items[lastAvailableSlot].type).maxStack;
                    ushort remainder = (ushort)((items[lastAvailableSlot].stack + item.stack) % itemMaxStack);
                    items[lastAvailableSlot].stack = itemMaxStack;

                    if (entityCarrier != null)
                    {
                        var i = EItem.CreateItem(item.type);
                        i.position = entityCarrier.position;
                        i.count = remainder;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }

            int foundSlot = FindItem(item.type);

            if(foundSlot != -1)
            {
                ushort totalStack = (ushort)(items[foundSlot].stack + item.stack);
                ushort itemMaxStack = ItemID.GetItem(items[foundSlot].type).maxStack;
                if (totalStack <= itemMaxStack)
                {
                    items[foundSlot].stack += item.stack;
                }
                else
                {
                    //overflow
                    int fullSlot = foundSlot;
                    items[fullSlot].stack = itemMaxStack;

                    int freeSlot = FindItem(item.type);
                    if(freeSlot == -1)
                    {
                        freeSlot = FindFreeSlot(fullSlot);
                    }

                    items[freeSlot].type = item.type;
                    items[freeSlot].stack += (ushort)(totalStack - itemMaxStack);
                }
            }
            else
            {
                int freeSlot = FindFreeSlot();
                //no need to check if container is full since its already been done at the start of the function
                items[freeSlot] = item;
            }

            return true;
        }

        public bool Remove(EItemType type, ushort amount)
        {
            int foundSlot = FindItemLessOrEqual(type);

            if(foundSlot != -1)
            {
                ushort removeStack = (ushort)(items[foundSlot].stack - amount);
                items[foundSlot].stack = removeStack;

                if(removeStack < 1)
                {
                    ClearSlot(foundSlot);
                }
            }

            return true;
        }

        public bool RemoveFromSlot(int slot, ushort amount)
        {
            int foundSlot = slot;

            if (foundSlot != -1)
            {
                ushort removeStack = (ushort)(items[foundSlot].stack - amount);
                items[foundSlot].stack = removeStack;

                if (removeStack < 1)
                {
                    ClearSlot(foundSlot);
                }
            }

            return true;
        }

        public void ClearSlot(int index)
        {
            items[index] = ContainerItem.empty;
            items[index].stack = 0;
        }

        public int FindItem(EItemType type)
        {
            for (int i = 0; i < items.Length; i++)
            {
                ushort max = ItemID.GetItem(items[i].type).maxStack;
                if (items[i].type == type)
                {
                    if(items[i].stack < max)
                    {
                        return i;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return -1;
        }

        public int FindItemLessOrEqual(EItemType type)
        {
            for (int i = 0; i < items.Length; i++)
            {
                ushort max = ItemID.GetItem(items[i].type).maxStack;
                if (items[i].type == type)
                {
                    if (items[i].stack <= max)
                    {
                        return i;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return -1;
        }

        public int FindFreeSlot(params int?[]? excludeIndices)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if(excludeIndices.Contains(i))
                { 
                    continue;
                }

                if (items[i].type == EItemType.None)
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindFreeSlot(int[]? excludeIndices, EItemType? type = null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (excludeIndices.Contains(i))
                {
                    continue;
                }
                
                if(type != null)
                {
                    if(items[i].type == type && items[i].stack <= ItemID.GetItem(items[i].type).maxStack)
                    {
                        return i;
                    }
                }

                if (items[i].type == EItemType.None)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool IsContainerFull()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if(items[i].stack <= ItemID.GetItem(items[i].type).maxStack)
                {
                    return false;
                }
            }
            return true;
        }

        public bool WillContainerBeFull(ContainerItem item)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if ((item.type == items[i].type || items[i].type == EItemType.None) && items[i].stack + item.stack <= ItemID.GetItem(items[i].type).maxStack)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
