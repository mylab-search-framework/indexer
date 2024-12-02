using Indexer.Application;
using MyLab.WebErrors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(c => c.AddExceptionProcessing());
builder.Services.AddIndexerApplicationLogic();
builder.Services.ConfigureIndexerApplicationLogic(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program{}