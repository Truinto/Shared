using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.IEnum
{
    [JsonConverter(typeof(IEnum<ExampleIEnum>.Converter))]
    public readonly struct ExampleIEnum(int number) : IEnum<ExampleIEnum>
    {
        public readonly int Int { get; } = number;

        #region constants
        // public const int ([@A-z]+) = ([-0-9]+);
        // $2 => "$1",
        // "$1" => $2,

        public const int No = 0;
        public const int Yes = 1;
        public const int invalid = -1;
        #endregion

        public readonly override string ToString()
        {
            return this.Int switch
            {
                0 => "No",
                1 => "Yes",
                _ => "invalid"
            };
        }

        public static ExampleIEnum Parse(string? name)
        {
            return name switch
            {
                "No" => 0,
                "Yes" => 1,
                _ => invalid
            };
        }

        public static implicit operator ExampleIEnum(int number)
        {
            return new(number);
        }
    }

    public interface IEnum<T> where T : IEnum<T>
    {
        public int Int { get; }

        public static abstract T Parse(string? name);

        public static abstract implicit operator T(int number);

        public static virtual bool TryParse(string name, out T code)
        {
            code = T.Parse(name);
            return code != -1;
        }

        public class Converter : JsonConverter<T>
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                    return T.Parse(reader.GetString());
                if (reader.TokenType == JsonTokenType.Number)
                    return (T)reader.GetInt32();
                return (T)0;
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        #region equality

        public static virtual bool operator ==(T value1, T value2)
        {
            return value1.Int == value2.Int;
        }

        public static virtual bool operator !=(T value1, T value2)
        {
            return value1.Int != value2.Int;
        }

        public static virtual bool operator ==(T value1, int value2)
        {
            return value1.Int == value2;
        }

        public static virtual bool operator !=(T value1, int value2)
        {
            return value1.Int != value2;
        }

        public virtual bool Equals(object? obj)
        {
            if (obj is T type)
                return this.Int == type.Int;
            if (obj is int number)
                return this.Int == number;
            if (obj is string name)
                return this.Int == T.Parse(name).Int;
            return false;
        }

        public virtual int GetHashCode()
        {
            return this.Int.GetHashCode();
        }

        #endregion
    }
}
