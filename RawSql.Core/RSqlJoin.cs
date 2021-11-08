namespace RawSql.Core
{
    public class RawSqlJoin : IRawSqlItem
    {
        public RawSqlJoinType JoinType { get; set; }
        public IRawSqlItem Source { get; set; }
        public IRawSqlItem On { get; set; }

        void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}