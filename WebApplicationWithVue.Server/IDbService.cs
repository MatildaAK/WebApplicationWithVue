using WebApplicationWithVue.Server.Controllers;

namespace WebApplicationWithVue.Server
{
    public interface IDbService
    {
        Task<string> RegisterAccountAsync(LoginInformation loginInformation);
        Task<bool> LoginAsync(string userName, string password);
        void SaveRefreshToken(string refreshToken);
        Task<bool> RefreshToken(string refreshToken);
    }
}
