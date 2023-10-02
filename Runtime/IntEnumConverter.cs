using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Reflection;

namespace UnityRestClient
{
    public class IntEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            Type t = value.GetType();
            if (Enum.GetUnderlyingType(t) == typeof(byte))
                writer.WriteValue((byte)value);
            if (Enum.GetUnderlyingType(t) == typeof(sbyte))
                writer.WriteValue((sbyte)value);
            if (Enum.GetUnderlyingType(t) == typeof(ushort))
                writer.WriteValue((ushort)value);
            if (Enum.GetUnderlyingType(t) == typeof(short))
                writer.WriteValue((short)value);
            if (Enum.GetUnderlyingType(t) == typeof(uint))
                writer.WriteValue((uint)value);
            if (Enum.GetUnderlyingType(t) == typeof(int))
                writer.WriteValue((int)value);
            if (Enum.GetUnderlyingType(t) == typeof(ulong))
                writer.WriteValue((ulong)value);
            if (Enum.GetUnderlyingType(t) == typeof(long))
                writer.WriteValue((long)value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            try
            {
                if (reader.TokenType == JsonToken.Integer)
                {
                    return Enum.ToObject(objectType, reader.Value);
                }
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, "Error converting value {0} to type '{1}'.", reader.Value.ToString(), objectType), ex);
            }

            // we don't actually expect to get here.
            throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, "Unexpected token {0} when parsing enum.", reader.TokenType));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsEnum;
        }
    }
}