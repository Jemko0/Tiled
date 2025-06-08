using System;
using System.Security.Cryptography.X509Certificates;
using Tiled.Interfaces;

namespace Tiled.Gameplay.Components
{
    public class DamageEventArgs
    {
        uint damage;
        int fromNetID;
        public DamageEventArgs(uint damage, int fromNetID)
        {
            this.damage = damage;
            this.fromNetID = fromNetID;
        }
    }
    public class HealthComponent : IDamageable
    {
        public uint health;
        public uint maxHealth;
        public uint defense;

        public delegate void Damaged(DamageEventArgs e);
        public event Damaged onDamageGet;

        public HealthComponent()
        {
        }

        public HealthComponent(uint health = 0, uint maxHealth = 100, uint defense = 0)
        {
            this.health = health;
            this.maxHealth = maxHealth;
            this.defense = defense;
        }

        public void AddHealth(int amt)
        {
            health = (uint)(health + amt);
        }

        public void ApplyDamage(uint damage, int fromNetID)
        {
            if(Main.netMode == DataStructures.ENetMode.Standalone)
            {
                DoDamage(damage, fromNetID);
                return;
            }
#if !TILEDSERVER
            if(Main.netMode == DataStructures.ENetMode.Client)
            {
                Main.netClient.SendDamage(damage, fromNetID);
                return;
            }
#endif

#if TILEDSERVER
            AddHealth((int)-CalcDamage(damage, defense));
            return;
#endif
        }


        /// <summary>
        /// actually performs the damage calc and sets hp
        /// </summary>
        public void DoDamage(uint dmg, int id)
        {
            AddHealth((int)-CalcDamage(dmg, defense));
            onDamageGet?.Invoke(new DamageEventArgs(dmg, id));
        }

        public static uint CalcDamage(uint rawDamage, uint defense)
        {
            return (uint)Math.Clamp((rawDamage - (defense * World.difficulty / 2.0f)), 1, uint.MaxValue);
        }
    }
}