using System;
using System.Collections.Generic;
using System.Text;
using Business.DTO.Smtp;
using Database;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class SmtpService(LMSContext context)
    {
        private readonly LMSContext _context = context;

        public async Task<Result> AddSmtp(SmtpDto smtpConfig)
        {
            var entity = new SmtpSettings
            {
                Host = smtpConfig.Host,
                Port = smtpConfig.Port,
                Username = smtpConfig.Username,
                SenderEmail = smtpConfig.SenderEmail,
                Password = smtpConfig.Password,
            };
            await _context.SmtpSettings.AddAsync(entity);
            return await Result.DBCommitAsync(_context, "SMTP Configuration Added Successfully", null, "Failed to Add SMTP Configuration", entity);
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
            return new Result(true, "Smtp retrieved successfully", smtpConfig);
        }

        public async Task<Result> GetAll()
        {
            var list = await _context.SmtpSettings
               .Where(h => !h.IsDeleted)
               .AsNoTracking()
               .ToListAsync();
            if (list.Count == 0)
            {
                return new Result(false, "No Smtp Found");
            }
            return new Result(true, "All smtp found", list);
        }
    }
}