using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NJsonSchema.Converters;

namespace NetPad.Data;

[JsonConverter(typeof(JsonInheritanceConverter<DataConnection>))]
[KnownType("GetKnownTypes")]
public abstract class DataConnection
{
    protected DataConnection(Guid id, string name, DataConnectionType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    public Guid Id { get; }
    public string Name { get; }
    public DataConnectionType Type { get; }

    /// <summary>
    /// Tests if the connection is valid.
    /// </summary>
    /// <returns></returns>
    public abstract Task<DataConnectionTestResult> TestConnectionAsync(IDataConnectionPasswordProtector passwordProtector);

    public override string ToString()
    {
        return $"[{Id}] {Name}";
    }

    #region KnownTypes

    private static Type[] GetKnownTypes()
    {
        return DataConnectionKnownTypes.ScanForKnownTypes();
    }

    private static class DataConnectionKnownTypes
    {
        private static readonly object _lock = new();
        private static Type[]? _knownTypes;

        public static Type[] ScanForKnownTypes()
        {
            if (_knownTypes != null)
                return _knownTypes;

            Type[] knownTypes;

            lock (_lock)
            {
                if (_knownTypes != null)
                    return _knownTypes;

                var dataConnectionType = typeof(DataConnection);

                knownTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name?.StartsWith("NetPad.") == true)
                    .SelectMany(a => a.GetExportedTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && dataConnectionType.IsAssignableFrom(t))
                    .Distinct()
                    .ToArray();

                _knownTypes = knownTypes;
            }

            return _knownTypes;
        }
    }

    #endregion
}
