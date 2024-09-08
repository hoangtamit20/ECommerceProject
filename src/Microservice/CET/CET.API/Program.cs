using CET.Service;
using Microsoft.AspNetCore.Mvc;
using Core.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCETServices();

builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(cor =>
{
    cor.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.MapJwtRevocation();
app.UseAuthorization();
app.MapRuntimeContext();
app.MapControllers();

app.Run();