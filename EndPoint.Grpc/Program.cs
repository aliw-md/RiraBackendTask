using Application.Interfaces;
using Application.Services;
using Application.Validation;
using Domain.Entities.Person;
using EndPoint.Grpc.Services;
using FluentValidation;
using Persistence.Repository;

namespace EndPoint.Grpc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5079, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
            });

            // Add services to the container.
            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<ErrorHandlingInterceptor>();
            });

            builder.Services.AddScoped<IPersonRepository, FilePersonRepository>();
            builder.Services.AddScoped<IPersonService,PersonService>();
            builder.Services.AddScoped<IValidator<Person>, PersonValidator>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<PersonGrpcService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}