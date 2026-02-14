using System;
using System.Collections.Generic;
using System.Text;
using Business.DTO.Comment;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class CommentService(LMSContext context)
    {
        private readonly LMSContext _context = context;

        public async Task<Result> Create(CommentDto comment)
        {
            var entity = new Comment
            {
                CommentText = comment.CommentText,
                UserId = comment.UserId,
                CaseId = comment.CaseId
            };
            await _context.Comment.AddAsync(entity);
            var result = await Result.DBCommitAsync(_context, "Comment Created", null, "Creation Failed", entity);
            return result;
        }

        public async Task<Result> Update(Comment comment)
        {
            Comment? entity = await _context.Comment.FirstOrDefaultAsync(u => u.CommentId == comment.CommentId);
            if (entity == null)
            {
                return new Result(false, "No comment found");
            }
            entity = comment;
            _context.Comment.Update(entity);
            return await Result.DBCommitAsync(_context, "Comment updated successfully", null, data: entity);
        }

        public async Task<Result> Delete(int commentId)
        {
            Comment? entity = await _context.Comment.FirstOrDefaultAsync(u => u.CommentId == commentId);
            if (entity == null)
            {
                return new Result(false, "No comment found");
            }
            entity.IsDeleted = true;
            _context.Comment.Update(entity);
            return await Result.DBCommitAsync(_context, "Comment updated successfully", null, data: entity);
        }

        public async Task<Result> Get(int caseId) // for particular case
        {
            var list = await _context.Comment
                                    .Where(c => c.CaseId == caseId && !c.IsDeleted)
                                    .ToListAsync();

            return new Result(true, "All comments found for that comment", list);
        }

        public async Task<Result> GetById(int commentId)
        {
            Comment? entity = await _context.Comment.FindAsync(commentId);
            if (entity == null)
            {
                return new Result(false, "This comment is not found");
            }
            return new Result(true, "The comment is found", entity);
        }
    }
}