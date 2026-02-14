using Database.Context;
using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class CaseTypeService(LMSContext context)
    {
        private readonly LMSContext _context = context;

        public async Task<Result> Create(CaseType type)
        {
            await _context.CaseType.AddAsync(type);

            return await Result.DBCommitAsync(_context, "Case Type created Successfully", null, "Failed to save", type);
        }

        public async Task<Result> Update(CaseType type)
        {
            CaseType? entity = await _context.CaseType.FirstOrDefaultAsync(u => u.CaseTypeId == type.CaseTypeId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity = type;
            _context.CaseType.Update(entity);
            return await Result.DBCommitAsync(_context, "User info updated successfully", null, data: entity);
        }

        public async Task<Result> Delete(int typeId)
        {
            CaseType? entity = await _context.CaseType.FirstOrDefaultAsync(u => u.CaseTypeId == typeId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity.IsDeleted = true;
            _context.CaseType.Update(entity);
            return await Result.DBCommitAsync(_context, "User info updated successfully", null, data: entity);
        }

        public async Task<Result> GetAll()
        {
            var list = await _context.CaseType.ToListAsync();
            return new Result(true, "All cases found", list);
        }

        public async Task<Result> GetById(int typeId)
        {
            CaseType? entity = await _context.CaseType.FindAsync(typeId);
            if (entity == null)
            {
                return new Result(false, "This is user is not found");
            }
            return new Result(true, "The user is found", entity);
        }
    }
}