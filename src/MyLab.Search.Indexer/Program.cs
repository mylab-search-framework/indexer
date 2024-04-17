using FluentValidation;
using MyLab.Search.Indexer.Handlers;
using MyLab.Search.Indexer.MqConsuming;
using MyLab.WebErrors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>())
    .AddAutoMapper
    (
        c =>
        {
            c.AddProfile(new IndexerMqMessageMappingProfile());
            c.AddProfile(new CqrsCommandMappingProfile());
        }
    )
    .AddValidatorsFromAssemblyContaining<Program>()
    .AddRabbit()
    .AddRabbitConsumers<IndexerMqConsumerRegistrar>()
    .AddControllers(opt => opt.AddExceptionProcessing());

builder.Services
    .ConfigureRabbit(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
