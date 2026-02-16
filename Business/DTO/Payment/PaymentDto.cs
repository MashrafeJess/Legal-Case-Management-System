using System;
using System.Collections.Generic;
using System.Text;

namespace Business.DTO.Payment
{
    public static class PaymentDto
    {
        public class InitiatePaymentDto
        {
            public int CaseId { get; set; }
            public int? HearingId { get; set; }
            public int Amount { get; set; }
            public int PaymentMethodId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public string CustomerPhone { get; set; } = string.Empty;
            public string CustomerAddress { get; set; } = string.Empty;
        }
        public class PaymentResponseDto
        {
            public string PaymentId { get; set; } = string.Empty;
            public int CaseId { get; set; }
            public int Amount { get; set; }
            public string Status { get; set; } = string.Empty;
            public string? TransactionId { get; set; }
            public string? ValidationId { get; set; }
            public string PaymentMethodName { get; set; } = string.Empty;
            public string CreatedBy { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; }
        }
        public class CashPaymentDto
        {
            public int CaseId { get; set; }
            public int? HearingId { get; set; }
            public int Amount { get; set; }
            public int PaymentMethodId { get; set; }
        }
    }
}