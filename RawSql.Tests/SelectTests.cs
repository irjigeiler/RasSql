using System;
using System.Collections.Generic;
using EFCore.RawSql.Postgres;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RawSql.Core;

namespace RawSql.Tests
{
    [TestClass]
    public class SelectTests
    {
        private TestContext Context { get; set; }
        
        [TestMethod]
        public void SelectWithJoin()
        {
            var orderItem = RawEx.Table<OrderItem>();
            var order = RawEx.Table<Order>();

            var select = new RawSqlSelect
            {
                From = order,
                Joins = new List<RawSqlJoin>
                {
                    new(orderItem, 
                        RawEx.Equal(order.Column(v => v.Id), orderItem.Column(v => v.OrderId)))
                },
                Columns = new List<IRawSqlItem>
                {
                    order.Column(v => v.Id),
                    order.Column(v => v.Date),
                    orderItem.Column(v => v.Product),
                    orderItem.Column(v => v.Price),
                    orderItem.Column(v => v.Quantity),
                },
                OrderBy = new List<RawSqlOrderBy>
                {
                    new(orderItem.Column(v => v.Price)),
                    new(orderItem.Column(v => v.Quantity), RawSqlOrderByDirection.Desc),
                },
                Where = RawEx.GreaterThan(orderItem.Column(v => v.Quantity), new RawSqlConstant(1))
            };

            var result = PostgresSqlQueryBuilder.Build(select, Context);
            
            Console.WriteLine(result);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SelectWithSubSelect()
        {

            var orders = RawEx.Table<Order>();
            var orderItems = RawEx.Table<OrderItem>();

            var productTotal = RawEx.Multiply(orderItems.Column(v => v.Price), orderItems.Column(v => v.Quantity));
            var select = new RawSqlSelect
            {
                Columns = new List<IRawSqlItem>
                {
                    orders.Column(v => v.Country),
                    productTotal
                },
                From = orders,
                Joins = new List<RawSqlJoin>
                {
                    new(orderItems, RawEx.Equal(orders.Column(v => v.Id), orderItems.Column(v => v.OrderId)))
                }
            };

            var main = new RawSqlSelect
            {
                Columns = new List<IRawSqlItem>
                {
                    orders.Column(v => v.Country),
                    productTotal
                },
                From = select
            };
            
            var result = PostgresSqlQueryBuilder.Build(main, Context);
            Console.WriteLine(result);
            
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