using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.Gameplay;

namespace Tiled.Interfaces
{
    public interface IUsable
    {
        public void Use();

        public void Use(int? tileX = null, int? tileY = null, Entity? usingEntity = null);
    }
}
