using MassTransit;
using MassTransit.MongoDbIntegration;
using Microsoft.AspNetCore.Mvc;
using MongoOutboxSample.Messages;

namespace MongoOutboxSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IPublishEndpoint _endpoint;
        private readonly MongoDbContext _context;

        public TestController(IPublishEndpoint endpoint, MongoDbContext context)
        {
            _endpoint = endpoint;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Publish()
        {
            var e = new OrderCreatedEvent()
            {
                CorrelationId = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                OrderId = 1
            };

            await _context.BeginTransaction(HttpContext.RequestAborted);

            await _endpoint.Publish(e, HttpContext.RequestAborted);

            await _context.CommitTransaction(HttpContext.RequestAborted);

            return Ok();
        }
    }
}
