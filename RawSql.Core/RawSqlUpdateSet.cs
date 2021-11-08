namespace RawSql.Core
{
    public sealed class RawSqlUpdateSet<TEntity> : IRawSqlItem
    {
        public RawSqlUpdateSet(RawSqlColumn<TEntity> column, IRawSqlItem value)
        {
            Column = column;
            Value = value;
        }

        public RawSqlColumn<TEntity> Column { get; }
        public IRawSqlItem Value { get; }

        public void Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}