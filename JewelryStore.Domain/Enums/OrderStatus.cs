using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,      // در انتظار پرداخت
        Paid = 1,         // پرداخت‌شده
        Processing = 2,   // در حال پردازش
        Shipped = 3,      // ارسال‌شده
        Delivered = 4,    // تحویل‌شده
        Cancelled = 5,    // لغو‌شده
        Returned = 6      // مرجوع‌شده
    }
}
