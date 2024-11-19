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
                int rand = random.Next(1, 1000);
                return rand;
            }
            var user = userRepository.GetByIdAsync(CurrentUserId); 
            if (user != null)
            {
                
                return CurrentUserId;
            }
            return new Random().Next(1, 1000);
        }

        public static async Task SetCurrentUserId(IUserRepository userRepository, string? email)
        {
            if(email != null)
            {
                var users = await userRepository.GetAllAsync();
                var userList = users.ToList();
                foreach(var user in userList)
                {
                    if(user.Email == email)
                        CurrentUserId = user.Id;
                }
            }
            else
            {
                Random random = new Random();
                int rand = random.Next(10000000, 20000000);
                CurrentUserId = rand;
            }
        }
    }
}