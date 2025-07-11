﻿using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;

namespace Tiled.Input
{
    public class Mappings
    {
        public static Dictionary<string, ActionMapping> actionMappings = new Dictionary<string, ActionMapping>();
        public static Collection<Keys> invokePressedArray;
        public static Collection<Keys> invokeReleasedArray;
        public static void Update()
        {
            foreach (var mapping in actionMappings.Values)
            {
                if(Keyboard.GetState().GetPressedKeys().Contains(mapping.keyboardKey) && !invokePressedArray.Contains(mapping.keyboardKey))
                {
                    invokePressedArray.Add(mapping.keyboardKey);
                    invokeReleasedArray.Remove(mapping.keyboardKey);

                    mapping.InvokePressed(new ActionMappingArgs(mapping.keyboardKey));
                }
                if(!Keyboard.GetState().GetPressedKeys().Contains(mapping.keyboardKey) && !invokeReleasedArray.Contains(mapping.keyboardKey))
                {
                    invokePressedArray.Remove(mapping.keyboardKey);
                    invokeReleasedArray.Add(mapping.keyboardKey);

                    mapping.InvokeReleased(new ActionMappingArgs(mapping.keyboardKey));
                }
            }
        }

        public static void InitializeMappings()
        {
            invokePressedArray = new Collection<Keys>();
            invokeReleasedArray = new Collection<Keys>();

            actionMappings.Add("move_left", new ActionMapping(Keys.A));
            actionMappings.Add("move_right", new ActionMapping(Keys.D));
            actionMappings.Add("move_jump", new ActionMapping(Keys.Space));
            actionMappings.Add("inv_1", new ActionMapping(Keys.D1));
            actionMappings.Add("inv_2", new ActionMapping(Keys.D2));
            actionMappings.Add("inv_3", new ActionMapping(Keys.D3));
            actionMappings.Add("inv_4", new ActionMapping(Keys.D4));
            actionMappings.Add("inv_5", new ActionMapping(Keys.D5));
            actionMappings.Add("inv_open", new ActionMapping(Keys.E));
            actionMappings.Add("esc_menu", new ActionMapping(Keys.Escape));
            actionMappings.Add("dbg_selfdmg", new ActionMapping(Keys.P));
            actionMappings.Add("time_fwd", new ActionMapping(Keys.OemPeriod));
            actionMappings.Add("time_bwd", new ActionMapping(Keys.OemComma));
            actionMappings.Add("dbg_move", new ActionMapping(Keys.C));
        }

        /// <summary>
        /// replaces a mapping that already exists, does not check for non existing ones so it will crash when u use an invalid mapping name
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="newKey"></param>
        public static void RebindMapping(string mapping, Keys newKey)
        {
            actionMappings[mapping].keyboardKey = newKey;
        }


        /// <summary>
        /// loads mappings from fil
        /// </summary>
        /// <param name="file"></param>
        public static void LoadMappings(string file)
        {
            throw new NotImplementedException("lolol");
        }

        public static bool IsMappingHeld(string mapping)
        {
            return Keyboard.GetState().GetPressedKeys().Contains(actionMappings[mapping].keyboardKey);
        }
    }
}
