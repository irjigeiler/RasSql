using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RawSql.Core;

namespace EFCore.RawSql.Postgres
{
    public sealed class EFRawSqlMetadataProvider : IRawSqlMetadataProvider
    {

        public static EFRawSqlMetadataProvider Create(DbContext context) => new EFRawSqlMetadataProvider(context.Model);
        
        public EFRawSqlMetadataProvider(IModel model)
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
}