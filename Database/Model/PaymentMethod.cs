using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Database.Model
{
    public class PaymentMethod : BaseModel
    {
        [Key]
        public int PaymentMethodId { get; set; }
        [StringLength(50)]
        public string PaymentMethodName { get; set; } = string.Empty;
        public bool PaymentStatus { get; set; }


    }
}
