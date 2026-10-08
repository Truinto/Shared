global using Newtonsoft.Json;
global using JsonInclude = Newtonsoft.Json.JsonPropertyAttribute;
global using JsonSerializerOptions = Newtonsoft.Json.JsonSerializerSettings;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
            ContractResolver = new WritableMembersOnlyResolver(),
        };

        /// <summary>
        /// Serialization settings, no fields.
        /// </summary>
        public static JsonSerializerOptions JsonOptionsNoFields = new()
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
        /// Serialization settings with $ref (with fields).
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
            ContractResolver = new WritableMembersOnlyResolver(),
        };

        /// <summary>
        /// Serialization settings with no indenting (with fields).
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
            ContractResolver = new WritableMembersOnlyResolver(),
        };

        /// <summary>
        /// Serialization settings with $ref and no indenting (with fields).
        /// </summary>
        public static JsonSerializerOptions JsonOptionsRefCompact = new()
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
            ContractResolver = new WritableMembersOnlyResolver(),
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
            }
            catch (Exception) { }
            result = default;
            return false;
        }

        public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            return (T?)JsonConvert.DeserializeObject(json, typeof(T), options ?? JsonOptions);
        }

        public static bool Deserialize<T>(string? json, [NotNullWhen(true)] out T? result, JsonSerializerOptions? options = null)
        {
            try
            {
                if (json != null)
                    return (result = (T?)JsonConvert.DeserializeObject(json, typeof(T), options ?? JsonOptions)) != null;
            }
            catch (Exception) { }
            result = default;
            return false;
        }

        public static bool SerializeFile<T>(string? path, T value, JsonSerializerOptions? options = null)
        {
            try
            {
                if (path is null or "")
                    return false;
                if (Path.GetDirectoryName(path) is string directory)
                    Directory.CreateDirectory(directory);

                using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                stream.SetLength(0); // truncate
                using var writer = new StreamWriter(stream);
                JsonSerializer.Create(options ?? JsonOptions).Serialize(writer, value, typeof(T));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static T? DeserializeFile<T>(string path, JsonSerializerOptions? options = null)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            return (T?)JsonSerializer.Create(options ?? JsonOptions).Deserialize(reader, typeof(T));
        }

        public static bool DeserializeFile<T>(string? path, [NotNullWhen(true)] out T? result, JsonSerializerOptions? options = null)
        {
            try
            {
                if (path != null)
                    return (result = DeserializeFile<T>(path, options)) != null;
            }
            catch (Exception) { }
            result = default;
            return false;
        }

        public class WritableMembersOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization).Where(w => w.Writable).ToList();
            }
        }

        public class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization).Where(w => w.Writable && w.PropertyType?.MemberType == MemberTypes.Property).ToList();
            }
        }
    }
}
