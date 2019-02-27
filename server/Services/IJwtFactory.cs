using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Diplomacy.Models;

namespace Diplomacy.Services
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity);
        Task<Token> CreateApplicationUserToken(string username, string password);
        ClaimsIdentity GenerateClaimsIdentity(string userName, string id);
    }
}
