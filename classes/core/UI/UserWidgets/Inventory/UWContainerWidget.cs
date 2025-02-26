using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tiled.Inventory;

namespace Tiled.UI.UserWidgets
{
    public class UWContainerWidget : PanelWidget
    {
        public Container container;
        public WWrapBox wrapBox;

        public bool renderFull = true;
        public UWContainerWidget(HUD owner) : base(owner)
        {
        }

        public override void Construct()
        {
            wrapBox = HUD.CreateWidget<WWrapBox>(owningHUD);
            wrapBox.AttachToParent(this);
        }

        public void SetContainer(ref Container container)
        {
            this.container = container;
        }

        public void SetOpenInv(bool open)
        {
            renderFull = open;
            wrapBox.maxChildIndex = open ? 512 : 5;
        }

        public void UpdateSlots()
        {
            /*
            if(children != null && children.Count > 0)
            {
                foreach (Widget c in children)
                {
                    c.DestroyWidget();
                }
            }
            */

            for (int i = 0; i < container.items.Length; i++)
            {
                UWContainerSlot currentChild = HUD.CreateWidget<UWContainerSlot>(owningHUD);
                currentChild.SetGeometry(new Vector2(64, 64), DataStructures.AnchorPosition.TopLeft);
                currentChild.slotID = i;
                currentChild.container = container;
                currentChild.AttachToParent(wrapBox);
            }
        }

        public void UpdateChildren(ref Container refcontainer)
        {
            container = refcontainer;
            foreach(var c in wrapBox.GetChildren())
            {
                ((UWContainerSlot)c).container = refcontainer;
            }
        }
    }
}
