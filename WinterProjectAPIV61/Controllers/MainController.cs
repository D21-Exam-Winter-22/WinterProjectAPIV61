using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Diagnostics;
using System.Timers;
using WinterProjectAPIV61.DataTransferObjects;
using WinterProjectAPIV61.Functions;
using WinterProjectAPIV61.Models;


namespace WinterProjectAPIV61.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly PaymentApidb3Context context;
        private static Dictionary<int, string> TokenDictionary = new Dictionary<int, string>();

        public MainController(PaymentApidb3Context context)
        {
            this.context = context;
            
        }

        [HttpGet("IsOnline")]
        public bool ApplicationIsOnline()
        {
            return true;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> LoginForToken(GetEncodingDto request)
        {
            //string toEncode = request.Username + request.Password;
            //string EncodedValue = Functions.Base64.Encode(toEncode);
            string EncodedValue = GenerateRandomString.CreateString(20);

            List<ShareUser> UsersList = await context.ShareUsers.Where(user => string.Equals(user.UserName, request.Username) && string.Equals(user.Password, request.Password)).ToListAsync();
            if (UsersList.Count == 0)
            {
                return NotFound("Invalid user details");
            }
            
            //The selected user
            ShareUser user = UsersList.First();

            //Check if the key is already present in the dictionary
            if (TokenDictionary.ContainsKey(user.UserId))
            {
                return Ok("Already logged in");
            }

            if ((bool)user.IsBlacklisted)
            {
                return Ok("Unable to log into a blacklisted account");
            }

            if ((bool)user.IsDisabled)
            {
                return Ok("This account has been disabled");
            }
            
            TokenDictionary.Add(user.UserId, EncodedValue);

            return Ok(EncodedValue);
        }

        [HttpPost("Logout/{UserID}")]
        public async Task<ActionResult<string>> LogOutRemoveToken(int UserID)
        {
            string ReturnString = LogOutClass.LogOut(TokenDictionary, UserID);
            return Ok(ReturnString);
        }

        [HttpGet("GetUserIDOnToken/{token}")]
        public async Task<ActionResult<int>> GetUserIDOnToken(string token)
        {
            int TheKey = TokenDictionary.FirstOrDefault(x => string.Equals(x.Value, token)).Key;
            return Ok(TheKey);
        }
        
        [HttpGet("GetUserIDUserNameOnToken/{token}")]
        public async Task<ActionResult<UserIDUserNameOnTokenDTO>> GetUserIDUserNameOnToken(string token)
        {
            int TheKey = TokenDictionary.FirstOrDefault(x => string.Equals(x.Value, token)).Key;

            ShareUser TheUser = await context.ShareUsers.FindAsync(TheKey);

            UserIDUserNameOnTokenDTO UserDetails = new UserIDUserNameOnTokenDTO
            {
                UserID = TheKey,
                UserName = TheUser.UserName
            };
            
            return Ok(UserDetails);
        }

        [HttpGet("GetWholeDictionary")]
        public async Task<ActionResult<Dictionary<int, string>>> GetTokenDictionary()
        {
            return Ok(TokenDictionary);
        }

    }
}
