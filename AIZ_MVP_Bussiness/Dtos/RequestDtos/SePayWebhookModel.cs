using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    public class SePayWebhookModel
    {
        public string content { get; set; }        // Nội dung chuyển khoản
        public decimal transferAmount { get; set; } // Số tiền nhận được
        public string account_number { get; set; }  // Số tài khoản ngân hàng của bạn
        public string transactionDate { get; set; } // Thời gian giao dịch
        public string reference_number { get; set; } // Mã tham chiếu của ngân hàng
    }
}
