using System;
using System.Collections.Generic;
using System.Text;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class CommentService
    {
        private readonly LMSContext context = new();

        public async Task<Result> AddCase(Comment comment)
        {
            await context.Comment.AddAsync(comment);
            return new Result(true, "Case created successfully", comment);
        }

        public async Task<Result> UpdateCase(Comment comment)
        {
            Comment? entity = await context.Comment.FirstOrDefaultAsync(u => u.CommentId == comment.CommentId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity = comment;
            context.Comment.Update(entity);
            return await Result.DBCommitAsync(context, "User info updated successfully", null, data: entity);
        }

        public async Task<Result> DeleteCase(Case cases)
        {
            Case? entity = await context.Case.FirstOrDefaultAsync(u => u.CaseId == cases.CaseId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity.IsDeleted = true;
            context.Case.Update(entity);
            return await Result.DBCommitAsync(context, "User info updated successfully", null, data: entity);
        }

        public async Task<Result> AllComments(int caseId)
        {
            var list = await context.Comment
                                    .Where(c => c.CaseId == caseId)
                                    .ToListAsync();

            return new Result(true, "All comments found", list);
        }

        public async Task<Result> CommentById(int commentId)
        {
            Case? entity = await context.Case.FindAsync(commentId);
            if (entity == null)
            {
                return new Result(false, "This is user is not found");
            }
            return new Result(true, "The user is found", entity);
        }
    }
}