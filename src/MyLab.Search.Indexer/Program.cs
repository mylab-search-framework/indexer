using FluentValidation;
using Indexer.Domain.Model;
using Indexer.Domain.Validators;
using MyLab.WebErrors;
using IndexingObject = MyLab.Search.Indexer.Model.IndexingObject;
using IndexingObjectValidator = MyLab.Search.Indexer.Validators.IndexingObjectValidator;
using LiteralId = MyLab.Search.Indexer.Model.LiteralId;
using LiteralIdValidator = MyLab.Search.Indexer.Validators.LiteralIdValidator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(opt => opt.AddExceptionProcessing());

AddControllerParamsValidators();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();


void AddControllerParamsValidators()
{
    builder.Services
        .AddScoped<IValidator<LiteralId>, LiteralIdValidator>()
        .AddScoped<IValidator<IndexingObject>, IndexingObjectValidator>();
}