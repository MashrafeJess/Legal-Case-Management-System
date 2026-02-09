using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Database.Model
{
    public class Payment : BaseModel
    {
        [Key]
        public string PaymentId { get; set; } = string.Empty;
        public int Amount { get; set; } 
        public int PaymentMethodId { get; set; }
    }
}
