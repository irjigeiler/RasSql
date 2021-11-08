namespace RawSql.Tests
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}