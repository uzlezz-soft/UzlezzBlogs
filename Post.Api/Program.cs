using Post.Api;
using UzlezzBlogs.Microservices.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureDatabaseContext<PostDbContext>(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddJwtAuth(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
