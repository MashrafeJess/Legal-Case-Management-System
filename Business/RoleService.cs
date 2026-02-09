using System;
using System.Collections.Generic;
using System.Text;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business
{
    public class RoleService
    {
        private readonly LMSContext context = new();
        public async Task<Result> AddRole(Role role)
        {
            await context.Role.AddAsync(role);
            return new Result(true, "Role created successfully", role);
        }
        public async Task<Result> UpdateRole(Role Roles)
        {
            Role? entity = await context.Role.FirstOrDefaultAsync(u => u.RoleId == Roles.RoleId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity = Roles;
            context.Role.Update(entity);
            return Result.DBcommit(context, "User info updated successfully", null, entity);
        }
        public async Task<Result> DeleteRole(Role Roles)
        {
            Role? entity = await context.Role.FirstOrDefaultAsync(u => u.RoleId == Roles.RoleId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity.IsDeleted = true;
            context.Role.Update(entity);
            return Result.DBcommit(context, "User info updated successfully", null, entity);
        }
        public async Task<Result> AllRoles()
        {
            var list = await context.Role.ToListAsync();
            return new Result(true, "All Roles found", list);
        }
        public async Task<Result> RoleById(string RoleId)
        {
            Role? entity = await context.Role.FindAsync(RoleId);
            if (entity == null)
            {
                return new Result(false, "This is user is not found");
            }
            return new Result(true, "The user is found", entity);
        }



    }
}
