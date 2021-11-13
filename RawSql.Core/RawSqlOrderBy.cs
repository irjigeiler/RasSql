namespace RawSql.Core
{
    public class RawSqlOrderBy : IRawSqlItem
    {
        public RawSqlOrderBy(IRawSqlItem expression, RawSqlOrderByDirection direction = RawSqlOrderByDirection.Asc)
        {
            Expression = expression;
            Direction = direction;
        }

        public RawSqlOrderByDirection Direction { get; set; } = RawSqlOrderByDirection.Asc;
        public IRawSqlItem Expression { get; set; }
        
        void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}