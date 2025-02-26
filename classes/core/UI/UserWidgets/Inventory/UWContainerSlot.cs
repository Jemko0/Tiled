using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tiled.ID;
using Tiled.Inventory;

namespace Tiled.UI.UserWidgets
{
    public class UWContainerSlot : PanelWidget
    {
        private Texture2D slotBg;
        private WText amtText;
        public int slotID;
        public Container container;
        public UWContainerSlot(HUD owner) : base(owner)
        {
            slotBg = Program.GetGame().Content.Load<Texture2D>("UI/inventory/inventorySlot");
            
        }

        public override void Construct()
        {
            amtText = HUD.CreateWidget<WText>(owningHUD);
            amtText.text = "-1";
            amtText.SetGeometry(new Vector2(32, 16), DataStructures.AnchorPosition.BottomRight);
            amtText.AttachToParent(this, DataStructures.AnchorPosition.BottomRight);
            amtText.layerDepth = 0.8f;
            amtText.justification = DataStructures.ETextJustification.Center;
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            sb.Draw(slotBg, scaledGeometry, null, Program.GetGame().GetLocalPlayer().selectedSlot == slotID? Color.LightSkyBlue : Color.MediumSlateBlue, 0, new(), SpriteEffects.None, 1.0f);

            if(slotID > container.items.Length - 1)
            {
                return;
            }

            amtText.text = container.items[slotID].stack > 0 ? container.items[slotID].stack.ToString() : "";

            if (container.items[slotID].type == DataStructures.EItemType.None)
            {
                return;
            }

            Rectangle padded = scaledGeometry;
            padded.Inflate(-16, -16);

            Texture2D itemIcon = ItemID.GetItem(container.items[slotID].type).sprite;
            sb.Draw(itemIcon, padded, null, Color.White, 0, new(), SpriteEffects.None, 0.9f);

            //base.DrawWidget(ref sb);
        }
    }
}
