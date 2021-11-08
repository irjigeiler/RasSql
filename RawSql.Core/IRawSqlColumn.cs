using System.Reflection;

namespace RawSql.Core
{
    public interface IRawSqlColumn : IRawSqlItem
    {
        IRawSqlTable Table { get; }
        MemberInfo PropertyInfo { get; }
    }
}