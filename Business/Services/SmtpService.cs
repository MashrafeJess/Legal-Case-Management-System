using System;
using System.Collections.Generic;
using System.Text;
using Database;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Http;

namespace Business.Services
{
    public class SmtpService
    {
        private readonly LMSContext Context = new();

        public async Task<Result> AddSmtp(SmtpSettings smtpConfig)
        {
            Context.SmtpSettings.Add(smtpConfig);
            return await Result.DBCommitAsync(Context, "SMTP Configuration Added Successfully", null, "Failed to Add SMTP Configuration", data: smtpConfig);
        }

        public async Task<Result> UpdateSmtp(SmtpSettings smtpConfig)
        {
            var existingConfig = Context.SmtpSettings.Find(smtpConfig.SmtpId);
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
            return await Result.DBCommitAsync(Context, "SMTP Configuration Updated Successfully", null, "Failed to Update SMTP Configuration", data: smtpConfig);
        }

        public Result DeleteSmtp(SmtpSettings smtpConfig)
        {
            SmtpSettings? s = Context.SmtpSettings.Find(smtpConfig.SmtpId);
            if (s == null)
            {
                return new Result(false, "nothing found");
            }
            s.IsDeleted = true;
            Context.SmtpSettings.Update(s);
            return new Result(true, "Smtp Settings updated", s);
        }

        public Result GetSmtp(int id)
        {
            var smtpConfig = Context.SmtpSettings.Find(id);
            if (smtpConfig == null)
            {
                return new Result { Success = false, Message = "SMTP Configuration Not Found" };
            }
            return new Result { Success = true, Message = "SMTP Configuration Retrieved Successfully", Data = smtpConfig };
        }
    }
}