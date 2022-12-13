using Microsoft.IdentityModel.Tokens;

namespace WinterProjectAPIV61.Functions;

public class LogOutClass
{
    public static string LogOut(Dictionary<int, string> TokenDictionary, int UserID)
    {
        string TextToReturn = "";
        if (TokenDictionary.IsNullOrEmpty())
        {
            TextToReturn = "No users currently logged in";
            return TextToReturn;
        }
        if (TokenDictionary.ContainsKey(UserID))
        {
            TokenDictionary.Remove(UserID);
            TextToReturn = "Successfully Logged out";
        }
        else
        {
            TextToReturn = "Token not found";
        }
        
        return TextToReturn;
    }
}