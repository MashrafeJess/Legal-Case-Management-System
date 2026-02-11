using System;
using System.Collections.Generic;
using System.Text;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class PaymentMethodService
    {
        private readonly LMSContext context = new();

        public async Task<Result> AddPaymentMethod(PaymentMethod method)
        {
            await context.PaymentMethod.AddAsync(method);
            return new Result(true, "Case created successfully", method);
        }

        public async Task<Result> UpdatePaymentMethod(PaymentMethod method)
        {
            PaymentMethod? entity = await context.PaymentMethod.FirstOrDefaultAsync(u => u.PaymentMethodId == method.PaymentMethodId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity = method;
            context.PaymentMethod.Update(entity);
            return await Result.DBCommitAsync(context, "User info updated successfully", null, data: entity);
        }

        public async Task<Result> DeletePaymentMethod(PaymentMethod method)
        {
            PaymentMethod? entity = await context.PaymentMethod.FirstOrDefaultAsync(u => u.PaymentMethodId == method.PaymentMethodId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity.IsDeleted = true;
            context.PaymentMethod.Update(entity);
            return await Result.DBCommitAsync(context, "User info updated successfully", null, data: entity);
        }

        public async Task<Result> AllPaymentMethods()
        {
            var list = await context.PaymentMethod.ToListAsync();
            return new Result(true, "All cases found", list);
        }

        public async Task<Result> PaymentMethodById(string PaymentMethodId)
        {
            PaymentMethod? entity = await context.PaymentMethod.FindAsync(PaymentMethodId);
            if (entity == null)
            {
                return new Result(false, "This is user is not found");
            }
            return new Result(true, "The user is found", entity);
        }
    }
}