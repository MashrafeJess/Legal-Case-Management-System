using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class FileService(LMSContext context, IWebHostEnvironment env)
    {
        private readonly LMSContext _context = context;
        private readonly IWebHostEnvironment _env = env;

        // Upload file
        public async Task<Result> UploadAsync(IFormFile file, string userId, int? caseId = null)
        {
            if (file == null || file.Length == 0)
                return new Result(false, "File is empty");

            var folder = GetFolderForFile(file.ContentType);
            var fullFolderPath = Path.Combine(_env.ContentRootPath, folder);
            Directory.CreateDirectory(fullFolderPath);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var savePath = Path.Combine(_env.ContentRootPath, "Uploads", fileName);

            // Save physical file
            await using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save metadata in DB
            var entity = new FileEntity
            {
                FileName = file.FileName,
                FilePath = savePath,
                ContentType = file.ContentType,
                Size = file.Length,
                CreatedBy = userId,
                CaseId = caseId ?? 0
            };

            _context.FileEntity.Add(entity);
            var result = await Result.DBCommitAsync(_context, "File uploaded", null, "Failed to save file metadata", entity);

            return result;
        }

        private static string GetFolderForFile(string contentType)
        {
            if (contentType.StartsWith("image/"))
                return "Uploads/Images";

            if (contentType == "application/pdf")
                return "Uploads/PDFs";

            // default folder
            return "Uploads/Others";
        }

        // List files for a case
        public async Task<Result> GetFilesAsync(int caseId)
        {
            var files = await _context.FileEntity
                .Include(f => f.CreatedBy) // join User table for CreatedBy
                .Where(f => f.CaseId == caseId)
                .Select(f => new
                {
                    f.FileId,
                    f.FileName,
                    f.ContentType,
                    f.Size,
                    f.CreatedDate,
                    CreatedBy = f.CreatedByUser!.UserName
                })
                .ToListAsync();

            return files.Count != 0
                ? new Result(true, "Files retrieved", files)
                : new Result(false, "No files found", files);
        }

        // Delete file (soft delete)
        public async Task<Result> DeleteFileAsync(int fileId)
        {
            var file = await _context.FileEntity.FindAsync(fileId);
            if (file == null) return new Result(false, "File not found");

            file.IsDeleted = true;
            _context.FileEntity.Update(file);
            return await Result.DBCommitAsync(_context, "File deleted");
        }
    }
}