using System;
using System.Collections.Generic;
using System.Linq;

namespace RawSql.Core
{
    public class RawSqlAliasManager
    {
        public RawSqlAliasManager(IRawSqlMetadataProvider metadataProvider)
        {
            MetadataProvider = metadataProvider;
        }
        
        private IRawSqlMetadataProvider MetadataProvider { get; }
        private IDictionary<AliasKey, int> AliasIndexes { get; } = new Dictionary<AliasKey, int>();
        private IDictionary<IRawSqlItem, string> Items { get; } = new Dictionary<IRawSqlItem, string>();
        
        public string Alias(IRawSqlItem item, IRawSqlAliasScope scope = null)
        {
            return item switch
            {
                IRawSqlTable table => GetAlias(table, scope, GenerateTableName),
                IRawSqlColumn column => GetAlias(column, scope, GenerateColumnName),
                RawSqlSelect select => GetAlias(select, scope, _ => "source"),
                _ => GetAlias(item, scope, _ => "other")
            };
        }

        private string GetAlias<TItem>(TItem item, IRawSqlAliasScope scope, Func<TItem, string> generateName) where TItem:IRawSqlItem
        {
            if (!Items.TryGetValue(item, out var alias))
            {
                var name = generateName(item);
                var key = AliasKey.CreateSource(name, scope);
                alias = GetAliasName(key, name);
                Items[item] = alias;
            }

            return alias;
        }

        private string GenerateTableName(IRawSqlTable table) => MetadataProvider.TableName(table.EntityType);

        private string GenerateColumnName(IRawSqlColumn column) => MetadataProvider.ColumnName(column.Table.EntityType, column.PropertyInfo);

        private string GetAliasName(AliasKey key, string name)
        {
            AliasIndexes.TryGetValue(key, out var idx);
            var normalized = new string(name.Where((c, i) => i == 0 || Char.IsUpper(c)).Select(Char.ToLowerInvariant).ToArray());
            var result = idx == 0 ? normalized : $"{normalized}_{idx + 1}";
            AliasIndexes[key] = idx + 1;
            return result;
        }
        
        private sealed class AliasKey :IEquatable<AliasKey>
        {
            public AliasKey(string name, AliasType type, IRawSqlAliasScope scope)
            {
                Name = name;
                Type = type;
                Scope = scope;
            }

            public string Name { get;  }
            public AliasType Type { get; }
            public IRawSqlAliasScope Scope { get; }

            public bool Equals(AliasKey other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                
                return Name == other.Name
                       && Type == other.Type 
                       && (ReferenceEquals(Scope, other.Scope) || Scope is null && other.Scope is null);
            }

            public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is AliasKey other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Name, Type, Scope?.GetHashCode() ?? 0);

            public static AliasKey CreateSource(string name, IRawSqlAliasScope scope = null) => new AliasKey(name, AliasType.Source, scope);
            
            public static AliasKey CreateExpression(string name, IRawSqlAliasScope scope) => new AliasKey(name, AliasType.Expression, scope);
        }

        private enum AliasType
        {
            Source,
            Expression
        }
    }
}