using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using MyNotesApplication.Data.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyNotesApplication.Services;

var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot _DBconfigString = new ConfigurationBuilder().SetBasePath(builder.Environment.ContentRootPath).AddJsonFile("DBConfig.json").Build();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddControllers();

builder.Services.AddDbContext<MyDBContext>(options => options.UseNpgsql(_DBconfigString.GetConnectionString("PostgreSQLConnection")));
builder.Services.AddScoped<IRepository<Note>, NoteRepositoryPostgres>();
builder.Services.AddScoped<IRepository<User>, UserRepositoryPostgres>();
builder.Services.AddScoped<IRepository<ConfirmationToken>, ConfirmationTokenPostgres>();

builder.Services.AddScoped<EmailService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = AuthOptions.ISSUER,
        ValidateAudience = true,
        ValidAudience = AuthOptions.AUDIENCE,
        ValidateLifetime = true,
        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSession();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public class AuthOptions
{
    public const string ISSUER = "MyNotesApplicationServer"; // издатель токена
    public const string AUDIENCE = "MyNotesApplicationClient"; // потребитель токена
    const string KEY = "miimuvuyimntuxyrdtx";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}
