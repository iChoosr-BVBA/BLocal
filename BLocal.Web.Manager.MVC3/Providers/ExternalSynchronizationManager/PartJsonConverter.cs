using System;
using BLocal.Core;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class PartJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var part = (Part) value;
            writer.WriteStartObject();
            writer.WritePropertyName("serialized");
            writer.WriteValue(part.ToString());
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            reader.Read(); // property name
            reader.Read(); // property value
            var part = Part.Parse((String)reader.Value);
            reader.Read();
            return part;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (Part);
        }
    }
}