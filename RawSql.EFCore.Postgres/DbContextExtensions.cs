using Microsoft.EntityFrameworkCore;
using RawSql.Core;

namespace EFCore.RawSql.Postgres
{
    public static class DbContextExtensions
    {
        public static int Execute(this DbContext context, IRawSqlItem query)
        {
            var queryString = PostgresSqlQueryBuilder.Build(query, context);
            return context.Database.ExecuteSqlRaw(queryString);
        }
    }
}