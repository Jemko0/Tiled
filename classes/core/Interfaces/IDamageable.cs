using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled.Interfaces
{
    public interface IDamageable
    {
        public abstract void ApplyDamage(uint damage, int fromNetID);
    }
}
