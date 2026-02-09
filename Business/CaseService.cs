using System;
using System.Collections.Generic;
using System.Text;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business
{
    public class CaseService
    {
        public LMSContext context = new();
        public Result AddCase(Case cases)
        {
             context.Case.Add(cases);
                return new Result(true, "Case created successfully",cases);
        }
        public Result UpdateCase(Case cases)
        {
            Case? entity =  context.Case.FirstOrDefault(u => u.CaseId == cases.CaseId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity = cases;
            context.Case.Update(entity);
            return Result.DBcommit(context, "User info updated successfully", null, entity);
        }
        public Result DeleteCase(Case cases)
        {
            Case? entity =  context.Case.FirstOrDefault(u => u.CaseId == cases.CaseId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity.IsDeleted = true;
            context.Case.Update(entity);
            return Result.DBcommit(context, "User info updated successfully", null, entity);
        }
        public Result AllCases()
        {
            var list =  context.Case.ToList();
            return new Result(true, "All cases found", list);
        }
        public Result CaseById(int CaseId)
        {
            Case? entity =  context.Case.Find(CaseId);
            if (entity == null)
            {
                return new Result(false, "This is user is not found");
            }
            return new Result(true, "The user is found", entity);
        }



    }
}
