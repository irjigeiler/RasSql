using System;
using System.Collections.Generic;

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
        
        private string TableAlias(IRawSqlTable table)
        {
            if (!Items.TryGetValue(table, out var alias))
            {
                alias = GenerateTableName(table);
                Items[table] = alias;
            }

            return alias;
        }
        
        private string ColumnAlias(IRawSqlColumn column)
        {
            if (!Items.TryGetValue(column, out var alias))
            {
                alias = GenerateColumnName(column);
                Items[column] = alias;
            }

            return alias;
        }

        public string Alias(IRawSqlItem item)
        {
            if (Items.TryGetValue(item, out var alias))
            {
                return alias;
            }
            
            switch (item)
            {
                case IRawSqlTable table:
                    return TableAlias(table);
                case IRawSqlColumn column:
                    return ColumnAlias(column);
                default:
                    var key = new AliasKey("select", item.GetType());
                    var aliasName = GetAliasName(key, "source");
                    Items[item] = aliasName;
                    return aliasName;
            }
        }

        private string GenerateTableName(IRawSqlTable table)
        {
            var key = new AliasKey(table.EntityType.Name, table.GetType());
            var name = MetadataProvider.TableName(table.EntityType);
            return GetAliasName(key, name);
        }

        private string GenerateColumnName(IRawSqlColumn column)
        {
            var keyName = $"{column.Table.EntityType.Name}.{column.PropertyInfo.Name}";
            var key = new AliasKey(keyName, column.PropertyInfo.DeclaringType);
            var name = MetadataProvider.ColumnName(column.Table.EntityType, column.PropertyInfo);
            return GetAliasName(key, name);
        }
        
        private string GetAliasName(AliasKey key, string name)
        {
            AliasIndexes.TryGetValue(key, out var idx);

            var result = idx == 0 ? name : $"{name}_{idx + 1}";
            AliasIndexes[key] = idx + 1;
            return result;
        }

        private sealed class AliasKey :IEquatable<AliasKey>
        {
            public AliasKey(string name, Type type)
            {
                Name = name;
                Type = type;
            }

            public string Name { get;  }
            public Type Type { get; }

            public bool Equals(AliasKey other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Name == other.Name && Type == other.Type;
            }

            public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is AliasKey other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Name, Type);
        }
    }
}