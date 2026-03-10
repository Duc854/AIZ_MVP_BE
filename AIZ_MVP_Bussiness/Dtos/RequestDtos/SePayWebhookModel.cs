using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    public class SePayWebhookModel
    {
        public long id { get; set; }
        public string gateway { get; set; }
        public string transactionDate { get; set; }
        public string accountNumber { get; set; }
        public string content { get; set; }
        public decimal transferAmount { get; set; }
        public string transferType { get; set; }
        public string referenceCode { get; set; }
        public decimal? accumulated { get; set; }
        public string code { get; set; }
    }
}
