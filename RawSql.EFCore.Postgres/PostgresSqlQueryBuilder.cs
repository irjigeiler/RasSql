using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RawSql.Core;

namespace EFCore.RawSql.Postgres
{
    public class PostgresSqlQueryBuilder : IRawSqlVisitor
    {
        private PostgresSqlQueryBuilder(EFRawSqlMetadataProvider metadataProvider, RawSelectHierarchies selectHierarchies)
        {
            MetadataProvider = metadataProvider;
            AliasManager = new RawSqlAliasManager(metadataProvider);
            Builder = new RawSqlStringBuilder();
            Hierarchies = selectHierarchies;
            Context = new BuilderContext
            {
            };
        }

        public static string Build(IRawSqlItem item, DbContext context)
        {
            var hierarchies = RawSelectHierarchiesBuilder.Build(item);
            var metadataProvider = EFRawSqlMetadataProvider.Create(context);
            var queryBuilder = new PostgresSqlQueryBuilder(metadataProvider, hierarchies);
            item.Visit(queryBuilder);
            return queryBuilder.Builder.ToString();
        }

        private EFRawSqlMetadataProvider MetadataProvider { get; }
        private RawSqlStringBuilder Builder { get; }
        private RawSqlAliasManager AliasManager { get; }
        private RawSelectHierarchies Hierarchies { get; }
        private BuilderContext Context { get; }
        
        public void Visit<TEntity>(RawSqlTable<TEntity> table)
        {
            var name = MetadataProvider.TableName<TEntity>();
            Builder.AppendQuoted(name);
            // if (Context.UseTableAliases)
            // {
            //     var alias = AliasManager.Alias(table);
            //     Builder.Space().Append(alias);
            // }
        }

        public void Visit<TEntity>(RawSqlColumn<TEntity> column)
        {
            var parent = Hierarchies.GetParent(column, Context.CurrentAliasScope);
            if (parent != null)
            {
                var sourceAlias = AliasManager.Alias(parent);
                var columnAlias = AliasManager.Alias(column, parent);
                Builder.Append(sourceAlias).Dot().Append(columnAlias);
                return;
            }
            
            if (Context.UseTableAliases)
            {
                var alias = AliasManager.Alias(column.Table);
                Builder.Append(alias).Dot();
            }
            
            var name = MetadataProvider.ColumnName(column);
            Builder.AppendQuoted(name);
        }

        private bool TryVisitAsReference(IRawSqlItem item)
        {
            var parent = Hierarchies.GetParent(item, Context.CurrentAliasScope);
            if (parent != null)
            {
                var sourceAlias = AliasManager.Alias(parent);
                var columnAlias = AliasManager.Alias(item, parent);
                Builder.Append(sourceAlias).Dot().Append(columnAlias);
            }

            return parent != null;
        }

        public void Visit<TEntity>(RawSqlUpdate<TEntity> update)
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
            update.Where.Visit(this);
        }

        public void Visit<TEntity>(RawSqlUpdateSet<TEntity> set)
        {
            var name = MetadataProvider.ColumnName(set.Column);
            Builder.Append("SET ").AppendQuoted(name).Append(" = ");
            set.Value.Visit(this);
        }

        public void Visit(RawSqlConstant constant)
        {
            var value = GetConstantValue(constant);
            Builder.Append(value);
        }

        public void Visit(RawSqlBinaryOperator @operator)
        {
            if (TryVisitAsReference(@operator))
            {
                return;
            }
            
            @operator.Left.Visit(this);

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
                
                case RawSqlBinaryOperatorValue.Multiply:
                    Builder.Append("*");
                    break;
                case RawSqlBinaryOperatorValue.Divide:
                    Builder.Append("/");
                    break;
                case RawSqlBinaryOperatorValue.Add:
                    Builder.Append("+");
                    break;
                case RawSqlBinaryOperatorValue.Subtract:
                    Builder.Append("-");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator.Operator));
            }

            Builder.Space();
            @operator.Right.Visit(this);
        }

        public void Visit(RawSqlSelect select)
        {
            Builder.AppendIf(Context.CurrentAliasScope != null, "(");
            
            Context.BeginAliasScope(select);
            Builder
                .Append("SELECT ")
                .IncreaseIndent()
                .AppendMultiple(
                    select.Columns,
                    BuildSelectItem,
                    builder => builder.Comma().AppendLine()
                )
                .AppendLine()
                .DecreaseIndent()
                .Append("FROM ")
                .IncreaseIndent();
            
            select.From.Visit(this);
            if (Context.UseTableAliases)
            {
                var fromAlias = AliasManager.Alias(select.From);
                Builder.Space().Append(fromAlias);
            }
            
            Builder.DecreaseIndent();
            Builder.AppendLine();
            if (select.Joins?.Count > 0)
            {
                Builder
                    .AppendMultiple(select.Joins,
                        (builder, join) => Visit(join),
                        builder => builder.AppendLine())
                    .AppendLine();
            }

            if (select.Where != null)
            {
                Builder.Append("WHERE ");
                select.Where.Visit(this);
                Builder.AppendLine();
            }

            if (select.GroupBy?.Count > 0)
            {
                Builder
                    .Append("GROUP BY ")
                    .AppendMultiple(
                        select.GroupBy,
                        (builder, groupBy) => groupBy.Visit(this),
                        builder => builder.Comma().Space()
                    )
                    .AppendLine();
            }

            if (select.Having != null)
            {
                Builder.Append("HAVING ");
                select.Having.Visit(this);
                Builder.AppendLine();
            }

            if (select.OrderBy?.Count > 0)
            {
                Builder
                    .Append("ORDER BY ")
                    .AppendMultiple(
                        select.OrderBy,
                        (builder, orderBy) => Visit(orderBy),
                        builder => builder.Comma().Space()
                    )
                    .AppendLine();
            }

            Context.EndAliasScope();
            Builder.AppendIf(Context.CurrentAliasScope != null, ")");
        }
        
        public void Visit(RawSqlJoin join)
        {
            var joinType = GetJoinType(join);
            Builder.Append($"{joinType} JOIN ");
            join.Source.Visit(this);

            var alias = AliasManager.Alias(join.Source);
            Builder.Space().AppendQuoted(alias).Append(" ON ");
            join.On.Visit(this);
        }

        public void Visit(RawSqlOrderBy orderBy)
        {
            orderBy.Expression.Visit(this);
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
        
        private void BuildSelectItem<TItem>(RawSqlStringBuilder builder, TItem item) where TItem : IRawSqlItem
        {
            item.Visit(this);
            
            if (Hierarchies.IsRoot(item) || !Hierarchies.HasExternalReferences(item))
            {
                return;
            }
            
            var alias = AliasManager.Alias(item, Context.CurrentAliasScope);
            var parent = Hierarchies.GetParent(item, Context.CurrentAliasScope);
            if (parent == null || AliasManager.Alias(item, parent) != alias)
            {
                builder.Append(" AS ").Append(alias);
            }
        }
        
        private static string GetJoinType(RawSqlJoin join)
        {
            return join.JoinType switch
            {
                RawSqlJoinType.Inner => "INNER",
                RawSqlJoinType.Left => "LEFT",
                RawSqlJoinType.Right => "RIGHT",
                _ => throw new ArgumentOutOfRangeException(nameof(join.JoinType), join.JoinType, "Unsupported join type.")
            };
        }

        #region Context
        
        private sealed class BuilderContext
        {
            public BuilderContext()
            {
            }
            
            private Stack<IRawSqlAliasScope> AliasScopes { get; } = new Stack<IRawSqlAliasScope>();
            public bool UseTableAliases =>
                CurrentAliasScope is RawSqlSelect select 
                && (select.Joins?.Any() == true || !(select.From is IRawSqlTable));

            public void BeginAliasScope(IRawSqlAliasScope select)
            {
                AliasScopes.Push(select);
            }

            public void EndAliasScope()
            {
                AliasScopes.Pop();
            }

            public IRawSqlAliasScope CurrentAliasScope => AliasScopes.TryPeek(out var item) ? item : null;
        }

        #endregion
    }
}