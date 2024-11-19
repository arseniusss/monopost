using Monopost.DAL.Repositories.Interfaces;

namespace Monopost.PresentationLayer.Helpers
{
    public static class UserSession
    {
        public static int CurrentUserId { get; set; }


        public static int GetCurrentUserId(IUserRepository userRepository)
        {
            if (CurrentUserId == 0)
            {
                Random random = new Random();
                return random.Next(1, 1000); 
            }
            var user = userRepository.GetByIdAsync(CurrentUserId); 
            if (user != null)
            {
                return CurrentUserId;
            }
            return new Random().Next(1, 1000);
        }
    }
}