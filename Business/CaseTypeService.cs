using Database.Context;
using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Business
{
    public class CaseTypeService
    {
        private readonly LMSContext context = new();
        public async Task<Result> AddCase(CaseType type)
        {
            await context.CaseType.AddAsync(type);
            return new Result(true, "Case created successfully", type);
        }
        public async Task<Result> UpdateCase(CaseType type)
        {
            CaseType? entity = await context.CaseType.FirstOrDefaultAsync(u => u.CaseTypeId == type.CaseTypeId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity = type;
            context.CaseType.Update(entity);
            return Result.DBcommit(context, "User info updated successfully", null, entity);
        }
        public async Task<Result> DeleteCase(CaseType type)
        {
            CaseType? entity = await context.CaseType.FirstOrDefaultAsync(u => u.CaseTypeId == type.CaseTypeId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity.IsDeleted = true;
            context.CaseType.Update(entity);
            return Result.DBcommit(context, "User info updated successfully", null, entity);
        }
        public async Task<Result> AllCases()
        {
            var list = await context.CaseType.ToListAsync();
            return new Result(true, "All cases found", list);
        }
        public async Task<Result> CaseTypeById(string CaseId)
        {
            CaseType? entity = await context.CaseType.FindAsync(CaseId);
            if (entity == null)
            {
                return new Result(false, "This is user is not found");
            }
            return new Result(true, "The user is found", entity);
        }



    }
}
