global using System.Text.Json;
using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace Shared.JsonNS
{
    /// <summary>
    /// Serialization with System.Text.Json.
    /// </summary>
    /// <remarks>
    /// [JsonInclude] [JsonIgnore]
    /// </remarks>
    public static class JsonTool
    {
        /// <summary>
        /// Serialization settings with fields.
        /// </summary>
        public static JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <summary>
        /// Serialization settings, no fields.
        /// </summary>
        public static JsonSerializerOptions JsonOptionsNoFields = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = false,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <summary>
        /// Serialization settings with $ref.
        /// </summary>
        public static JsonSerializerOptions JsonOptionsRef = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <summary>
        /// Serialization settings with no indenting.
        /// </summary>
        public static JsonSerializerOptions JsonOptionsCompact = new()
        {
            WriteIndented = false,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <summary>
        /// Serialization settings with $ref and no indenting.
        /// </summary>
        public static JsonSerializerOptions JsonOptionsRefCompact = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters = { new JsonStringEnumConverter() },
        };

        public static string Serialize<T>(T value, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize<T>(value, options ?? JsonOptions);
        }

        public static bool Serialize<T>(T value, [NotNullWhen(true)] out string? result, JsonSerializerOptions? options = null)
        {
            try
            {
                return (result = JsonSerializer.Serialize<T>(value, options ?? JsonOptions)) != null;
            } catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize<T>(json, options ?? JsonOptions);
        }

        public static bool Deserialize<T>(string json, [NotNullWhen(true)] out T? result, JsonSerializerOptions? options = null)
        {
            try
            {
                return (result = JsonSerializer.Deserialize<T>(json, options)) != null;
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
                JsonSerializer.Serialize<T>(stream, value, options ?? JsonOptions);
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
            return JsonSerializer.Deserialize<T>(stream, options ?? JsonOptions);
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
    }
}
