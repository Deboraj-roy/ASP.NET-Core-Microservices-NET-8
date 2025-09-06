using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            this._orderService = orderService;
        }
        public IActionResult OrderIndex()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<OrderHeaderDto> result;
            string userId = "";
            if (!User.IsInRole(SD.RoleAdmin))
            {
                userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            }
            ResponseDto response = _orderService.GetAllOrder(userId).GetAwaiter().GetResult();
            if (response != null && response.IsSuccess)
            {
                result = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));
            }
            else
            {
                result = new List<OrderHeaderDto>();
                //result = new List<OrderHeaderDto>();
            }
            return Json(new { data = result });
        }
    }
}
