//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Authorization;
//using T3App.Blazor.ViewModels;

//namespace T3App.Blazor.Pages
//{
//    public class LoginBase : ComponentBase
//    {
//        [Inject] public NavigationManager NavigationManager { get; set; }
//        [Inject] public ILoginViewModel LoginViewModel { get; set; }

//        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }

//        protected async Task LoginUser()
//        {
//            await LoginViewModel.LoginUser();
//            NavigationManager.NavigateTo("/profile", true);
//        }

      

//        private async Task LogUserAuthenticationState()
//        {
//            var authState = await authenticationStateTask;
//            var user = authState.User;

//            if (user.Identity.IsAuthenticated)
//            {
//                Console.WriteLine($"User {user.Identity.Name} is authenticated.");
//            }
//            else
//            {
//                Console.WriteLine("User is NOT authenticated.");
//            }
//        }
//    }
//}