using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.Order;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace JewelryStore.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;
        private readonly IShippingService _shippingService;
        private readonly ITaxService _taxService;
        private readonly IDiscountService _discountService;
        private readonly ISmsSender _smsSender;

        public OrderService(
            ApplicationDbContext context,
            IMapper mapper,
            ICartService cartService,
            IShippingService shippingService,
            ITaxService taxService,
            IDiscountService discountService,
            ISmsSender smsSender)
        {
            _context = context;
            _mapper = mapper;
            _cartService = cartService;
            _shippingService = shippingService;
            _taxService = taxService;
            _discountService = discountService;
            _smsSender = smsSender;
        }

        // 1️⃣ ایجاد سفارش جدید از سبد خرید
        public async Task<OrderResultDto> CreateOrderAsync(CreateOrderDto createDto)
        {
            // دریافت سبد خرید کاربر
            var cartDto = await _cartService.GetCartAsync(createDto.UserId, null);
            if (cartDto == null || !cartDto.Items.Any())
                throw new InvalidOperationException("سبد خرید شما خالی است.");

            // اعتبارسنجی سبد خرید
            await _cartService.ValidateCartAsync(createDto.UserId);

            // دریافت مجدد سبد خرید معتبر
            cartDto = await _cartService.GetCartAsync(createDto.UserId, null);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstAsync(c => c.Id == cartDto.Id);

            // محاسبه هزینه‌ها
            decimal subTotal = cartDto.TotalPrice;
            decimal discountTotal = 0;
            decimal discountCodeAmount = 0;
            DiscountCode? discountCode = null;

            // اعمال کد تخفیف
            if (!string.IsNullOrEmpty(createDto.DiscountCode))
            {
                var discountResult = await _discountService.ValidateAndApplyDiscountAsync(
                    createDto.DiscountCode, createDto.UserId, subTotal);

                if (!discountResult.IsValid)
                    throw new InvalidOperationException(discountResult.Message);

                discountCode = discountResult.DiscountCode;
                discountCodeAmount = discountResult.DiscountAmount;
                discountTotal += discountCodeAmount;
            }

            // محاسبه هزینه ارسال
            var shippingCost = await _shippingService.CalculateShippingCostAsync(subTotal);

            // محاسبه مالیات (قابل توجه: مالیات بعد از تخفیف و قبل از ارسال محاسبه می‌شود)
            var amountAfterDiscount = subTotal - discountTotal;
            var taxAmount = await _taxService.CalculateTaxAsync(amountAfterDiscount);

            // محاسبه مبلغ نهایی
            var totalAmount = amountAfterDiscount + shippingCost + taxAmount;

            // تولید شماره سفارش یکتا
            var orderNumber = GenerateOrderNumber();

            // ایجاد موجودیت سفارش
            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = createDto.UserId,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                PaymentMethod = createDto.PaymentMethod,
                SubTotal = subTotal,
                DiscountTotal = discountTotal,
                ShippingCost = shippingCost,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                DiscountCodeId = discountCode?.Id,
                DiscountCodeAmount = discountCodeAmount,
                ShippingAddress = createDto.ShippingAddress,
                RecipientName = createDto.RecipientName,
                RecipientPhone = createDto.RecipientPhone,
                CustomerNote = createDto.CustomerNote,
                CreatedAt = DateTime.Now
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // ایجاد آیتم‌های سفارش از آیتم‌های سبد خرید
            foreach (var cartItem in cart.Items)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product == null) continue;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    VariantId = cartItem.VariantId,
                    ProductName = product.Name,
                    ProductImage = product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl,
                    VariantName = cartItem.Variant?.VariantName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    DiscountAmount = cartItem.DiscountAmount,
                    FinalUnitPrice = cartItem.FinalUnitPrice,
                    TotalPrice = cartItem.TotalPrice,
                    CreatedAt = DateTime.Now
                };

                await _context.OrderItems.AddAsync(orderItem);

                // کاهش موجودی محصول اصلی و تنوع
                product.Quantity -= cartItem.Quantity;
                if (cartItem.VariantId.HasValue)
                {
                    var variant = await _context.ProductVariants.FindAsync(cartItem.VariantId);
                    if (variant != null)
                        variant.Quantity -= cartItem.Quantity;
                }
            }

            // ثبت تاریخچه وضعیت سفارش
            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Pending,
                Note = "سفارش ثبت شد و در انتظار پرداخت است.",
                CreatedAt = DateTime.Now
            };
            await _context.OrderStatusHistories.AddAsync(statusHistory);

            // خالی کردن سبد خرید کاربر
            await _cartService.ClearCartAsync(createDto.UserId);

            await _context.SaveChangesAsync();

            // TODO: اتصال به درگاه پرداخت و دریافت لینک پرداخت
            // var paymentUrl = await _paymentGateway.InitiatePaymentAsync(order);
            var paymentUrl = "/Payment/Pay/" + order.Id; // موقت




            // ارسال پیامک به کاربر
            var user = await _context.Users.FindAsync(createDto.UserId);
            if (user != null && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                // ✅ درست: استفاده از آرایه
                _smsSender.SendSms(
                    type: 3,
                    phoneNumber: user.PhoneNumber,
                    parameters: new string[]
                    {
            user.FullName ?? user.Username,
            order.TotalAmount.ToString("N0"),
            order.OrderNumber
                    }
                );
            }

            // ارسال پیامک به ادمین
            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == Domain.Enums.UserRole.Admin && u.IsActive);
            if (admin != null)
            {
                string adminMessage = $"💰 پرداخت جدید! سفارش {order.OrderNumber} به مبلغ {order.TotalAmount:N0} توسط {user?.FullName ?? user?.Username}";

                // ✅ اینجا هم فقط یک پارامتر داریم، پس یک آرایه تک‌عنصری می‌سازیم
                _smsSender.SendSms(
                    type: 0,
                    phoneNumber: admin.PhoneNumber,
                    parameters: new string[] { adminMessage }
                );
            }

            return new OrderResultDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = totalAmount,
                PaymentUrl = paymentUrl,
                Message = "سفارش با موفقیت ثبت شد."
            };
        }

        // 2️⃣ دریافت سفارش با شناسه
        public async Task<OrderDetailDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException("سفارش یافت نشد.");

            return _mapper.Map<OrderDetailDto>(order);
        }

        // 3️⃣ دریافت سفارش با شماره سفارش
        public async Task<OrderDetailDto> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null)
                throw new KeyNotFoundException("سفارش یافت نشد.");

            return _mapper.Map<OrderDetailDto>(order);
        }

        // 4️⃣ دریافت سفارش‌های یک کاربر
        public async Task<IEnumerable<OrderDetailDto>> GetUserOrdersAsync(int userId, int page = 1, int pageSize = 10)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderDetailDto>>(orders);
        }

        // 5️⃣ تغییر وضعیت سفارش (ادمین)
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? note = null)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException("سفارش یافت نشد.");

            order.OrderStatus = newStatus;
            order.UpdatedAt = DateTime.Now;

            // اگر وضعیت به 'Paid' تغییر کرد، تاریخ پرداخت را ثبت کن
            if (newStatus == OrderStatus.Paid)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaymentDate = DateTime.Now;
            }

            // ثبت تاریخچه
            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = newStatus,
                Note = note ?? $"وضعیت به {newStatus} تغییر یافت.",
                CreatedAt = DateTime.Now
            };
            await _context.OrderStatusHistories.AddAsync(statusHistory);

            await _context.SaveChangesAsync();
            return true;
        }

        // 6️⃣ افزودن کد رهگیری به سفارش
        public async Task<bool> AddTrackingCodeAsync(int orderId, string trackingCode)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException("سفارش یافت نشد.");

            order.TrackingCode = trackingCode;
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // 🔧 متدهای کمکی خصوصی
        private static string GenerateOrderNumber()
        {
            var datePart = DateTime.Now.ToString("yyyyMMdd");
            var randomPart = RandomNumberGenerator.GetInt32(1000, 9999).ToString();
            return $"ORD-{datePart}-{randomPart}";
        }
    }
}
