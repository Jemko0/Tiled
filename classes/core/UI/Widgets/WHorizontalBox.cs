using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tiled.DataStructures;

namespace Tiled.UI
{
    /// <summary>
    /// Lays children out horizontally from left to right
    /// </summary>
    public class WHorizontalBox : PanelWidget
    {
        public int innerPadding = 5;
        public bool childrenKeepWidth = true;
        public Vector2 childrenAnchorOffset = new Vector2 (0, 0);
        public WHorizontalBox(HUD owner) : base(owner)
        {
        }

        public override void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            if (childIdx >= maxChildIndex)
                return;

            Widget child = children[childIdx];

            child.anchorPosition = childrenAnchorOffset;

            if (!childrenKeepWidth)
            {
                child.SetGeometry(new Vector2(GetSize().X / children.Count, GetSize().Y), AnchorPosition.TopLeft);
            }

            if (childIdx != 0)
            {
                child.SetOffset(new Vector2((children[childIdx - 1].GetSize().X * childIdx) + innerPadding * childIdx, 0));
            }

            child.ScaleGeometry();
            child.Draw(ref sb);
        }
    }
}
