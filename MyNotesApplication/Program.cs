using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using MyNotesApplication.Data.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyNotesApplication.Services;
using Microsoft.Extensions.Configuration;
using MyNotesApplication.Services.Interfaces;
using MyNotesApplication.Services.RabbitMQBroker;
using RabbitMQ.Client;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";


var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile("client_secret.json");

IConfigurationRoot _DBconfigString = new ConfigurationBuilder().SetBasePath(builder.Environment.ContentRootPath).AddJsonFile("DBConfig.json").Build();
IConfiguration _configuration = builder.Configuration;

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .WithOrigins(_configuration.GetValue<string>("FrontRedirectUrl"));
                      });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddControllers();

builder.Services.AddDbContext<MyDBContext>(options => options.UseNpgsql(_DBconfigString.GetConnectionString("PostgreSQLConnection")));

builder.Services.AddScoped<IRepository<Note>, NoteRepositoryPostgres>();
builder.Services.AddScoped<IRepository<User>, UserRepositoryPostgres>();
builder.Services.AddScoped<IRepository<ConfirmationToken>, ConfirmationTokenPostgres>();
builder.Services.AddScoped<IRepository<FileModel>, FileModelRepositoryPostgres>();
builder.Services.AddScoped<IRepository<Board>, BoardRepositoryPostgres>();
builder.Services.AddScoped<IRepository<UserBoardRole>, UserBoardRoleRepositoryPostgres>();
builder.Services.AddScoped<IRepository<Column>, ColumnRepositoryPostgres>();
builder.Services.AddScoped<IRepository<InvitationToken>, InvitationTokenRepositoryPostgres>();

builder.Services.AddScoped<IMessageBrokerPersistentConnection, PersistentConnectionRabbitMQ>();
builder.Services.AddScoped<IMessageBroker, MessageBrokerRabbitMQ>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();

public class AuthOptions
{
    public const string ISSUER = "MyNotesApplicationServer"; // издатель токена
    public const string AUDIENCE = "MyNotesApplicationClient"; // потребитель токена
    const string KEY = "miimuvuyimntuxyrdtx";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}
