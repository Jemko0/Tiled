using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tiled.UI
{
    /// <summary>
    /// Lays children out horizontally from left to right
    /// </summary>
    public class WHorizontalBox : PanelWidget
    {
        public int innerPadding = 5;
        public Vector2 childrenAnchorOffset = new Vector2 (0, 0);
        public WHorizontalBox(HUD owner) : base(owner)
        {
        }

        public override void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            if (childIdx >= maxChildIndex)
                return;

            children[childIdx].anchorPosition = childrenAnchorOffset;
            children[childIdx].SetOffset(new Vector2(0, 0));

            if (childIdx != 0)
            {
                children[childIdx].SetOffset(new Vector2((children[childIdx - 1].GetSize().X * childIdx) + innerPadding * childIdx, 0));
            }

            children[childIdx].ScaleGeometry();
            children[childIdx].Draw(ref sb);
        }
    }
}
