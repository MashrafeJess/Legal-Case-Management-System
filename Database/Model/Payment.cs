using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Database.Model
{
    public class Payment : BaseModel
    {
        [Key]
        public string PaymentId { get; set; } = Guid.NewGuid().ToString();

        public int CaseId { get; set; }
        public int Amount { get; set; }
        public string? Status { get; set; } = "PENDING";
        public string? ValidationId { get; set; } = string.Empty;
        public string? TransactionId { get; set; } = string.Empty;
        public int PaymentMethodId { get; set; }

        public PaymentMethod? Method { get; set; }
    }
}