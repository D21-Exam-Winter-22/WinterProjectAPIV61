namespace WinterProjectAPIV61.Functions;

public class GenerateRandomString
{
    public static string CreateString(int stringLength)
    {
        /*
         Random rd = new Random();
        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
        char[] chars = new char[stringLength];

        for (int i = 0; i < stringLength; i++)
        {
            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
        }
        */
        
        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";

        string FinalString = "";
        var seed = 3;
        var random = new Random(seed);

        for (int i = 0; i < stringLength; i++)
        {
            int randomNumber = random.Next(0, allowedChars.Length);
            char RandomCharacter = allowedChars[randomNumber];
            FinalString = FinalString + RandomCharacter;
        }
        

        return FinalString;
    }
}