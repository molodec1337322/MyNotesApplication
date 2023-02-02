using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MyNotesApplication.Data;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using MyNotesApplication.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot _configString = new ConfigurationBuilder().SetBasePath(builder.Environment.ContentRootPath).AddJsonFile("DBConfig.json").Build();

builder.Services.AddControllers();
builder.Services.AddDbContext<MyDBContext>(options => options.UseNpgsql(_configString.GetConnectionString("PostgreSQLConnection")));
builder.Services.AddScoped<IRepository<Note>, NoteRepositoryPostgres>();
builder.Services.AddScoped<IRepository<User>, UserRepositoryPostgres>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();

app.Run();
