using JewelryStore.Services.DTOs.Report;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IAdminService adminService, ILogger<DashboardController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = await _adminService.GetDashboardStatisticsAsync();
                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت آمار داشبورد");
                TempData["Error"] = "خطا در دریافت آمار.";
                return View(new DashboardStatisticsDto());
            }
        }
    }
}
