using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RawSql.Core
{
    public class RawSelectHierarchiesBuilder : RawSqlVisitorBase
    {
        public static RawSelectHierarchies Build(IRawSqlItem root)
        {
            var visitor = new RawSelectHierarchiesBuilder();
            root.Visit(visitor);
            return new RawSelectHierarchies(visitor.Hierarchies);
        }

        private Stack<RawSqlSelect> Context { get; } = new Stack<RawSqlSelect>();
        private ImmutableDictionary<RawSqlSelect, RawSqlSelect> Hierarchy { get; set; } = ImmutableDictionary<RawSqlSelect, RawSqlSelect>.Empty;
        
        private Dictionary<IRawSqlItem, RawSelectHierarchy> Hierarchies { get; } = new Dictionary<IRawSqlItem, RawSelectHierarchy>();

        private void TryAddHierarchy(IRawSqlItem item)
        {
            if (!Context.TryPeek(out var select) || !select.Columns.Contains(item))
            {
                return;
            }

            if (Hierarchies.TryGetValue(item, out var existing))
            {
                existing.HasExternalReferences = true;
                return;
            }

            var hierarchy = new RawSelectHierarchy(Hierarchy, select);
            Hierarchies.Add(item, hierarchy);
        }
        
        private void Push(RawSqlSelect select)
        {
            if (Context.TryPeek(out var parent))
            {
                Hierarchy = Hierarchy.Add(parent, select);
            }
            
            Context.Push(select);
        }

        private void Pop()
        {
            if (!Context.TryPop(out _))
            {
                throw new InvalidOperationException("Context doesn't contain select scope.");
            }

            if (Context.TryPeek(out var parent))
            {
                Hierarchy = Hierarchy.Remove(parent);
            }
        }

        public override void Visit(RawSqlSelect select)
        {
            TryAddHierarchy(select);
            
            Push(select);
           
            Visit(select.From);
            Visit(select.Joins);
            
            Visit(select.Columns);
            Visit(select.Where);
            Visit(select.GroupBy);
            Visit(select.Having);
            Visit(select.OrderBy);
            Pop();
        }

        public override void Visit<TEntity>(RawSqlColumn<TEntity> column)
        {
            TryAddHierarchy(column);
            base.Visit(column);
        }

        public override void Visit(RawSqlBinaryOperator @operator)
        {
            base.Visit(@operator);
            TryAddHierarchy(@operator);
        }
    }
}