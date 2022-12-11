using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterProjectAPIV61.DataTransferObjects;
using WinterProjectAPIV61.Functions;
using WinterProjectAPIV61.Models;

namespace WinterProjectAPIV61.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShareUserController : ControllerBase
    {
        private readonly PaymentApidb3Context context;

        public ShareUserController(PaymentApidb3Context context)
        {
            this.context = context;
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<List<ShareUser>>> GetUsers()
        {
            var users = await context.ShareUsers.ToListAsync();
            if (users.Count == 0)
            {
                return NotFound(users);
            }
            return Ok(users);
        }

        [HttpGet("GetUserByID/{ID}")]
        public async Task<ActionResult<UserWithQuestionDTO>> GetUserOnID(int ID)
        {
            ShareUser SearchedUser = context.ShareUsers.Find(ID);
            if (SearchedUser == null)
            {
                return NotFound(SearchedUser);
            }

            var query = from user in context.ShareUsers
                join securityquestion in context.SecurityQuestions
                    on user.QuestionId equals securityquestion.QuestionId
                where user.UserId == ID
                select new
                {
                    user.UserId,
                    user.UserName,
                    user.PhoneNumber,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Password,
                    user.IsAdmin,
                    user.Address,
                    securityquestion.QuestionId,
                    user.SecurityAnswer,
                    user.IsDisabled,
                    user.IsBlacklisted, 
                    securityquestion.Question
                };
            UserWithQuestionDTO ResultUser = new UserWithQuestionDTO();
            foreach (var entry in query)
            {
                ResultUser.UserId = entry.UserId;
                ResultUser.UserName = entry.UserName;
                ResultUser.PhoneNumber = entry.PhoneNumber;
                ResultUser.FirstName = entry.FirstName;
                ResultUser.LastName = entry.LastName;
                ResultUser.Email = entry.Email;
                ResultUser.Password = entry.Password;
                ResultUser.IsAdmin =entry.IsAdmin;
                ResultUser.Address =entry.Address;
                ResultUser.QuestionId =entry.QuestionId;
                ResultUser.SecurityAnswer = entry.SecurityAnswer;
                ResultUser.IsDisabled = entry.IsDisabled;
                ResultUser.IsBlacklisted = entry.IsBlacklisted;
                ResultUser.Question = entry.Question;
                break;
            }

            return Ok(ResultUser);
        }

        [HttpPost("CreateUser")]
        public async Task<ActionResult<List<ShareUser>>> CreateShareUser(CreateShareUserDto request)
        {
            //Check if the username already exists in the DB
            List<ShareUser> ExistingUsers = await context.ShareUsers.Where(User => User.UserName == request.UserName).ToListAsync();
            if (ExistingUsers.Count > 0)
            {
                return Ok(string.Format("User with username: {0}, already exists", request.UserName));
            }
            
            //Check if the user with that email exists in the system and is blacklisted
            //Get the account
            List<ShareUser> ListOfUsers = await context.ShareUsers.Where(user => user.Email == request.Email).ToListAsync();

            foreach (ShareUser user in ListOfUsers)
            {
                if ((bool)user.IsBlacklisted)
                {
                    return Ok(string.Format("{0} has been blacklisted", request.Email));
                }
            }
            

            //Create the user to insert
            ShareUser UserToInsert = new ShareUser
            {
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                IsAdmin = request.IsAdmin,
                Address = request.Address,
                QuestionId = request.QuestionID,
                SecurityAnswer = request.SecurityAnswer,
                IsDisabled = false,
                IsBlacklisted = false
            };

            if (UserToInsert.PhoneNumber == null || UserToInsert.PhoneNumber.Length == 0)
            {
                UserToInsert.PhoneNumber = "11111111";
            }

            if (UserToInsert.Address == null || UserToInsert.Address.Length == 0)
            {
                UserToInsert.Address = "No Address";
            }

            if (UserToInsert.QuestionId == null || UserToInsert.QuestionId == 0)
            {
                UserToInsert.QuestionId = 1; ///////////////////////////////////THIS ONE NEEDS TO BE CHANGED NEXT ITERATION
                UserToInsert.SecurityAnswer = "No Answer";
            }
            
            

            //Insert the user
            await context.ShareUsers.AddAsync(UserToInsert);
            await context.SaveChangesAsync();

            return await GetUsers();
        }

        //Update details of a user
        [HttpPut("UpdateUser")]
        public async Task<ActionResult<ShareUser>> UpdateUserDetails(ShareUser request)
        {
            ShareUser RecordToChange = context.ShareUsers.Find(request.UserId);
            if (RecordToChange != null)
            {
                RecordToChange.UserName = request.UserName;
                RecordToChange.PhoneNumber = request.PhoneNumber;
                RecordToChange.FirstName = request.FirstName;
                RecordToChange.LastName = request.LastName;
                RecordToChange.Email = request.Email;
                RecordToChange.Password = request.Password;
                RecordToChange.IsAdmin = request.IsAdmin;
                RecordToChange.Address = request.Address;
                RecordToChange.QuestionId = request.QuestionId;
                RecordToChange.SecurityAnswer = request.SecurityAnswer;
                RecordToChange.IsDisabled = request.IsDisabled;
                RecordToChange.IsBlacklisted = request.IsBlacklisted;
                
            }
            else
            {
                return NotFound(RecordToChange);
            }
            await context.SaveChangesAsync();
            return Ok(RecordToChange);
        }

        [HttpGet("GetDetailsOnUsername/{UserName}")]
        public async Task<ActionResult<CreateShareUserDto>> GetLogInDetails(string UserName)
        {
            List<ShareUser> UsersList = await context.ShareUsers.Where(User => User.UserName == UserName).ToListAsync();

            ShareUser SingleUser = UsersList.First();
            CreateShareUserDto User = null;

            if (UsersList.Count == 0)
            {
                return NotFound(User);
            }

            User = new CreateShareUserDto
            {
                UserName = SingleUser.UserName,
                PhoneNumber = SingleUser.PhoneNumber,
                FirstName = SingleUser.FirstName,
                LastName = SingleUser.LastName,
                Email = SingleUser.Email,
                IsAdmin = SingleUser.IsAdmin,
                Password = SingleUser.Password,
                Address = SingleUser.Address,
                QuestionID = SingleUser.QuestionId,
                SecurityAnswer = SingleUser.SecurityAnswer,
                IsDisabled = SingleUser.IsDisabled,
                IsBlacklisted = SingleUser.IsBlacklisted
            };

            return Ok(User);
        }

        [HttpGet("SearchShareUsers/{SearchString}")]
        public async Task<ActionResult<List<UserWithQuestionDTO>>> SearchForUsers(string SearchString)
        {
            List<ShareUser> SearchedUsers = await context.ShareUsers.Include(entry => entry.Question)
                .Where(user => user.UserName.Contains(SearchString) || user.FirstName.Contains(SearchString) || user.LastName.Contains(SearchString)).ToListAsync();
          

            
            List<UserWithQuestionDTO> CustomizedSearchList = new List<UserWithQuestionDTO>();

            foreach (ShareUser User in SearchedUsers)
            {
                UserWithQuestionDTO CustomizedUser = new UserWithQuestionDTO
                {
                    UserId = User.UserId,
                    UserName = User.UserName,
                    PhoneNumber = User.PhoneNumber,
                    FirstName = User.FirstName,
                    LastName = User.LastName,
                    Email = User.Email,
                    Password = User.Password,
                    IsAdmin = User.IsAdmin,
                    Address = User.Address,
                    QuestionId = User.QuestionId,
                    SecurityAnswer = User.SecurityAnswer,
                    IsDisabled = User.IsDisabled,
                    IsBlacklisted = User.IsBlacklisted,
                    Question = User.Question != null ? User.Question.Question : "No Question"
                };
                CustomizedSearchList.Add(CustomizedUser);
            }

            return Ok(CustomizedSearchList);
        }

        [HttpGet("GetAllUsersGroups/{UserID}")]
        public async Task<ActionResult<List<CustomUsersGroupDTO>>> GetAllUsersGroups(int UserID)
        {
            //List<UserGroup> ListOfUsersGroups = await context.UserGroups.Include(entry => entry.Group).Include(entry => entry.User).Where(usergroup => usergroup.UserId == UserID).ToListAsync();

            var query = from sharegroup in context.ShareGroups
                join usergroup in context.UserGroups on sharegroup.GroupId equals usergroup.GroupId
                where usergroup.UserId == UserID
                select new
                {
                    sharegroup.GroupId,
                    sharegroup.Name,
                    sharegroup.Description,
                    sharegroup.HasConcluded,
                    sharegroup.IsPublic,
                    sharegroup.CreationDate,
                    sharegroup.ConclusionDate,
                    sharegroup.LastActiveDate
                };

            List<CustomUsersGroupDTO> ListOfusersGroups = new List<CustomUsersGroupDTO>();
            foreach (var entry in query)
            {
                ListOfusersGroups.Add(new CustomUsersGroupDTO
                {
                    GroupId = entry.GroupId,
                    Name = entry.Name,
                    Description = entry.Description,
                    HasConcluded = entry.HasConcluded,
                    IsPublic = entry.IsPublic,
                    CreationDate = entry.CreationDate,
                    ConclusionDate = entry.ConclusionDate,
                    LastActiveDate = entry.LastActiveDate
                });
            }

            return Ok(ListOfusersGroups);
        }

        [HttpPut("DisableAccountOnID/{UserID}")]
        public async Task<ActionResult<string>> DisableAccountOnID(int UserID)
        {
            //Get the Account
            ShareUser account = await context.ShareUsers.FindAsync(UserID);

            if (account == null)
            {
                return Ok(string.Format("Account with ID: {0} not found", UserID));
            }
            
            //Edit the account
            account.IsDisabled = true;
            await context.SaveChangesAsync();
            
            return Ok("Account Disabled");
        }

        [HttpPut("BlacklistUserOnID/{UserID}")]
        public async Task<ActionResult<string>> BlacklistUserOnID(int UserID)
        {
            //get the account
            ShareUser account = await context.ShareUsers.FindAsync(UserID);

            if (account == null)
            {
                return Ok(string.Format("Account with ID: {0}", UserID));
            }
            
            //Edit the account

            account.IsBlacklisted = true;
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("UnBlacklistUserOnID/{UserID}")]
        public async Task<ActionResult<string>> UnBlacklistUserOnID(int UserID)
        {
            //Find the user
            ShareUser TheUser = await context.ShareUsers.FindAsync(UserID);
            
            //IF the user is not found
            if (TheUser == null)
            {
                return Ok("The user was not found");
            }
            
            //Set the user to false for blacklisted
            TheUser.IsBlacklisted = false;
            //Save the changes
            await context.SaveChangesAsync();
            
            return Ok("Un-Blacklisted the user");
        }

        [HttpPut("ResetAccountPassword/{UserID}")]
        public async Task<ActionResult<string>> ResetAccountPassword(int UserID)
        {
            //Identify the account
            ShareUser TheAccount = await context.ShareUsers.FindAsync(UserID);

            if (TheAccount == null)
            {
                return NotFound(string.Format("Account with the UserID: {0} does not exist", UserID));
            }
            
            //Change the password
            string password = GenerateRandomString.CreateString(15);
            
            //Save it in the DB
            TheAccount.Password = password;
            await context.SaveChangesAsync();

            return Ok(password);

        }

        [HttpPut("ChangePassword")]
        public async Task<ActionResult<string>> ChangePassword(ChangePasswordDTO request)
        {
            //Get the account for that userID
            ShareUser TheAccount = await context.ShareUsers.FindAsync(request.UserID);
            //Check if it is a valid userid
            if (TheAccount == null)
            {
                return Ok("The Account does not exist");
            }
            
            //Check that the current password matches
            if (TheAccount.Password != request.CurrentPassword)
            {
                return Ok("The Current password is incorrect");
            }
            else
            {
                //If it does match, update the password
                TheAccount.Password = request.NewPassword;
                await context.SaveChangesAsync();
                return Ok("Password was successfully updated");
            }
            
           
        }





    }
}
