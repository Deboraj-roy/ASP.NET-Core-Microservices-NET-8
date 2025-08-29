using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartAPIController : ControllerBase
    {
        private ResponseDto _response;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private IConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly ISendEmailService _emailService;


        public CartAPIController(AppDbContext db, IMapper mapper, IProductService productService,
            ICouponService couponService, IConfiguration configuration, IMessageBus messageBus, ISendEmailService emailService)
        {
            this._db = db;
            this._mapper = mapper;
            this._response = new ResponseDto();
            this._productService = productService ?? throw new ArgumentNullException(nameof(productService));
            this._couponService = couponService;
            this._configuration = configuration;
            this._messageBus = messageBus;
            this._emailService = emailService;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                CartDto cart = new();
                var cartHead = await _db.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cartHead is not null)
                {
                    cart.CartHeader = _mapper.Map<CartHeaderDto>(cartHead);
                    var details = _db.CartDetails.Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId);

                    cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(details);

                    //var productDtos = await _productService.GetProductsAsync();


                    foreach (var item in cart.CartDetails)
                    {
                        //item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                        item.Product = await _productService.GetProductByIdAsync(item.ProductId);

                        cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                    }

                    //apply coupon if any
                    if (!string.IsNullOrWhiteSpace(cart.CartHeader.CouponCode))
                    {
                        CouponDto coupon = await _couponService.GetCouponAsync(cart.CartHeader.CouponCode);
                        if (coupon != null && cart.CartHeader.CartTotal > coupon.MinAmount)
                        {
                            cart.CartHeader.CartTotal -= coupon.DiscountAmount;
                            cart.CartHeader.Discount = coupon.DiscountAmount;
                        }
                    }

                }


                _response.Result = cart;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.ToString();
            }
            return _response;
        }

        //[HttpPost("RemoveCoupon")]
        //public async Task<object> RemoveCoupon([FromBody] CartDto cartDto)
        //{
        //    try
        //    {
        //        var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
        //        cartFromDb.CouponCode = "";
        //        _db.CartHeaders.Update(cartFromDb);
        //        await _db.SaveChangesAsync();
        //        _response.Result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.IsSuccess = false;
        //        _response.Message = ex.ToString();
        //    }
        //    return _response;
        //}

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);

                if (cartHeaderFromDb == null)
                {
                    //create header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    _db.CartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();
                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
                else
                {
                    //if header is not null
                    //check if details has same product
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                        u => u.ProductId == cartDto.CartDetails.First().ProductId &&
                        u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                    if (cartDetailsFromDb == null)
                    {
                        //create cartdetails
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();

                    }
                    else
                    {
                        //update count in cart details
                        cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();

                    }

                }
                _response.Result = _mapper.Map<CartDto>(cartDto);

            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails = _db.CartDetails
                   .First(u => u.CartDetailsId == cartDetailsId);

                int totalCountofCartItem = _db.CartDetails.Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();
                _db.CartDetails.Remove(cartDetails);
                if (totalCountofCartItem == 1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders
                       .FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);

                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }
                //await _db.SaveChangesAsync();

                _response.Result = await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }
        [HttpPost("EmailCartRequest")]
        public async Task<object> EmailCartRequest([FromBody] CartDto cartDto)
        {
            try
            {
                await _emailService.SendAsync(cartDto.CartHeader.Email, "Your Mango Cart Summary", MailBody(cartDto));

                var queueName = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue") ?? "emailshoppingcart";
                await _messageBus.PublishMessage(cartDto, queueName);
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.ToString();
            }
            return _response;
        }

        private string MailBody(CartDto cartDto)
        {
            var message = new StringBuilder();
            message.Append("<html><body style='font-family: Arial, sans-serif; font-size:14px;'>");
            message.Append("<h2>Cart Email Requested</h2>");
            message.Append($"<p><strong>Total:</strong> {cartDto.CartHeader.CartTotal:C}</p>");
            message.Append("<hr/>");
            message.Append("<h3>Items in Your Cart:</h3>");
            message.Append("<ul style='list-style-type: none; padding: 0;'>");

            foreach (var item in cartDto.CartDetails)
            {
                message.Append("<li style='margin-bottom: 8px;'>");
                message.Append($"{item.Product.Name} <strong>x {item.Count}</strong>");
                message.Append("</li>");
            }

            message.Append("</ul>");
            message.Append("<p>Thank you for shopping with Mango!</p>");
            message.Append("</body></html>");

            return message.ToString();
        }

    }

}
