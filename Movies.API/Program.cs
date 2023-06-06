using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Api.Health;
using Movies.Api.Swagger;
using Movies.API.Mapping;
using Movies.Application;
using Movies.Application.Database;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var config = builder.Configuration;

builder.Services.AddAuthentication(x =>
{
	x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
	x.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
		ValidateLifetime = true,
		ValidIssuer = config["Jwt:Issuer"],
		ValidAudience = config["Jwt:Audience"],
		ValidateIssuer = true,
		ValidateAudience = true,

	};
});

builder.Services.AddAuthorization(x =>
{
	/*x.AddPolicy(AuthConstants.ADMINUSERPOLICYNAME, 
		p => p.RequireClaim(AuthConstants.ADMINUSERCLAIMNAME, "true"));*/

	x.AddPolicy(AuthConstants.ADMINUSERPOLICYNAME,
		p => p.AddRequirements(new AdminAuthRequirement(config["ApiKey"]!)));

	x.AddPolicy(AuthConstants.TRUSTEDMEMBERPOLICYNAME,
		p => p.RequireAssertion(c =>
		c.User.HasClaim(m => m is { Type: AuthConstants.ADMINUSERCLAIMNAME, Value: "true"}) ||
		c.User.HasClaim(m => m is { Type: AuthConstants.TRUSTEDMEMBERCLAIMNAME, Value: "true"})

		));
});

builder.Services.AddScoped<ApiKeyAuthFilter>();

builder.Services.AddApiVersioning(x =>
{
	x.DefaultApiVersion = new ApiVersion(1.0);
	x.AssumeDefaultVersionWhenUnspecified = true;
	x.ReportApiVersions = true;
	x.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
}).AddMvc().AddApiExplorer();

//builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(x =>
{
	x.AddBasePolicy(c => c.Cache());
	x.AddPolicy("MovieCache", c => 
		c.Cache()
		 .Expire(TimeSpan.FromSeconds(30))
		 .SetVaryByQuery(new[] { "title", "year", "sortBy", "page", "pageSize" })
		 .Tag("movies"));
});

builder.Services.AddControllers();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name);
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();


builder.Services.AddSwaggerGen(x => x.OperationFilter<SwaggerDefaultValues>());
builder.Services.AddApplication();
builder.Services.AddDatabase(config["Database:ConnectionString"]!);

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(x =>
	{
		foreach(var description in app.DescribeApiVersions())
		{
			x.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
				description.GroupName);
		}
	});
}

app.MapHealthChecks("_health");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

/*app.UseCors();
app.UseResponseCaching();*/
app.UseOutputCache();

app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();
