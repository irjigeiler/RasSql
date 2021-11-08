using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RawSql.Core
{
    public class RawSqlUpdate<TEntity> : IRawSqlItem
    {
        public RawSqlUpdate(RawSqlTable<TEntity> table)
        {
            Table = table;
        }

        public RawSqlTable<TEntity> Table { get; }
        public IList<RawSqlSet<TEntity>> Setters { get; internal set; } = new List<RawSqlSet<TEntity>>();
        public IRawSqlItem Where { get; set; } 
        
        public RawSqlUpdate<TEntity> AddSet<TProperty>(Expression<Func<TEntity, TProperty>> selector, IRawSqlItem value)
        {
            var column = Table.Column(selector);
            var setter = new RawSqlSet<TEntity>(column, value);
            Setters.Add(setter);
            return this;
        }
        
         void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}