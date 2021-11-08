namespace RawSql.Core
{
    public class RawSqlConstant : IRawSqlItem
    {
        public RawSqlConstant(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public override string ToString() => Value?.ToString();

        void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}