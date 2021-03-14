using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T3App.Blazor.ViewModels
{
    public interface ILoginViewModel
    {
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        Task LoginUser();
    }
}
