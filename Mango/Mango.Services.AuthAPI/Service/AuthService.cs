using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this._db = db ?? throw new ArgumentNullException(nameof(db));
            this._userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this._roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Register(RegistrationRequestDto model)
        {
            ApplicationUser user = new()
            {
                UserName = model.Email,
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                Name = model.Name,
                PhoneNumber = model.PhoneNumber
            };

            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var userToReturn = _db.Users.First(u => u.Email == model.Email);
                    if (userToReturn != null)
                    {
                        UserDto userDto = new()
                        {
                            ID = userToReturn.Id,
                            Email = userToReturn.Email,
                            Name = userToReturn.Name,
                            PhoneNumber = userToReturn.PhoneNumber
                        };
                        return "";
                    }

                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;

                }
            }
            catch (Exception e)
            {

            }
            return "Error Encountered";
        }
    }
}
