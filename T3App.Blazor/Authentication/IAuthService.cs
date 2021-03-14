
using T3App.Shared;
using System.Threading.Tasks;

namespace T3App.Blazor.Authentication
{
    public interface IAuthService
    {
        Task<LoginResult> Login(LoginModel loginModel);
        Task Logout();
        Task<RegisterResult> Register(RegisterModel registerModel);
    }
}
