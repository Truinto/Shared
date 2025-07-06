global using JsonInclude = Newtonsoft.Json.JsonPropertyAttribute;
global using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using JsonSerializerOptions = Newtonsoft.Json.JsonSerializerSettings;

namespace Shared.JsonNS
{
    /// <summary>
    /// Serialization with Newtonsoft.Json.
    /// </summary>
    /// <remarks>
    /// [JsonInclude] [JsonIgnore]
    /// </remarks>
    public static class JsonTool
    {
        public static List<JsonConverter> DefaultConverters = [new StringEnumConverter()];

        /// <summary>
        /// Serialization settings with fields.
        /// </summary>
        public static JsonSerializerOptions JsonOptions = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.Default,
            Converters = DefaultConverters,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new WritablePropertiesOnlyResolver(),
        };

        /// <summary>
        /// Serialization settings with $ref.
        /// </summary>
        public static JsonSerializerOptions JsonOptionsRef = new()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.Default,
            Converters = DefaultConverters,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new WritablePropertiesOnlyResolver(),
        };

        /// <summary>
        /// Serialization settings with no indenting.
        /// </summary>
        public static JsonSerializerOptions JsonOptionsCompact = new()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.Default,
            Converters = DefaultConverters,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new WritablePropertiesOnlyResolver(),
        };

        /// <summary>
        /// Serialization settings with $ref and no indenting.
        /// </summary>
        public static JsonSerializerOptions JsonOptionsCompactRef = new()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.Default,
            Converters = DefaultConverters,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new WritablePropertiesOnlyResolver(),
        };

        public static string Serialize<T>(T value, JsonSerializerOptions? options = null)
        {
            return JsonConvert.SerializeObject(value, typeof(T), options ?? JsonOptions);
        }

        public static bool Serialize<T>(T value, [NotNullWhen(true)] out string? result, JsonSerializerOptions? options = null)
        {
            try
            {
                return (result = JsonConvert.SerializeObject(value, typeof(T), options ?? JsonOptions)) != null;
            } catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
        {
            return (T?)JsonConvert.DeserializeObject(json, typeof(T), options ?? JsonOptions);
        }

        public static bool Deserialize<T>(string json, [NotNullWhen(true)] out T? result, JsonSerializerOptions? options = null)
        {
            try
            {
                return (result = (T?)JsonConvert.DeserializeObject(json, typeof(T), options ?? JsonOptions)) != null;
            } catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public static bool SerializeFile<T>(string path, T value, JsonSerializerOptions? options = null)
        {
            try
            {
                if (Path.GetDirectoryName(path) is string directory)
                    Directory.CreateDirectory(directory);

                using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream);
                JsonSerializer.Create(options ?? JsonOptions).Serialize(writer, value, typeof(T));
                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        public static T? DeserializeFile<T>(string path, JsonSerializerOptions? options = null)
        {
            if (!File.Exists(path))
                return default;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            return (T?)JsonSerializer.Create(options ?? JsonOptions).Deserialize(reader, typeof(T));
        }

        public static bool DeserializeFile<T>(string path, [NotNullWhen(true)] out T? result, JsonSerializerOptions? options = null)
        {
            try
            {
                return (result = DeserializeFile<T>(path, options)) != null;
            } catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);
                for (int i = props.Count - 1; i >= 0; i--)
                {
                    if (!props[i].Writable)
                        props.RemoveAt(i);
                }
                return props;
            }
        }
    }
}
