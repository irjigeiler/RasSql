using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RawSql.Core
{
    public abstract class RawSqlColumn<TEntity> : IRawSqlColumn
    {
        protected RawSqlColumn(RawSqlTable<TEntity> table, MemberInfo propertyInfo)
        {
            Table = table;
            PropertyInfo = propertyInfo;
        }

        public RawSqlTable<TEntity> Table { get; }
        public MemberInfo PropertyInfo { get; }
        
        void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);

        IRawSqlTable IRawSqlColumn.Table => Table;
    }
    
    public class RawSqlColumn<TEntity, TProperty> : RawSqlColumn<TEntity>
    {
        public RawSqlColumn(RawSqlTable<TEntity> table, Expression<Func<TEntity, TProperty>> property) : base(table, property.GetMemberInfo())
        {
            Property = property;
        }

        public Expression<Func<TEntity, TProperty>> Property { get; }

        public override string ToString() => $"{nameof(TEntity)}.{Property.GetPropertyName()}";
    }
}