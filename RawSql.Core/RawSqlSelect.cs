using System.Collections.Generic;

namespace RawSql.Core
{
    public class RawSqlSelect : IRawSqlItem, IRawSqlAliasScope
    {
        public long? Limit { get; set; }
        public long? Offset { get; set; }
        public IRawSqlItem From { get; set; }
        public IList<IRawSqlItem> Columns { get; set; }
        public IList<RawSqlJoin> Joins { get; set; }
        public IList<IRawSqlItem> GroupBy { get; set; }
        public IList<RawSqlOrderBy> OrderBy { get; set; }
        public IRawSqlItem Where { get; set; }
        public IRawSqlItem Having { get; set; }

        void IRawSqlItem.Visit(IRawSqlVisitor visitor) => visitor.Visit(this);
    }
}