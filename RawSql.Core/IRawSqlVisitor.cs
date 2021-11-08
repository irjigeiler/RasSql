namespace RawSql.Core
{
    public interface IRawSqlVisitor
    {
        void Visit<TEntity>(RawSqlTable<TEntity> table);
        void Visit<TEntity>(RawSqlColumn<TEntity> column);
        void Visit<TEntity>(RawSqlUpdate<TEntity> update);
        void Visit<TEntity>(RawSqlUpdateSet<TEntity> set);
        void Visit(RawSqlConstant constant);
        void Visit(RawSqlBinaryOperator @operator);
        void Visit(RawSqlSelect select);
        void Visit(RawSqlJoin join);
        void Visit(RawSqlOrderBy orderBy);
    }
}