namespace MongoOutboxSample.Messages
{
    public class OrderCreatedEvent
    {
        public Guid CorrelationId { get; set; }

        public int OrderId { get; set; }

        public DateTime Created { get; set; }
    }
}
