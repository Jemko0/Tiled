using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Tiled.Events
{
    public static class EventHelper
    {
        /// <summary>
        /// Unbinds all event handlers from the specified event using reflection.
        /// </summary>
        /// <param name="instance">The object instance that contains the event. Use null for static events.</param>
        /// <param name="eventName">The name of the event to unbind handlers from.</param>
        /// <returns>True if the event was found and handlers were removed, false otherwise.</returns>
        public static bool UnbindAllEventHandlers(object instance, string eventName)
        {
            try
            {
                // Get the type of the object that contains the event
                Type type = instance != null ? instance.GetType() : null;
                
                // For static events, we need the type directly
                if (instance == null && type == null)
                {
                    throw new ArgumentException("For static events, provide the type as the instance parameter");
                }

                // Get the event field info
                EventInfo eventInfo = type.GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | 
                                                            BindingFlags.Instance | BindingFlags.Static);
                
                if (eventInfo == null)
                {
                    return false;
                }

                // Get the backing field for the event (events are represented by a private field)
                string backingFieldName = eventName;
                FieldInfo backingField = type.GetField(backingFieldName, BindingFlags.NonPublic | 
                                                                        BindingFlags.Instance | 
                                                                        BindingFlags.Static);
                
                if (backingField == null)
                {
                    // Try with the common naming pattern for event backing fields
                    backingFieldName = $"_{eventName}";
                    backingField = type.GetField(backingFieldName, BindingFlags.NonPublic | 
                                                                BindingFlags.Instance | 
                                                                BindingFlags.Static);
                }

                // If still not found, try another common pattern
                if (backingField == null)
                {
                    backingFieldName = $"m_{eventName}";
                    backingField = type.GetField(backingFieldName, BindingFlags.NonPublic | 
                                                                BindingFlags.Instance | 
                                                                BindingFlags.Static);
                }

                // If still not found, try with "on" prefix which is common in some codebases
                if (backingField == null && !eventName.StartsWith("on"))
                {
                    backingFieldName = $"on{eventName}";
                    backingField = type.GetField(backingFieldName, BindingFlags.NonPublic | 
                                                                BindingFlags.Instance | 
                                                                BindingFlags.Static);
                }

                if (backingField != null)
                {
                    // Set the backing field to null, effectively removing all handlers
                    backingField.SetValue(instance, null);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unbinding event handlers: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unbinds all event handlers from the specified static event using reflection.
        /// </summary>
        /// <param name="type">The type that contains the static event.</param>
        /// <param name="eventName">The name of the event to unbind handlers from.</param>
        /// <returns>True if the event was found and handlers were removed, false otherwise.</returns>
        public static bool UnbindAllEventHandlers(Type type, string eventName)
        {
            return UnbindAllEventHandlers(null, eventName);
        }

        /// <summary>
        /// Gets the number of subscribers to an event.
        /// </summary>
        /// <param name="instance">The object instance that contains the event. Use null for static events.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <returns>The number of subscribers, or -1 if the event couldn't be found.</returns>
        public static int GetEventSubscriberCount(object instance, string eventName)
        {
            try
            {
                // Get the type of the object that contains the event
                Type type = instance != null ? instance.GetType() : null;
                
                // For static events, we need the type directly
                if (instance == null && type == null)
                {
                    throw new ArgumentException("For static events, provide the type as the instance parameter");
                }

                // Try to find the backing field with various naming conventions
                string[] possibleFieldNames = new[]
                {
                    eventName,
                    $"_{eventName}",
                    $"m_{eventName}",
                    eventName.StartsWith("on") ? eventName : $"on{eventName}"
                };

                FieldInfo backingField = null;
                foreach (var fieldName in possibleFieldNames)
                {
                    backingField = type.GetField(fieldName, BindingFlags.NonPublic | 
                                                          BindingFlags.Instance | 
                                                          BindingFlags.Static);
                    if (backingField != null)
                        break;
                }

                if (backingField != null)
                {
                    // Get the delegate
                    var value = backingField.GetValue(instance) as Delegate;
                    if (value != null)
                    {
                        // Get the invocation list which contains all subscribers
                        return value.GetInvocationList().Length;
                    }
                    return 0; // No subscribers
                }

                return -1; // Event not found
            }
            catch
            {
                return -1; // Error occurred
            }
        }
    }
}
