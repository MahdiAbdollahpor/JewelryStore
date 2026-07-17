using JewelryStore.Services.DTOs.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface ISmsService
    {
        /// ارسال یک پیامک به شماره مشخص
        Task<SmsResult> SendAsync(string phoneNumber, string message);
    }
}
