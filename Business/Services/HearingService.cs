using System;
using System.Collections.Generic;
using System.Text;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class HearingService
    {
        private readonly LMSContext context = new();

        public async Task<Result> AddHearing(Hearing hearing)
        {
            await context.Hearing.AddAsync(hearing);
            return new Result(true, "Case created successfully", hearing);
        }

        public async Task<Result> UpdateHearing(Hearing hearing)
        {
            Hearing? entity = await context.Hearing.FirstOrDefaultAsync(u => u.HearingID == hearing.HearingID);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity = hearing;
            context.Hearing.Update(hearing);
            return await Result.DBCommitAsync(context, "User info updated successfully", null, data: entity);
        }

        public async Task<Result> DeleteCase(Hearing cases)
        {
            Hearing? entity = await context.Hearing.FirstOrDefaultAsync(u => u.CaseId == cases.CaseId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity.IsDeleted = true;
            context.Hearing.Update(entity);
            return await Result.DBCommitAsync(context, "User info updated successfully", null, data: entity);
        }

        public async Task<Result> AllHearing()
        {
            var list = await context.Hearing.ToListAsync();
            return new Result(true, "All cases found", list);
        }

        public async Task<Result> CaseById(string HearingId)
        {
            Hearing? entity = await context.Hearing.FindAsync(HearingId);
            if (entity == null)
            {
                return new Result(false, "This is user is not found");
            }
            return new Result(true, "The user is found", entity);
        }
    }
}