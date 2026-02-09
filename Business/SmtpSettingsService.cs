using System;
using System.Collections.Generic;
using System.Text;
using Database;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Http;

namespace Business
{
    public class SmtpService
    {
        private readonly LMSContext Context = new();
        public Result AddSmtp(SmtpSettings smtpConfig)
        {
            Context.SmtpSettings.Add(smtpConfig);
            return Result.DBcommit(Context, "SMTP Configuration Added Successfully", "Failed to Add SMTP Configuration");
        }
        public Result UpdateSmtp(SmtpSettings smtpConfig)
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
            return Result.DBcommit(Context, "SMTP Configuration Updated Successfully", "Failed to Update SMTP Configuration");
        }
        public Result DeleteSmtp(SmtpSettings smtpConfig)
        {
            SmtpSettings s = Context.SmtpSettings.Find(smtpConfig.SmtpId);
            if(s==null)
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