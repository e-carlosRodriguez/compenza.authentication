using Serilog;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using compenza.authentication.application;
using compenza.authentication.application.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var compenzaOriginPolicy = "CompenzaPortal";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: compenzaOriginPolicy, policy => {
        policy.WithOrigins("*");
        policy.WithMethods("*");
        policy.WithHeaders("*");
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetSection("Settings:TokenSecret").Value)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

var app = builder.Build();
var local = app.Environment.IsDevelopment();

app.UseSwagger();
app.UseSwaggerUI( options =>
{
    var iisRoute = "/ApiCompenza/swagger/v1/swagger.json";
    var localRoute = "/swagger/v1/swagger.json";
    options.SwaggerEndpoint( local ? localRoute : iisRoute ,"Authentication");
});

app.UseCors(compenzaOriginPolicy);
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();
