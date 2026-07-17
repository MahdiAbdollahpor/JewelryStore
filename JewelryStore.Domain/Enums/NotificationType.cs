using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Enums
{
    public enum NotificationType
    {
        PaymentSuccess = 0,
        OrderShipped = 1,
        OrderDelivered = 2,
        AdminPayment = 3
    }
}
