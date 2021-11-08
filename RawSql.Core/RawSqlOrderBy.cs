namespace RawSql.Core
{
    public class RawSqlOrderBy : IRawSqlItem
    {
        public RawSqlOrderByDirection Direction { get; set; }
        public IRawSqlItem Expression { get; set; }
        
        void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}