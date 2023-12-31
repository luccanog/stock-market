using Microsoft.EntityFrameworkCore;
using Stock.Market.Common.Services;
using Stock.Market.Common.Services.Interfaces;
using Stock.Market.Data;
using Stock.Market.WebApi.GraphQL.Schema;
using Stock.Market.WebApi.GraphQL.Services;
using Stock.Market.WebApi.GraphQL.Services.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Stock.Market.WebApi.GraphQL
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddSingleton<INasdaqService, NasdaqService>();
            builder.Services.AddSingleton<IMessagingService, MessagingService>();

            builder.Services.AddDbContext<ApplicationDBContext>(ServiceLifetime.Singleton);

            builder.Services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>();

            var app = builder.Build();
            
            using (var serviceScope = app.Services.CreateScope())
            {
                Console.WriteLine("EF: EnsureCreated");
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                context.Database.Migrate();
            }
            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGraphQL();
            app.MapControllers();

            app.Run();
        }
    }
}