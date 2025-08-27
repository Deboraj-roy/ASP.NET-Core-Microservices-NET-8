using Mango.MessageBus;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDto _response;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;
        private string _registerUserQueue;

        public AuthAPIController(IAuthService authService, IMessageBus messageBus, IConfiguration configuration)
        {
            this._authService = authService ?? throw new ArgumentNullException(nameof(authService));
            this._configuration = configuration;
            this._messageBus = messageBus;
            this._response = new ResponseDto();
            this._registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            var responseMessage = await _authService.Register(model);
            if (!string.IsNullOrEmpty(responseMessage))
            {
                _response.IsSuccess = false;
                _response.Message = responseMessage;
                return BadRequest(_response);
            }
            await _messageBus.PublishMessage(model.Email, _registerUserQueue);

            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Username or password is incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);

        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDto model)
        {
            var assignRoleSuccessful = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessful)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encountered";
                return BadRequest(_response);
            }
            return Ok(_response);

        }


    }
}
