using Microsoft.IdentityModel.Tokens;

namespace WinterProjectAPIV61.Functions;

public class LogOutClass
{
    public static string LogOut(Dictionary<int, string> TokenDictionary, int UserID)
    {
        if (TokenDictionary.IsNullOrEmpty())
        {
            return "No users currently logged in";
        }
        if (TokenDictionary.ContainsKey(UserID))
        {
            TokenDictionary.Remove(UserID);
            return "Successfully Logged out";
        }
        
        return "Token not found";
    }
}