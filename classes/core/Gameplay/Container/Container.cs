using System;
using System.Linq;
using Tiled.DataStructures;
using Tiled.ID;

namespace Tiled.Gameplay.Container
{
    public class Container
    {
        public int containerSize = 49;
        public ContainerItem[] items;

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

            int foundSlot = FindItem(item.type);

            if(foundSlot != -1)
            {
                ushort totalStack = items[foundSlot].stack = item.stack;
                ushort itemMaxStack = ItemID.GetItem(items[foundSlot].type).maxStack;
                if (totalStack <= itemMaxStack)
                {
                    items[foundSlot].stack += item.stack;
                }
                else
                {
                    //overflow
                    int slotsNeeded = (int)Math.Ceiling(item.stack / (float)containerSize);
                    int[] overflowSlots = new int[slotsNeeded];
                    for(int i = 0; i < slotsNeeded; i++)
                    {
                        overflowSlots[i] = FindFreeSlot(overflowSlots);
                        overflowSlots[i] = i;
                    }

                    for(int i = 0; i < overflowSlots.Length; i++)
                    {
                        if(overflowSlots[i] != overflowSlots.Length - 1)
                        {
                            items[overflowSlots[i]] = item;
                        }
                        else
                        {
                            items[overflowSlots[i]].type = item.type;
                            items[overflowSlots[i]].stack = (ushort)(item.stack % itemMaxStack);
                        }
                    }
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
            int foundSlot = FindItem(type);

            if(foundSlot != -1)
            {
                ushort removeStack = (ushort)(items[foundSlot].stack - amount);

                if(removeStack < 1)
                {
                    ClearSlot(foundSlot);
                }
            }

            return true;
        }

        public void ClearSlot(int index)
        {
            items[index] = ContainerItem.empty;
        }

        public int FindItem(EItemType type)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if(items[i].type == type)
                {
                    return i;
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

        public int FindFreeSlot(int[]? excludeIndices)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (excludeIndices.Contains(i))
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
    }
}
