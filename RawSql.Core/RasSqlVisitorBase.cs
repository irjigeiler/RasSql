using System.Collections.Generic;

namespace RawSql.Core
{
    public abstract class RawSqlVisitorBase : IRawSqlVisitor
    {
        public virtual void Visit<TEntity>(RawSqlTable<TEntity> table)
        {
        }

        public virtual void Visit<TEntity>(RawSqlColumn<TEntity> column)
        {
        }

        public virtual void Visit<TEntity>(RawSqlUpdate<TEntity> update)
        {
            Visit(update.Table);
            Visit(update.Setters);
            Visit(update.Where);
        }

        public virtual void Visit<TEntity>(RawSqlUpdateSet<TEntity> set)
        {
            Visit(set.Column);
            set.Value.Visit(this);
        }

        public virtual void Visit(RawSqlConstant constant)
        {
        }

        public virtual void Visit(RawSqlBinaryOperator @operator)
        {
            Visit(@operator.Left);
            Visit(@operator.Right);
        }

        public virtual void Visit(RawSqlSelect select)
        {
           Visit(select.Columns);
           Visit(select.From);
           Visit(select.Joins);
           Visit(select.Where);
           Visit(select.GroupBy);
           Visit(select.Having);
           Visit(select.OrderBy);
        }

        public virtual void Visit(RawSqlJoin join)
        {
           Visit(join.Source);
           Visit(join.On);
        }

        public virtual void Visit(RawSqlOrderBy orderBy)
        {
            Visit(orderBy.Expression);
        }
        
        protected void Visit(IRawSqlItem item) => item?.Visit(this);

        protected void Visit(IEnumerable<IRawSqlItem> items)
        {
            if (items == null)
            {
                return;
            }
            
            foreach (var item in items)
            {
                item.Visit(this);
            }
        }
    }
}