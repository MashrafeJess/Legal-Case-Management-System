using System;
using System.Collections.Generic;
using System.Text;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business
{
    public class PaymentService
    {
        private readonly LMSContext context = new();
        public async Task<Result> AddPayment(Payment payment)
        {
            await context.Payment.AddAsync(payment);
            return new Result(true, "Case created successfully", payment);
        }
        public async Task<Result> UpdatePayment(Payment payment)
        {
            Payment? entity = await context.Payment.FirstOrDefaultAsync(u => u.PaymentId == payment.PaymentId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity = payment;
            context.Payment.Update(entity);
            return Result.DBcommit(context, "User info updated successfully", null, entity);
        }
        public async Task<Result> DeletePayment(Payment payment)
        {
            Payment? entity = await context.Payment.FirstOrDefaultAsync(u => u.PaymentId == payment.PaymentId);
            if (entity == null)
            {
                return new Result(false, "No user found");
            }
            entity.IsDeleted = true;
            context.Payment.Update(entity);
            return Result.DBcommit(context, "User info updated successfully", null, entity);
        }
        public async Task<Result> AllCases()
        {
            var list = await context.Payment.ToListAsync();
            return new Result(true, "All cases found", list);
        }
        public async Task<Result> CaseById(string PaymentId)
        {
            Case? entity = await context.Case.FindAsync(PaymentId);
            if (entity == null)
            {
                return new Result(false, "This is user is not found");
            }
            return new Result(true, "The user is found", entity);
        }



    }
}
