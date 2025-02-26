using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tiled.UI
{
    /// <summary>
    /// Lays children out vertically from top to bottom
    /// </summary>
    public class WVerticalBox : PanelWidget
    {
        public int innerPadding = 5;
        public bool childrenKeepWidth = false;
        public WVerticalBox(HUD owner) : base(owner)
        {
        }

        public override void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            if (childIdx >= maxChildIndex)
                return;

            Vector2 size = childrenKeepWidth ? children[childIdx].GetSize() : new Vector2(GetSize().X, children[childIdx].GetSize().Y);
            children[childIdx].SetGeometry(size, DataStructures.AnchorPosition.Center);

            if (childIdx != 0)
            {
                children[childIdx].SetOffset(new Vector2(0, (children[childIdx - 1].GetSize().Y * childIdx) + innerPadding * childIdx));
            }
            
            children[childIdx].ScaleGeometry();
            children[childIdx].Draw(ref sb);
        }
    }
}
