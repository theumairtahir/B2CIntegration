using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen(
  c =>
  {
    c.SwaggerDoc("v1", new() { Title = "B2CIntegration.Server", Version = "v1" });
    c.AddSecurityDefinition("oauth2.", new OpenApiSecurityScheme
    {
      Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
      Name = "OAuth2.0",
      Type = SecuritySchemeType.OAuth2,
      In = ParameterLocation.Header,
      Flows = new OpenApiOAuthFlows
      {
        Implicit = new OpenApiOAuthFlow
        {
          AuthorizationUrl = new Uri(builder.Configuration["SwaggerAzureAd:AuthorizeUrl"]!),
          TokenUrl = new Uri(builder.Configuration["SwaggerAzureAd:TokenUrl"]!),
          Scopes = new Dictionary<string, string>
          {
            { builder.Configuration["SwaggerAzureAd:Scope"]!, "Access as User" },
          }
        }
      }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "oauth2.0"
          }
        },
        [builder.Configuration["SwaggerAzureAd:Scope"]!]
      }
    });
  });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(x =>
  {
    x.OAuthClientId(builder.Configuration["SwaggerAzureAd:ClientId"]);
    x.OAuthUsePkce();
    x.OAuthScopeSeparator(".");
  });
}
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
