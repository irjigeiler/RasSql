namespace RawSql.Core
{
    public sealed class RawSqlSet<TEntity> : IRawSqlItem
    {
        public RawSqlSet(RawSqlColumn<TEntity> column, IRawSqlItem value)
        {
            Column = column;
            Value = value;
        }

        public RawSqlColumn<TEntity> Column { get; }
        public IRawSqlItem Value { get; }

        public void Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}