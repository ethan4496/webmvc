using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebMVC.Interfaces;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }
        [Authorize]
        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff)]
        public IActionResult Index()
        {
            return View();
        }


        #region Json data
        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            var data = await _warehouseService.GetWarehousesByStatus(-1);
            return Json(data);
        }
        #endregion
    }
}
