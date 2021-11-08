using System;

namespace RawSql.Tests
{
    public class Order
    {
        public long Id { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public long CustomerId { get; set; }
        public DateTime Date { get; set; }
    }
}