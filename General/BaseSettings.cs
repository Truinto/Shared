using Shared.JsonNS;
using System.Diagnostics;

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
        /// Called, if file version is different from current version. Return true, if file should be (re-)saved.
        /// </summary>
        protected virtual bool OnUpdate() => true;

        /// <summary>
        /// Try save file to <see cref="FilePath"/>.
        /// </summary>
        public virtual void TrySave() => JsonTool.SerializeFile(FilePath, (T)this, JsonTool.JsonOptions);

        /// <summary>
        /// Get fields and properties by reflection.
        /// </summary>
        /// <remarks>
        /// Type.GetType("$Namespace.Settings, $AssemblyName")?.BaseType?.GetMethod("TryGetValue")?.Invoke(null, ["Version"])?.ToString();
        /// </remarks>
        /// <param name="name">Name of the field or property.</param>
        /// <returns>Value of the requested member.</returns>
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
            try
            {
                instance = JsonTool.DeserializeFile<T>(filePath);
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
            } catch (Exception)
            {
                if (instance != null)
                {
                    Trace.WriteLine("Settings update failed.");
                    return instance;
                }
            }
            Trace.WriteLine("Could not load setting, creating new.");
            reference.FilePath = filePath;
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
