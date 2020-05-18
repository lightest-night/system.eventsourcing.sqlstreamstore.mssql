using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LightestNight.System.EventSourcing.SqlStreamStore.MsSql.Scripts
{
    internal class Scripts
    {
        private readonly string _schema;
        private readonly ConcurrentDictionary<string, string> _scripts = new ConcurrentDictionary<string,string>();

        internal string CreateSchema => GetScript();
        internal string SetCheckpoint => GetScript();
        internal string GetCheckpoint => GetScript();
        
        internal Scripts(string schema)
        {
            _schema = schema;
        }

        private string GetScript([CallerMemberName] string? name = default)
            => _scripts.GetOrAdd(name ?? string.Empty,
                (key, assembly) =>
                {
                    using var stream =
                        assembly.GetManifestResourceStream(
                            $"LightestNight.System.EventSourcing.SqlStreamStore.MsSql.Scripts.{key}.sql");
                    if (stream == null)
                        throw new FileNotFoundException($"Embedded resource '{key}' was not found.");
                
                    using var reader = new StreamReader(stream);
                    return reader.ReadToEnd().Replace("dbo", _schema, StringComparison.InvariantCultureIgnoreCase);
                }, typeof(Scripts).GetTypeInfo().Assembly);
    }
}