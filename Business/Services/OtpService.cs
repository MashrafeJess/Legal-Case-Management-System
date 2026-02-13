using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Database;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Http;

namespace Business.Services
{
    public class OTPService(LMSContext context)
    {
        private readonly LMSContext _context = context;

        public async Task<Result> CreateToken(string email)
        {
            Token token = new()
            {
                TokenId = OTPService.GenerateOtp(),
                Email = email
            };
            _context.Token.Add(token);
            return await Result.DBCommitAsync(_context, "Token Created Successfully", null, "Failed to Create Token", token);
        }

        public static string GenerateOtp()
        {
            int otp = RandomNumberGenerator.GetInt32(100000, 999999);
            return otp.ToString();
        }
    }
}