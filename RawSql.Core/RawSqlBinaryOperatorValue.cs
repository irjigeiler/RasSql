namespace RawSql.Core
{
    public enum RawSqlBinaryOperatorValue
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterOrEqualThen,
        
        And,
        Or,
        
        Multiply,
        Add,
        Subtract,
        Divide,
    }

    // public sealed class RawSqlOperator : IRawSqlItem, IEquatable<RawSqlOperator>
    // {
    //     //Comparison
    //     public static RawSqlOperator Equal { get; } = new RawSqlOperator("=");
    //     public static RawSqlOperator NotEqual { get; } = new RawSqlOperator("<>");
    //     public static RawSqlOperator GreaterThan { get; } = new RawSqlOperator(">");
    //     public static RawSqlOperator GreaterOrEqualThan { get; } = new RawSqlOperator(">=");
    //     public static RawSqlOperator LessThan { get; } = new RawSqlOperator("<");
    //     public static RawSqlOperator LessOrEqualThan { get; } = new RawSqlOperator("<=");
    //     
    //     //Arithmetic
    //     public static RawSqlOperator Add { get; } = new RawSqlOperator("+");
    //     public static RawSqlOperator Subtract { get; } = new RawSqlOperator("-");
    //     
    //     private RawSqlOperator(string representation)
    //     {
    //         Representation = representation;
    //     }
    //
    //     public string Representation { get; }
    //
    //     public override string ToString() => Representation;
    //  
    //
    //     public bool Equals(RawSqlOperator other)
    //     {
    //         if (ReferenceEquals(null, other)) return false;
    //         if (ReferenceEquals(this, other)) return true;
    //         return Representation == other.Representation;
    //     }
    //
    //     public override bool Equals(object obj)
    //     {
    //         return ReferenceEquals(this, obj) || obj is RawSqlOperator other && Equals(other);
    //     }
    //
    //     public override int GetHashCode()
    //     {
    //         return (Representation != null ? Representation.GetHashCode() : 0);
    //     }
    // }
    
}