using System.Security.Claims;
using Business.DTO.Case;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class CaseService(LMSContext context, FileService service, IHttpContextAccessor accessor, ILogger<CaseService> logger)
    {
        private readonly LMSContext _context = context;
        private readonly FileService _fileService = service;
        private readonly IHttpContextAccessor _accessor = accessor;
        private readonly ILogger<CaseService> _logger = logger;

        public async Task<Result> AddCase(CreateCaseDto cases)
        {
            var entity = new Case
            {
                CaseName = cases.CaseName,
                CaseType = cases.CaseType,
                Email = cases.Email,
                Fee = cases.Fee,
                CreatedBy = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier),
                CaseHandlingBy = cases.CaseHandlingBy,
            };
            _context.Case.Add(entity);
            var result = await Result.DBCommitAsync(_context, "Case Created Successfully", _logger, "Failed to create result", entity);
            if (!result.Success)
            {
                return result;
            }

            if (cases.FormFiles!.Count > 0)
            {
                foreach (var file in cases.FormFiles)
                {
                    result = await _fileService.UploadAsync(file, entity.CreatedBy!, entity.CaseId);
                    if (!result.Success) return result;
                }
            }
            return new Result(true, "Case Created Successfully with files", entity);
        }

        public async Task<Result> UpdateCase(CaseDto cases)
        {
            if (cases == null)
                return new Result(false, "CaseDto was Null");

            Case? entity = await _context.Case.FirstOrDefaultAsync(u => u.CaseId == cases.CaseId && !u.IsDeleted);
            if (entity == null)
                return new Result(false, "Case not found");

            // Only update if value is provided — keeps old data if field is empty ✅
            if (!string.IsNullOrWhiteSpace(cases.CaseName))
                entity.CaseName = cases.CaseName;

            if (!string.IsNullOrWhiteSpace(cases.CaseHandlingBy))
                entity.CaseHandlingBy = cases.CaseHandlingBy;

            if (cases.CaseType != 0)
                entity.CaseType = cases.CaseType;

            if (!string.IsNullOrWhiteSpace(cases.Email))
                entity.Email = cases.Email;

            if (cases.Fee != 0)
                entity.Fee = cases.Fee;

            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Case.Update(entity);
            var result = await Result.DBCommitAsync(_context, "Case updated successfully", _logger, data: entity);

            if (!result.Success)
                return result;

            return new Result(true, "Case updated successfully", entity);
        }

        public async Task<Result> DeleteCase(int caseId)
        {
            Case? entity = _context.Case.FirstOrDefault(u => u.CaseId == caseId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Case.Update(entity);
            return await Result.DBCommitAsync(_context, "Case Deleted Successfully");
        }

        public async Task<Result> AllCases()
        {
            var list = await _context.Case
                        .Include(u => u.Type)
                        .Where(u => !u.IsDeleted)
                        .Select(u => new CaseDto
                        {
                            CaseId = u.CaseId,
                            CaseHandlingBy = u.CaseHandlingByUser!.UserName,
                            TypeName = u.Type!.CaseTypeName,
                            Email = u.Email,
                            Fee = u.Fee,
                        })
                        .AsNoTracking()
                        .ToListAsync();
            if (list.Count > 0)
            {
                return new Result(true, "All cases found", list);
            }
            return new Result(false, "Case Fetch Failed");
        }

        public async Task<Result> CaseById(int caseId)
        {
            var entity = await _context.Case
                .Where(u => u.CaseId == caseId && !u.IsDeleted)
                .Select(u => new
                {
                    u.CaseId,
                    u.CaseName,
                    HandlingBy = u.CaseHandlingByUser!.UserName,
                    u.Type!.CaseTypeName,
                    u.Fee,
                    u.CreatedDate,
                    u.UpdatedDate,

                    // Join to User table inline — single query, no null issues
                    CreatedBy = _context.User
                        .Where(c => c.UserId == u.CreatedBy)
                        .Select(c => c.UserName)
                        .FirstOrDefault() ?? "Unknown",

                    UpdatedBy = _context.User
                        .Where(c => c.UserId == u.UpdatedBy)
                        .Select(c => c.UserName)
                        .FirstOrDefault() ?? "Unknown",

                    Files = u.Files!
                        .Where(f => !f.IsDeleted)
                        .Select(f => new
                        {
                            f.FileId,
                            f.FileName,
                            f.Description,
                            f.FilePath,
                            f.ContentType,
                            f.Size,
                            f.CreatedDate,
                            f.UpdatedDate,

                            CreatedBy = _context.User
                                .Where(c => c.UserId == f.CreatedBy)
                                .Select(c => c.UserName)
                                .FirstOrDefault() ?? "Unknown",

                            UpdatedBy = _context.User
                                .Where(c => c.UserId == f.UpdatedBy)
                                .Select(c => c.UserName)
                                .FirstOrDefault() ?? "Unknown",
                        }).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (entity == null)
                return new Result(false, "Case not found");

            return new Result(true, "Case found", entity);
        }
    }
}