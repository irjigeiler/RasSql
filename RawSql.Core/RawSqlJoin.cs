namespace RawSql.Core
{
    public class RawSqlJoin : IRawSqlItem
    {
        public RawSqlJoin(IRawSqlItem source, IRawSqlItem on, RawSqlJoinType joinType = RawSqlJoinType.Inner)
        {
            Source = source;
            On = on;
            JoinType = joinType;
        }
        
        public IRawSqlItem Source { get; set; }
        public IRawSqlItem On { get; set; }
        public RawSqlJoinType JoinType { get; set; }

        void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}