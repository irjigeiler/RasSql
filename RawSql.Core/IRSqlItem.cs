namespace RawSql.Core
{
    public interface IRawSqlItem
    {
        void Visit(IRawSqlVisitor visitor);
    }
}