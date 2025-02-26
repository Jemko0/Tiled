using Microsoft.Xna.Framework.Input;
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
            actionMappings.Add("inv_open", new ActionMapping(Keys.I));
        }

        public static bool IsMappingHeld(string mapping)
        {
            return Keyboard.GetState().GetPressedKeys().Contains(actionMappings[mapping].keyboardKey);
        }
    }
}
