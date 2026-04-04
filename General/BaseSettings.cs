using Shared.JsonNS;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Shared
{
    /// <summary>
    /// Inherit this class in your settings class. Implements basic load/save functions.
    /// </summary>
    /// <typeparam name="T">Class that contains settings.</typeparam>
    /// <example><code>
    /// public class Settings : BaseSettings&lt;Settings&gt;
    /// {
    ///    public override string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "APPNAME", "settings.json");
    ///    [JsonInclude] public override int Version { get; set; } = 1;
    ///    protected override bool OnUpdate()
    ///    {
    ///        return true;
    ///    }
    /// }
    /// </code></example>
    public abstract class BaseSettings<T> where T : BaseSettings<T>, new()
    {
        /// <summary>
        /// File path the instance was loaded from and where <see cref="TrySave"/> will save it to.
        /// </summary>
        [JsonIgnore] public string? FilePath;

        /// <summary>
        /// File path used to load <see cref="Instance"/>.
        /// </summary>
        [JsonIgnore] public abstract string DefaultPath { get; }

        /// <summary>
        /// Current version of the instance. If this is updated, <see cref="OnUpdate"/> is called.
        /// </summary>
        [JsonInclude] public abstract int Version { get; set; }

        /// <summary>
        /// Called, if the file version is different from current version. The returned json is used for deserialization.
        /// </summary>
        /// <param name="oldVersion">Version of the file read.</param>
        /// <param name="json">Raw json of the file.</param>
        protected virtual string BeforeUpdate(int oldVersion, string json) => json;

        /// <summary>
        /// Called, if file version is different from current version. Return true, if file should be (re-)saved.
        /// </summary>
        protected virtual bool OnUpdate() => true;

        /// <summary>
        /// Called, if reading the file throws an exception. Return true, if the file should be reset.
        /// </summary>
        protected virtual bool OnError(Exception e) => true;

        /// <summary>
        /// Try save file to <see cref="FilePath"/>.
        /// </summary>
        public virtual void TrySave() => JsonTool.SerializeFile(FilePath ?? DefaultPath, (T)this, JsonTool.JsonOptions);

        /// <summary>
        /// Returns value of the requested field or property by reflection.
        /// </summary>
        /// <remarks>
        /// Example call:<br/>
        /// Type.GetType("$Namespace.Settings, $AssemblyName")?.BaseType?.GetMethod("TryGetValue")?.Invoke(null, ["Version"])?.ToString();
        /// </remarks>
        /// <param name="name">Name of the field or property.</param>
        public virtual object? GetValue(string name)
        {
            try
            {
                var field = typeof(T).GetField(name);
                if (field != null)
                    return field.GetValue(this);
                var prop = typeof(T).GetProperty(name);
                if (prop != null)
                    return prop.GetValue(this);
            } catch (Exception) { }
            return null;
        }

        /// <summary>
        /// Singleton instance. Loads from <see cref="DefaultPath"/>.
        /// </summary>
        public static T Instance = TryLoad();

        /// <summary>
        /// Try load file. Creates new file, if loading failed for any reason.
        /// </summary>
        /// <param name="filePath">Path to load from and save to. If null, uses <see cref="DefaultPath"/>.</param>
        /// <returns>Loaded instance.</returns>
        public static T TryLoad(string? filePath = null)
        {
            T? instance = null;
            T reference = new();
            filePath ??= reference.DefaultPath;
            bool trySaveIfReset = true;

            try
            {
                if (File.Exists(filePath))
                {
                    // get all text, using FileShare so we can open even busy files
                    string json;
                    {
                        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var reader = new StreamReader(stream);
                        json = reader.ReadToEnd();
                        stream.Close();
                    }

                    // reading version from text and applying BeforeUpdate patch
                    var match = Regex.Match(json, @"\""[Vv]ersion\"":\s*(\d+)");
                    int version = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                    json = reference.BeforeUpdate(version, json);

                    // actually deserializing, and applying OnUpdate patch
                    instance = JsonTool.Deserialize<T>(json);
                    if (instance != null)
                    {
                        instance.FilePath = filePath;
                        if (instance.Version != reference.Version)
                        {
                            if (instance.OnUpdate())
                            {
                                instance.Version = reference.Version;
                                instance.TrySave();
                            }
                        }
                        return instance;
                    }
                }
            } catch (Exception e)
            {
                trySaveIfReset = reference.OnError(e);
                Trace.WriteLine("Error loading settings:");
                Trace.WriteLine(e);
                if (instance != null)
                    return instance;
            }

            Trace.WriteLine("Could not load setting, creating new.");
            reference.FilePath = filePath;
            if (trySaveIfReset)
                reference.TrySave();
            return reference;
        }

        /// <inheritdoc cref="GetValue"/>
        public static object? TryGetValue(string name)
        {
            return Instance.GetValue(name);
        }
    }
}
