using System;

using Newtonsoft.Json;

namespace SwedbankPay.Sdk.JsonSerialization {
    public class TypedSafeEnumValueConverter<TEnum, TValue> : JsonConverter
        where TEnum : TypeSafeEnum<TEnum, TValue> {

        public override bool CanConvert(Type objectType) {
            return typeof(TEnum).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            bool existingIsNull = existingValue == null;
            if (!(existingIsNull || existingValue is TEnum)) {
                throw new JsonSerializationException("Converter cannot read JSON with the specified existing value. {0} is required.");
            }
            return ReadJson(reader, objectType, existingIsNull ? default(TEnum) : (TEnum)existingValue, !existingIsNull, serializer);
        }

        /// <summary>
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        private TEnum ReadJson(JsonReader reader, Type objectType, TEnum existingValue, bool hasExistingValue, JsonSerializer serializer) {
            try {
                TValue value;
                if (reader.TokenType == JsonToken.Integer && typeof(TValue) != typeof(long) && typeof(TValue) != typeof(bool))
                    value = (TValue)Convert.ChangeType(reader.Value, typeof(TValue));
                else
                    value = (TValue)reader.Value;

                return TypeSafeEnum<TEnum, TValue>.FromValue(value);
            } catch (Exception ex) {
                throw new JsonSerializationException($"Error converting {reader.Value ?? "Null"} to {objectType.Name}.", ex);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (!(value != null ? value is TEnum : IsNullable(typeof(TEnum)))) {
                throw new JsonSerializationException("Converter cannot write specified value to JSON. {0} is required.");
            }
            WriteJson(writer, (TEnum)value, serializer);
        }

        /// <summary>
        ///     Write json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        private void WriteJson(JsonWriter writer, TEnum value, JsonSerializer serializer) {
            if (value is null)
                writer.WriteNull();
            else
                writer.WriteValue(value.Value);
        }

        private static bool IsNullable(Type t) {
            ArgumentNotNull(t, nameof(t));

            if (t.IsValueType) {
                return IsNullableType(t);
            }

            return true;
        }

        private static void ArgumentNotNull(object value, string parameterName) {
            if (value == null) {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static bool IsNullableType(Type t) {
            ArgumentNotNull(t, nameof(t));

            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}