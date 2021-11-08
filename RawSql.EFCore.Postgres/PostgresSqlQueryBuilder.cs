using System;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RawSql.Core;

namespace EFCore.RawSql.Postgres
{
    public class PostgresSqlQueryBuilder : IRawSqlVisitor
    {
        private PostgresSqlQueryBuilder(BuilderContext context)
        {
            Context = context;
            AliasManager = new RawSqlAliasManager(context);
            Builder = new RawSqlStringBuilder();
        }

        public static string Build(IRawSqlItem item, DbContext context)
        {
            var builderContext = new BuilderContext(context.Model);
            var builder = new PostgresSqlQueryBuilder(builderContext);
            item.Visit(builder);
            return builder.Builder.ToString();
        }

        private BuilderContext Context { get; }
        private RawSqlStringBuilder Builder { get; }
        private RawSqlAliasManager AliasManager { get; }
        
        void IRawSqlVisitor.Visit<TEntity>(RawSqlTable<TEntity> table)
        {
            var name = Context.TableName<TEntity>();
            Builder.AppendQuoted(name);
        }

        void IRawSqlVisitor.Visit<TEntity>(RawSqlColumn<TEntity> column)
        {
            var name = Context.ColumnName(column);
            Builder.AppendQuoted(name);
        }

        void IRawSqlVisitor.Visit<TEntity>(RawSqlUpdate<TEntity> update)
        {
            Builder.Append("UPDATE ");
            Visit(update.Table);
            Builder
                .AppendLine()
                .IncreaseIndent()
                .AppendMultiple(
                    update.Setters,
                    (builder, setter) => Visit(setter),
                    builder => builder.Comma().AppendLine()
                )
                .AppendLine()
                .DecreaseIndent()
                .Append("WHERE ");
            Visit(update.Where);
        }

        private void Visit(IRawSqlItem item)
        {
            item.Visit(this);
        }
        
        private void VisitAsAlias(IRawSqlItem item)
        {
            item.Visit(this);
        }

        void IRawSqlVisitor.Visit<TEntity>(RawSqlSet<TEntity> set)
        {
            var name = Context.ColumnName(set.Column);
            Builder.Append("SET ").AppendQuoted(name).Append(" = ");
            Visit(set.Value);
        }

        void IRawSqlVisitor.Visit(RawSqlConstant constant)
        {
            var value = GetConstantValue(constant);
            Builder.Append(value);
        }

        void IRawSqlVisitor.Visit(RawSqlBinaryOperator @operator)
        {
            Visit(@operator.Left);

            Builder.Space();
            switch (@operator.Operator)
            {
                case RawSqlBinaryOperatorValue.And:
                    Builder.Append("AND");
                    break;
                case RawSqlBinaryOperatorValue.Or:
                    Builder.Append("OR");
                    break;
                case RawSqlBinaryOperatorValue.Equal:
                    Builder.Append("=");
                    break;
                case RawSqlBinaryOperatorValue.NotEqual:
                    Builder.Append("<>");
                    break;
                case RawSqlBinaryOperatorValue.GreaterThan:
                    Builder.Append(">");
                    break;
                case RawSqlBinaryOperatorValue.GreaterOrEqualThen:
                    Builder.Append(">=");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator.Operator));
            }

            Builder.Space();
            Visit(@operator.Right);
        }

        void IRawSqlVisitor.Visit(RawSqlSelect @select)
        {
            Builder
                .Append("SELECT ")
                .IncreaseIndent()
                .AppendMultiple(
                    select.Columns,
                    (builder, expression) =>
                    {
                        Visit(expression);
                        var alias = AliasManager.Alias(expression);
                        builder.Append(" AS ").AppendQuoted(alias);
                    },
                    builder => builder.Comma().AppendLine()
                )
                .AppendLine()
                .DecreaseIndent()
                .Append("FROM ")
                .IncreaseIndent();
            
            Visit(select.From);
            
            Builder
                .DecreaseIndent()
                .AppendMultiple(select.Joins,
                    (builder, join) => Visit(join),
                    builder => builder.AppendLine())
                .AppendLine();

            if (select.Where != null)
            {
                Builder.Append("WHERE ");
                Visit(select.Where);
                Builder.AppendLine();
            }

            if (select.Having != null)
            {
                Builder.Append("HAVING ");
                Visit(select.Having);
                Builder.AppendLine();
            }

            if (select.OrderBy?.Count > 0)
            {
                Builder
                    .Append("ORDER BY")
                    .AppendMultiple(
                        select.OrderBy,
                        (builder, orderBy) => Visit(orderBy),
                        builder => builder.Comma().Space()
                    )
                    .AppendLine();
            }
        }
        
        void IRawSqlVisitor.Visit(RawSqlJoin @join)
        {
            var joinType = GetJoinType(join);
            Builder.Append($"{joinType} JOIN ");
            Visit(join.Source);

            var alias = AliasManager.Alias(join.Source);
            Builder.Append(" AS ").AppendQuoted(alias).Append(" ON ");
            Visit(join.On);
        }

        void IRawSqlVisitor.Visit(RawSqlOrderBy orderBy)
        {
            Visit(orderBy.Expression);
            Builder.Space();
            var direction = orderBy.Direction == RawSqlOrderByDirection.Asc ? "ASC" : "DESC";
            Builder.Append(direction);
        }
        
        private static string GetConstantValue(RawSqlConstant constant)
        {
            return constant.Value switch
            {
                int intValue => intValue.ToString(),
                double doubleValue => doubleValue.ToString(CultureInfo.InvariantCulture),
                string stringValue =>$"'{stringValue}'",
                DateTime dateTimeValue => $"'{dateTimeValue:o}'",
                null => "NULL",
                _ => throw new ArgumentOutOfRangeException(nameof(constant), constant.Value, "Unknown constant type")
            };
        }

        private sealed class BuilderContext : IRawSqlMetadataProvider
        {
            public BuilderContext(IModel model)
            {
                Model = model;
            }

            private IModel Model { get; }

            public string TableName(Type entity) => Model.FindEntityType(entity).GetTableName();
            
            public string ColumnName(Type entityType, MemberInfo property)
            {
                var type = Model.FindEntityType(entityType);
                var schema = type.GetSchema();
                var tableName = type.GetTableName();
                var storeIdentity = StoreObjectIdentifier.Table(tableName, schema);
                var result = type.FindProperty(property);
                return result.GetColumnName(storeIdentity);
            }
        }
        
        private static string GetJoinType(RawSqlJoin join)
        {
            switch (join.JoinType)
            {
                case RawSqlJoinType.Inner:
                    return "INNER";
                case RawSqlJoinType.Left:
                    return "LEFT";
                case RawSqlJoinType.Right:
                    return "RIGHT";
                default:
                    throw new ArgumentOutOfRangeException(nameof(join.JoinType), join.JoinType, "Unsupported join type.");
            }
        }
    }
}