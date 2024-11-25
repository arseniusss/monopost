using Monopost.DAL.Entities;

namespace Monopost.BLL.Services.Interfaces
{
   
    public interface IAuthenticationService
    {
        Task<(bool success, string message)> Login(string email, string password);
        Task LoginAsGuestAsync();
        Task<bool> ValidateCredentialsAsync(string email, string password);
        Task<(bool success, string message)> Register(User user, string confirmPassword);
        (bool isValid, string errorMessage) ValidateUserData(User user, string confirmPassword);
    }
    
}
