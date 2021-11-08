using System.Collections.Generic;
using EFCore.RawSql.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RawSql.Core;

namespace RawSql.Tests
{
    [TestClass]
    public class SelectTests
    {
        private TestContext Context { get; set; }

        [TestMethod]
        public void BuildSelect()
        {
            var orderItem = RawSqlExpressions.Table<OrderItem>();
            var order = RawSqlExpressions.Table<Order>();

            var select = new RawSqlSelect
            {
                From = order,
                Joins = new List<RawSqlJoin>
                {
                    new()
                    {
                        Source = orderItem,
                        JoinType = RawSqlJoinType.Inner,
                        On = RawSqlExpressions.Equal(order.Column(v => v.Id), orderItem.Column(v => v.OrderId))
                    }
                },
                Columns = new List<IRawSqlItem>
                {
                    order.Column(v => v.Id),
                    order.Column(v => v.Date),
                    orderItem.Column(v => v.Product),
                    orderItem.Column(v => v.Price),
                    orderItem.Column(v => v.Quantity),
                }
            };

            var result = PostgresSqlQueryBuilder.Build(select, Context);
            
            Assert.IsNotNull(result);
        }
        
        [TestInitialize]
        public void TestInitialize()
        {
            Context = new TestContext();
            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
        }
        
        [TestCleanup]
        public void TestCleanup()
        {
            Context?.Dispose();
            Context = null;
        }
    }
}