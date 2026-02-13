using System;
using System.Collections.Generic;
using System.Text;
using Database;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Http;

namespace Business.Services
{
    public class SmtpService(LMSContext context)
    {
        private readonly LMSContext _context = context;

        public async Task<Result> AddSmtp(SmtpSettings smtpConfig)
        {
            await _context.SmtpSettings.AddAsync(smtpConfig);
            return await Result.DBCommitAsync(_context, "SMTP Configuration Added Successfully", null, "Failed to Add SMTP Configuration", data: smtpConfig);
        }

        public async Task<Result> UpdateSmtp(SmtpSettings smtpConfig)
        {
            var existingConfig = _context.SmtpSettings.Find(smtpConfig.SmtpId);
            if (existingConfig == null)
            {
                return new Result { Success = false, Message = "SMTP Configuration Not Found" };
            }
            existingConfig.Host = smtpConfig.Host;
            existingConfig.Port = smtpConfig.Port;
            existingConfig.Username = smtpConfig.Username;
            existingConfig.Password = smtpConfig.Password;
            existingConfig.EnableSsl = smtpConfig.EnableSsl;
            existingConfig.SenderEmail = smtpConfig.SenderEmail;
            return await Result.DBCommitAsync(_context, "SMTP Configuration Updated Successfully", null, "Failed to Update SMTP Configuration", data: smtpConfig);
        }

        public async Task<Result> DeleteSmtp(SmtpSettings smtpConfig)
        {
            SmtpSettings? s = _context.SmtpSettings.Find(smtpConfig.SmtpId);
            if (s == null)
            {
                return new Result(false, "nothing found");
            }
            s.IsDeleted = true;
            _context.SmtpSettings.Update(s);
            return await Result.DBCommitAsync(_context, "SmtpUpdated", null, "Failed to Update", s);
        }

        public async Task<Result> GetSmtp(int id)
        {
            var smtpConfig = await _context.SmtpSettings.FindAsync(id);
            if (smtpConfig == null)
            {
                return new Result { Success = false, Message = "SMTP Configuration Not Found" };
            }
            return new Result { Success = true, Message = "SMTP Configuration Retrieved Successfully", Data = smtpConfig };
        }
    }
}