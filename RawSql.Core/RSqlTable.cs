using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RawSql.Core
{
    public class RawSqlTable<TEntity> : IRawSqlTable
    {
        public RawSqlColumn<TEntity, TProperty> Column<TProperty>(Expression<Func<TEntity, TProperty>> selector)
        {
            var name = selector.GetPropertyName();
            if (ColumnInstances.TryGetValue(name, out var column))
            {
                return (RawSqlColumn<TEntity, TProperty>) column;
            }
            
            var newColumn = new RawSqlColumn<TEntity, TProperty>(this, selector);
            ColumnInstances.Add(name, newColumn);
            return newColumn;
        }

        private IDictionary<string, RawSqlColumn<TEntity>> ColumnInstances { get; } = new Dictionary<string, RawSqlColumn<TEntity>>();

        public override string ToString() => nameof(TEntity);

        Type IRawSqlTable.EntityType => typeof(TEntity);
        
        void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}