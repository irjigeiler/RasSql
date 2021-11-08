using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace RawSql.Core
{
    public static class RawSqlUtils
    {
        public static string GetPropertyName<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> selector)
        {
            var member = GetMemberInfo(selector);
            return member.Name;
        }
        
        public static MemberInfo GetMemberInfo<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> selector)
        {
            var member = selector.Body as MemberExpression;

            if (member == null)
            {
                throw new ArgumentException($"Cannot extract MemberInfo from {selector}.");
            }

            return member.Member;
        }
        
        public static string TableName<TEntity>(this IRawSqlMetadataProvider provider) => provider.TableName(typeof(TEntity));

        public static string ColumnName<TEntity, TProperty>(this IRawSqlMetadataProvider provider, Expression<Func<TEntity, TProperty>> property)
        {
            var member = property.GetMemberInfo();
            return provider.ColumnName(typeof(TEntity), member);
        }
        
        public static string ColumnName<TEntity>(this IRawSqlMetadataProvider provider, RawSqlColumn<TEntity> column)
        {
            return provider.ColumnName(typeof(TEntity), column.PropertyInfo);
        }
        
        public static Func<TKey, TResult> ConcurrentMemoize<TKey, TResult>(this Func<TKey, TResult> createFunction)
        {
            var dictionary = new ConcurrentDictionary<TKey, TResult>();
            return key => dictionary.GetOrAdd(key, createFunction);
        }
    }
}