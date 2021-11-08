namespace RawSql.Core
{
    public class RawSqlBinaryOperator : IRawSqlItem
    {
        public RawSqlBinaryOperator(RawSqlBinaryOperatorValue @operator, IRawSqlItem left, IRawSqlItem right)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }
        
        public RawSqlBinaryOperatorValue Operator { get; }
        public IRawSqlItem Left { get; }
        public IRawSqlItem Right { get; }

        void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}