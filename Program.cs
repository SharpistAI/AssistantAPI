
using System.Text.Json.Serialization;
using AssistantAPI.Service;
using Polly;

namespace AssistantAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var retryPolicy = Policy.Handle<Exception>(ex => ex is NullReferenceException) // Adjust exception type as needed
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            builder.Services.AddSingleton(retryPolicy);

            builder.Services.Configure<AiClientSettings>(builder.Configuration.GetSection("AiClientSettings"));
            builder.Services.AddScoped<AiClient>();

            builder.Services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
