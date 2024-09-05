using Core.Domain;
using CET.Service;
using Microsoft.AspNetCore.Mvc;
using Core.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCETServices();
// DateTimeOffset timeOffset = DateTimeOffset.UtcNow;

// var local = timeOffset.ToOffset(TimeZoneInfo.Local.GetUtcOffset(timeOffset.DateTime));

// var a= RuntimeContext.AppSettings.ConnectionStrings.CET_Connection;

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
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapJwtRevocation();
app.Run();