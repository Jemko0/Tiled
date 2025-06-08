using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.Gameplay.Items;
using Tiled.Networking.Shared;

namespace Tiled.Gameplay
{
    public static class Statics
    {
        public static bool IsSubclassOrInstanceOf (Type type, Type tSubclass)
        {
            return type.IsSubclassOf(tSubclass) || type.IsInstanceOfType(tSubclass);
        }
    }
}
