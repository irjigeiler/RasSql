using System;
using System.Reflection;

namespace RawSql.Core
{
    public interface IRawSqlMetadataProvider
    {
        string TableName(Type entity);
        string ColumnName(Type entityType, MemberInfo property);
    }
}