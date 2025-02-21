using System;
using System.Reflection;
namespace Tiled.Events
{
    public static class EventHelper
    {
        public static void RemoveAllEventHandlers(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            Type type = obj.GetType();

            foreach (EventInfo eventInfo in type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {

                FieldInfo fieldInfo = type.GetField(eventInfo.Name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

                if (fieldInfo != null)
                {
                    // Set the field to null to remove all event handlers
                    fieldInfo.SetValue(obj, null);
                }
            }
        }
    }
}
