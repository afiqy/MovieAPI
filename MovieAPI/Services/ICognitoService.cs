using System.Threading.Tasks;

namespace MovieAPI.Services
{
    public interface ICognitoService
    {
        Task<(bool IsSuccess, string Message, string CognitoUserId)> RegisterUserAsync(string email, string password);
        Task<(string IdToken, string AccessToken)> LoginAsync(string email, string password);
    }
}
