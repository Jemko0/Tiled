using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tiled.Inventory;

namespace Tiled.UI.UserWidgets
{
    public class UWContainerWidget : PanelWidget
    {
        public Container container;
        public WHorizontalBox horizontalBox;
        public UWContainerWidget(HUD owner) : base(owner)
        {
        }

        public override void Construct()
        {
            horizontalBox = HUD.CreateWidget<WHorizontalBox>(owningHUD);
            horizontalBox.AttachToParent(this);
        }

        public void SetContainer(ref Container container)
        {
            this.container = container;
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

            for (int i = 0; i < 5; i++)
            {
                UWContainerSlot currentChild = HUD.CreateWidget<UWContainerSlot>(owningHUD);
                currentChild.SetGeometry(new Vector2(64, 64), DataStructures.AnchorPosition.TopLeft);
                currentChild.slotID = i;
                currentChild.container = container;
                currentChild.AttachToParent(horizontalBox);
            }
        }

        public void UpdateChildren(ref Container refcontainer)
        {
            container = refcontainer;
            foreach(var c in horizontalBox.GetChildren())
            {
                ((UWContainerSlot)c).container = refcontainer;
            }
        }

        public override void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            base.DrawChild(ref sb, childIdx);
        }
    }
}
