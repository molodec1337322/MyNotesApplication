﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyNotesApplication.Controllers
{
    [Route("api/Files")]
    public class FilesController : Controller
    {
        private readonly IRepository<FileModel> _fileModelRepository;
        private readonly IRepository<Note> _notesRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IWebHostEnvironment _appEnviroment;
        private readonly IConfiguration _appConfiguration;

        
        public FilesController(IRepository<FileModel> fileModelRepo, IRepository<Note> notesRepo, IRepository<User> userRepo,IWebHostEnvironment appEnviroment, IConfiguration appConfiguration)
        {
            _fileModelRepository = fileModelRepo;
            _notesRepository = notesRepo;
            _userRepository = userRepo;
            _appEnviroment = appEnviroment;
            _appConfiguration = appConfiguration;
        }

        [HttpPost]
        [Authorize]
        [Route("Upload/{NoteId}")]
        public async Task UploadFile(List<IFormFile> files, int NoteId)
        {
            var username = GetUsernameFromJwtToken();

            if (_notesRepository.Get(NoteId)?.UserId == _userRepository.GetAll().FirstOrDefault(u => u.Username == username)?.Id)
            {
                try
                {
                    long size = files.Sum(f => f.Length);

                    var fileDirectory = Path.Combine(_appEnviroment.WebRootPath, _appConfiguration.GetValue<string>("FilesStorageFolder"));
                    if (!Directory.Exists(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }

                    foreach (var formFile in files)
                    {
                        if (formFile.Length > 0)
                        {
                            var fileType = formFile.ContentType.Split("/")[1];
                            var newFileName = Path.GetRandomFileName();
                            newFileName = newFileName.Split(".")[0] +  "." + fileType;

                            var filePath = Path.Combine(fileDirectory, newFileName);
                            using (var stream = System.IO.File.Create(filePath))
                            {
                                await formFile.CopyToAsync(stream);
                            }

                            FileModel newFile = new FileModel();
                            newFile.Name = newFileName;
                            newFile.NoteId = NoteId;
                            newFile.Path = filePath;
                            newFile.Format = fileType;

                            _fileModelRepository.Add(newFile);
                        }
                    }
                    await _fileModelRepository.SaveChanges();

                    await HttpContext.Response.WriteAsJsonAsync(new { message = "Uploaded" });
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                    await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message });
                }
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { error = "noteNotFound" });
            }
        }

        [HttpGet]
        [Authorize]
        [Route("Download/{FileId}")]
        public async Task DownloadFile(int FileId)
        {
            var username = GetUsernameFromJwtToken();

            FileModel fileModel = _fileModelRepository.Get(FileId);

            if (_notesRepository.Get(fileModel.NoteId)?.UserId == _userRepository.GetAll().FirstOrDefault(u => u.Username == username)?.Id)
            {
                
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { error = "noteNotFound" });
            }
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete/{FileId}")]
        public async Task DeleteFile(int FileId)
        {
            var username = GetUsernameFromJwtToken();

            FileModel fileModel = _fileModelRepository.Get(FileId);

            if (_notesRepository.Get(fileModel.NoteId)?.UserId == _userRepository.GetAll().FirstOrDefault(u => u.Username == username)?.Id)
            {

            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { error = "noteNotFound" });
            }
        }

        private string GetUsernameFromJwtToken()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            token = token.ToString().Split(" ")[1];
            return new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name).Value;
        }
    }
}
