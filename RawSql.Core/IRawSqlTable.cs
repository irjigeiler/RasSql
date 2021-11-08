using System;

namespace RawSql.Core
{
    public interface IRawSqlTable : IRawSqlItem
    {
        Type EntityType { get; }
    }
}