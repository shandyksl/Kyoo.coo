namespace Yoo.Core.Model.DTO
{
    public class TransferRequest
    {
        public string AgentCode { get; set; }
        public string LoginName { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
    }
}
