using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface ISmsSender
    {
        /// <summary>
        /// ارسال پیامک با الگو (Pattern)
        /// </summary>
        /// <param name="type">نوع پیامک: 1=ثبت‌نام، 2=فراموشی رمز، 3=پرداخت موفق، 4=ارسال سفارش، 5=تحویل سفارش</param>
        /// <param name="phoneNumber">شماره دریافت‌کننده</param>
        /// <param name="parameters">پارامترهای متنوع برای الگو</param>
        /// <returns>true در صورت موفقیت</returns>
        bool SendSms(int type, string phoneNumber, params string[] parameters);
    }
}
