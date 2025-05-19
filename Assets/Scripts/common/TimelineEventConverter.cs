using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace common
{
    public class TimelineEventConverter: JsonConverter
    {
        private static readonly Dictionary<string, Type> _eventTypes;

        static TimelineEventConverter()
        {
            _eventTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                    t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(TimelineEvent)))
                .ToDictionary(t => t.Name, t => t);
        }
        
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(TimelineEvent));
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
        
            if (jo["eventType"] == null)
                throw new JsonReaderException("eventType property is missing");

            string eventType = jo["eventType"].Value<string>();
            
            //Dialog handling
            if (jo["dialog"] != null && jo["timestamp"] != null)
            {
                var dialogEvent = new DialogEvent(
                    float.Parse(jo["timestamp"].Value<string>()), 
                    jo["dialog"].ToObject<List<DialogLine>>(serializer)
            );
                    
                DialogManager.AddDialogEvent(dialogEvent);

            } 
            
            if (_eventTypes.TryGetValue(eventType, out Type type))
            {
                
                if (type.IsSubclassOf(typeof(GuiEvent)) && !typeof(IInitializableEvent).IsAssignableFrom(type)) return null; // Not sure about this, temporary solution

                if (typeof(IIndirectEvent).IsAssignableFrom(type)) return null; // Added for PContainSpecialistRemoveWhenEntering and PEndTurn
                
                object result;

                if (type.IsSubclassOf(typeof(PlayerEvent)) && !(typeof(IIndirectEvent).IsAssignableFrom(type)))
                {
                    if (jo["currentPlayer"] == null)
                        throw new JsonReaderException("currentPlayer property is missing for PlayerEvent!");

                    string roleString;

                    if (typeof(PMobileHospitalCardPlayed).IsAssignableFrom(type) ||
                        typeof(PForecastCardPlayed).IsAssignableFrom(type) ||
                        typeof(PResourcePlanningCardPlayed).IsAssignableFrom(type) ||
                        typeof(PCallToMobilizeCardPlayed).IsAssignableFrom(type) ||
                        typeof(PResourcePlanning).IsAssignableFrom(type) ||
                        typeof(PForecast).IsAssignableFrom(type) ||
                        typeof(PMobilizeEvent).IsAssignableFrom(type))
                    {
                        roleString = jo["eventInitiator"].Value<string>();
                    }
                    else 
                        roleString = jo["currentPlayer"]["role"]?.Value<string>();
                    
                    if (string.IsNullOrEmpty(roleString))
                    {
                        throw new JsonReaderException("role property is missing or empty for currentPlayer");
                    }

                    if (!Enum.TryParse(roleString, out Player.Roles role))
                        throw new JsonReaderException($"Invalid role: {roleString}");
                    
                    Player player = PlayerList.GetPlayerByRole(role);
                    result = Activator.CreateInstance(type, player);
                }
                else
                {
                    result = jo.ToObject(type, serializer);
                }

                if (result is TimelineEvent timelineEvent)
                {
                    if (jo["timestamp"] != null)
                    {
                        timelineEvent.Timestamp = float.Parse(jo["timestamp"].Value<string>());
                    }
                }
                

                if (result is IInitializableEvent initializableEvent)
                {
                    initializableEvent.InitializeGameObjects(jo);
                }

                return result;
            }
            
            throw new JsonReaderException($"Unknown event type: {eventType}");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        { // Leave empty, not used as the Serialization is done in-class
        }
        
    }
}