using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SchoolService.Databases;
using SchoolService.Repositories.StudyGroups;
using SchoolService.Repositories.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var studyGroupDb = builder.Services.AddDbContext<SchoolDb>(dbBuilder =>
{
    dbBuilder.UseInMemoryDatabase("test-db");
});

builder.Services.AddScoped<IStudyGroupRepository, StudyGroupRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add controllers and allow enums to be serialized as string
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Make routing lowercase
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Allow endpoint to listen to requests from anywhere
builder.Host.ConfigureHostOptions((builder, options) => builder.Configuration.Bind("0.0.0.0"));

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

//Expose program for testing
public partial class Program { }
