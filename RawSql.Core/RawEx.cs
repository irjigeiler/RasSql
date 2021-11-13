using System.Collections.Generic;
using System.Linq;

namespace RawSql.Core
{
    public static class RawEx
    {
        public static RawSqlTable<TEntity> Table<TEntity>() => new RawSqlTable<TEntity>();
        
        public static RawSqlUpdate<TEntity> Update<TEntity>(RawSqlTable<TEntity> table, IEnumerable<RawSqlUpdateSet<TEntity>> setters, IRawSqlItem where = null)
        {
            return new RawSqlUpdate<TEntity>(table)
            {
                Setters = setters.ToList(),
                Where = where
            };
        }

        public static RawSqlUpdateSet<TEntity> Set<TEntity, TProperty>(RawSqlColumn<TEntity, TProperty> column, TProperty value)
        {
            return new RawSqlUpdateSet<TEntity>(column, new RawSqlConstant(value));
        }
        
        
        
        public static RawSqlConstant Constant(object value) => new RawSqlConstant(value);
        
        //Operators
        
        public static RawSqlBinaryOperator Equal(IRawSqlItem left, IRawSqlItem right) => new RawSqlBinaryOperator(RawSqlBinaryOperatorValue.Equal, left, right);
        
        public static RawSqlBinaryOperator GreaterThan(IRawSqlItem left, IRawSqlItem right) => new RawSqlBinaryOperator(RawSqlBinaryOperatorValue.GreaterThan, left, right);
        
        public static RawSqlBinaryOperator And(IRawSqlItem left, IRawSqlItem right) => new RawSqlBinaryOperator(RawSqlBinaryOperatorValue.And, left, right);

        public static RawSqlBinaryOperator And(IRawSqlItem left, IRawSqlItem right, params  IRawSqlItem[] others)
        {
            var first = And(left,right);
            return others.Aggregate(first, And);
        }
        
        public static RawSqlBinaryOperator Or(IRawSqlItem left, IRawSqlItem right) => new RawSqlBinaryOperator(RawSqlBinaryOperatorValue.Or, left, right);

        public static RawSqlBinaryOperator Or(IRawSqlItem left, IRawSqlItem right, params  IRawSqlItem[] others)
        {
            var first = Or(left,right);
            return others.Aggregate(first, Or);
        }
        
        public static RawSqlBinaryOperator Multiply(IRawSqlItem left, IRawSqlItem right) => new RawSqlBinaryOperator(RawSqlBinaryOperatorValue.Multiply, left, right);
        public static RawSqlBinaryOperator Subtract(IRawSqlItem left, IRawSqlItem right) => new RawSqlBinaryOperator(RawSqlBinaryOperatorValue.Subtract, left, right);
        public static RawSqlBinaryOperator Divide(IRawSqlItem left, IRawSqlItem right) => new RawSqlBinaryOperator(RawSqlBinaryOperatorValue.Divide, left, right);
        public static RawSqlBinaryOperator Add(IRawSqlItem left, IRawSqlItem right) => new RawSqlBinaryOperator(RawSqlBinaryOperatorValue.Add, left, right);

    }
}