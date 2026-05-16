using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum Payment_status
    {
        Failed = -1,   //  Thất bại
        Pending = 0,   // Đang chờ xử lý
        Success = 1     // Thành công
    }
    public enum Order_status
    {
        Cancelled = -1,   // Đã hủy
        Pending = 0,      // Đang chờ xử lý
        Completed = 1     // Hoàn thành
    }
}
