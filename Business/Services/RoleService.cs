using System;
using System.Collections.Generic;
using System.Text;
using Business.DTO.Role;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class RoleService(LMSContext context)
    {
        private readonly LMSContext _context = context;

        public async Task<Result> AddRole(RoleDto role)
        {
            var entity = new Role
            {
                RoleName = role.RoleName
            };
            await _context.Role.AddAsync(entity);
            var result = await Result.DBCommitAsync(_context, "This Role created Successfully", null, "Failed to create", entity);
            return result;
        }

        public async Task<Result> UpdateRole(Role Roles)
        {
            Role? entity = await _context.Role.FirstOrDefaultAsync(u => u.RoleId == Roles.RoleId);
            if (entity == null)
            {
                return new Result(false, "No Role found");
            }
            entity = Roles;
            _context.Role.Update(entity);
            return await Result.DBCommitAsync(_context, "Role updated successfully", null, data: entity);
        }

        public async Task<Result> DeleteRole(int roleId)
        {
            Role? entity = await _context.Role.FirstOrDefaultAsync(u => u.RoleId == roleId);
            if (entity == null)
            {
                return new Result(false, "No Role found");
            }
            entity.IsDeleted = true;
            _context.Role.Update(entity);
            return await Result.DBCommitAsync(_context, "Role updated successfully", null, data: entity);
        }

        public async Task<Result> AllRoles()
        {
            var list = await _context.Role.ToListAsync();
            return new Result(true, "All Roles found", list);
        }

        public async Task<Result> RoleById(int RoleId)
        {
            Role? entity = await _context.Role.FindAsync(RoleId);
            if (entity == null)
            {
                return new Result(false, "This role is not found");
            }
            return new Result(true, "The role is found", entity);
        }
    }
}