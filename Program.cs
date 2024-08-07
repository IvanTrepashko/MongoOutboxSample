using MongoDB.Driver;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver.Core.Configuration;

namespace MongoOutboxSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            RegisterOutbox(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void RegisterOutbox(WebApplicationBuilder builder)
        {
            const string databaseName = "outboxSample";

            builder.Services.TryAddSingleton<IMongoClient>(p =>
            {
                var loggerFactory = LoggerFactory.Create(b =>
                {
                    b.AddSimpleConsole();
                    b.SetMinimumLevel(LogLevel.Debug);
                });

                var connectionString = builder.Configuration.GetConnectionString("Default");
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                settings.LoggingSettings = new LoggingSettings(loggerFactory);
                return new MongoClient(settings);
            });

            builder.Services.AddSingleton<IMongoDatabase>(provider =>
                provider.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

            builder.Services.AddMassTransit(x =>
            {
                x.AddMongoDbOutbox(o =>
                {
                    o.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());
                    o.DatabaseFactory(provider => provider.GetRequiredService<IMongoDatabase>());

                    o.UseBusOutbox(bo =>
                    {
                    });
                });

                x.UsingRabbitMq((_, cfg) =>
                {
                    cfg.Host("rabbitmq://root:example@localhost:5672/test");
                    cfg.AutoStart = true;
                });
            });
        }
    }
}
