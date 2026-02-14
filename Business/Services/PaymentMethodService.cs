using System;
using System.Collections.Generic;
using System.Text;
using Business.DTO.PaymentMethod;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class PaymentMethodService(LMSContext context)
    {
        private readonly LMSContext _context = context;

        public async Task<Result> AddPaymentMethod(MethodDto method)
        {
            var entity = new PaymentMethod
            {
                PaymentMethodName = method.PaymentMethodName
            };
            await _context.PaymentMethod.AddAsync(entity);
            var result = await Result.DBCommitAsync(_context, "This Payment Method created Successfully", null, "Failed to create", entity);
            return result;
        }

        public async Task<Result> UpdatePaymentMethod(PaymentMethod method)
        {
            PaymentMethod? entity = await _context.PaymentMethod.FirstOrDefaultAsync(u => u.PaymentMethodId == method.PaymentMethodId);
            if (entity == null)
            {
                return new Result(false, "No Payment Method found");
            }
            entity = method;
            _context.PaymentMethod.Update(entity);
            return await Result.DBCommitAsync(_context, "Payment Method updated successfully", null, data: entity);
        }

        public async Task<Result> DeletePaymentMethod(int methodId)
        {
            PaymentMethod? entity = await _context.PaymentMethod.FirstOrDefaultAsync(u => u.PaymentMethodId == methodId);
            if (entity == null)
            {
                return new Result(false, "No Method found");
            }
            entity.IsDeleted = true;
            _context.PaymentMethod.Update(entity);
            return await Result.DBCommitAsync(_context, "Method updated successfully", null, data: entity);
        }

        public async Task<Result> AllPaymentMethods()
        {
            var list = await _context.PaymentMethod.ToListAsync();
            return new Result(true, "All methods found", list);
        }

        public async Task<Result> PaymentMethodById(int paymentMethodId)
        {
            PaymentMethod? entity = await _context.PaymentMethod.FindAsync(paymentMethodId);
            if (entity == null)
            {
                return new Result(false, "This is method is not found");
            }
            return new Result(true, "The method is found", entity);
        }
    }
}