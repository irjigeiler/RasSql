using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RawSql.Core
{
    public class RawSelectHierarchy
    {
        public RawSelectHierarchy(ImmutableDictionary<RawSqlSelect, RawSqlSelect> parentToChild, RawSqlSelect sourceScope)
        {
            ParentToChild = parentToChild;
            SourceScope = sourceScope;
        }

        public bool IsRootItem => ParentToChild.Count == 0;
        private ImmutableDictionary<RawSqlSelect, RawSqlSelect> ParentToChild { get; } 
        
      //  public RawSqlSelect GetChildSelect(RawSqlSelect item) => ParentToChild.GetValueOrDefault(item);

        public bool TryGetScope(RawSqlSelect currentScope, out RawSqlSelect select) =>
            ParentToChild.TryGetValue(currentScope, out select);
        
        public RawSqlSelect SourceScope { get; }
        public bool HasExternalReferences { get; set; }
    }

    public class RawSelectHierarchies
    {
        public RawSelectHierarchies(IReadOnlyDictionary<IRawSqlItem, RawSelectHierarchy> items)
        {
            Items = items;
            IsEmpty = items.Count == 0 || items.All(v => v.Value.IsRootItem);
        }

        public bool IsRoot(IRawSqlItem item)
        {
            return !Items.TryGetValue(item, out var value) || value.IsRootItem;
        }
        
        public bool HasExternalReferences(IRawSqlItem item)
        {
            return Items.TryGetValue(item, out var value) && value.HasExternalReferences;
        }
        
        public IRawSqlAliasScope GetParent(IRawSqlItem item, IRawSqlAliasScope currentScope)
        {
            if (!Items.TryGetValue(item, out var hierarchy))
            {
                return null;
            }

            if (hierarchy.SourceScope == currentScope)
            {
                return null;
            }

            if (hierarchy.TryGetScope((RawSqlSelect)currentScope, out var select))//change RawSqlSelect to IRawScope 
            {
                return select;
            }
            
            return null;
        }
        
        private IReadOnlyDictionary<IRawSqlItem, RawSelectHierarchy> Items { get; }
        public bool IsEmpty { get; set; }
    }
}