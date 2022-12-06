using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterProjectAPIV61.DataTransferObjects;
using WinterProjectAPIV61.Functions;
using WinterProjectAPIV61.Models;
using WinterProjectAPIV61.PDFGenerator;

namespace WinterProjectAPIV61.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserGroupController : ControllerBase
    {
        private readonly PaymentApidb3Context context;

        public UserGroupController(PaymentApidb3Context context)
        {
            this.context = context;
        }

        [HttpGet("GetGroupByGroupID/{GroupID}")]
        public async Task<ActionResult<List<UserGroup>>> GetAllGroupMembers(int GroupID)
        {
            List<UserGroup> GroupMembersList = await context.UserGroups
                .Include(UserGroup => UserGroup.User)
                .Include(Group => Group.Group)
                .Where(Group => Group.GroupId == GroupID).ToListAsync();


            return Ok(GroupMembersList);
        }

        [HttpGet("GetAllUserGroups")]
        public async Task<ActionResult<List<UserGroup>>> GetAllUserGroups()
        {
            List<UserGroup> GroupMembersList = await context.UserGroups
                .Include(UserGroup => UserGroup.User)
                .Include(Group => Group.Group).ToListAsync();

            return Ok(GroupMembersList);
        }

        [HttpPost("JoinExistingGroup")]
        public async Task<ActionResult<UserGroup>> JoinGroup(JoinGroupDto request)
        {
            //Insert into UserGroup table
            ShareGroup ExistingGroup = context.ShareGroups.Find(request.GroupID);

            //Make sure you the user isn't already in the group
            var ExistingUserGroupQuery = from usergroup in context.UserGroups
                                         where usergroup.UserId == request.UserID && usergroup.GroupId == request.GroupID
                                         select new
                                         {
                                             usergroup.UserGroupId,
                                             usergroup.UserId,
                                             usergroup.GroupId
                                         };

            List<JoinGroupDto> ExistingUserGroupsList = new List<JoinGroupDto>();

            foreach (var record in ExistingUserGroupQuery)
            {
                ExistingUserGroupsList.Add(new JoinGroupDto
                {
                    UserID = record.UserId,
                    GroupID = record.GroupId
                });
            }


            if (ExistingUserGroupsList.Count > 0)
            {
                return BadRequest("Already in the group!");
            }
            if (ExistingGroup == null)
            {
                return NotFound("Group does not exist");
            }
            UserGroup UserGroupToInsert = new UserGroup
            {
                UserId = request.UserID,
                GroupId = request.GroupID,
                IsOwner = false
            };
            
            //Update Last active date of group
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(request.GroupID);
            TheGroup.LastActiveDate = DateTime.Now;
            await context.SaveChangesAsync();

            context.UserGroups.Add(UserGroupToInsert);
            await context.SaveChangesAsync();

            //Query the UserGroup for that GroupID

            List<UserGroup> UserGroups = await context.UserGroups
                .Where(UserGroup => UserGroup.GroupId == request.GroupID)
                .Include(UserGroup => UserGroup.User)
                .ToListAsync();
            return Ok(UserGroups);
        }

        [HttpGet("MoneyOwedByEveryoneInGroupID/{GroupID}")]
        public async Task<ActionResult<List<MoneyOwedByUserGroupDto>>> CalculateIndividualSharesInGroup(int GroupID)
        {
            //Get the group's name and description
            ShareGroup TheGroup = context.ShareGroups.Find(GroupID);

            //Calculating how much everyone paid from the group
            List<Expense> AllExpensesQuery = await context.Expenses
                .Include(Expense => Expense.UserGroup)
                .Include(Expense => Expense.UserGroup.User)
                .Include(Expense => Expense.UserGroup.Group)
                .Where(UserGroup => UserGroup.UserGroup.GroupId == GroupID)
                .ToListAsync();

            //Query to get the distinct userIDs
            List<UserGroup> DistinctUserIDsQuery =
                await context.UserGroups.Where(usergroup => usergroup.GroupId == GroupID).ToListAsync();

            // Group by UserID and sum the Amount
            //Get all the distinct UserIDs


            List<int> DistinctUserIDs = DistinctUserIDsQuery.Select(entry => (int)entry.UserId).ToList();

            //Sum up all their individual expenses and create a list of MoneyOwedByUserGroupDto Objects
            List<MoneyOwedByUserGroupDto> ListOfMoneyOwedByUsersGroup = new List<MoneyOwedByUserGroupDto>();

            foreach (var UserID in DistinctUserIDs)
            {
                double TotalAmountOwed = (double)AllExpensesQuery.Where(row => row.UserGroup.UserId == UserID)
                    .Sum(entries => entries.Amount);
                //Get the individual User's details
                List<ShareUser> CurrentUsers = await context.ShareUsers.Where(entry => entry.UserId == UserID).ToListAsync();
                ShareUser CurrentUser = CurrentUsers.First();

                //Construct the Dto to add to the list
                MoneyOwedByUserGroupDto MoneyOwedByCurrentUser = new MoneyOwedByUserGroupDto
                {
                    UserID = UserID,
                    GroupID = GroupID,
                    AmountPaidDuringGroup = TotalAmountOwed,
                    FirstName = CurrentUser.FirstName,
                    LastName = CurrentUser.LastName,
                    PhoneNumber = CurrentUser.PhoneNumber,
                    UserName = CurrentUser.UserName,
                    GroupName = TheGroup.Name,
                    GroupDescription = TheGroup.Description
                };
                ListOfMoneyOwedByUsersGroup.Add(MoneyOwedByCurrentUser);
            }

            //Calculate the group's total expenditure
            double GroupsTotalExpenditure = 0;
            double GroupSize = 0;

            foreach (var UsersExpenditure in ListOfMoneyOwedByUsersGroup)
            {
                GroupsTotalExpenditure += UsersExpenditure.AmountPaidDuringGroup;
                GroupSize++;
            }

            double AverageAmountPaidDuringGroup = GroupsTotalExpenditure / GroupSize;

            foreach (var UsersExpenditure in ListOfMoneyOwedByUsersGroup)
            {
                UsersExpenditure.FinalAmountOwed = AverageAmountPaidDuringGroup - UsersExpenditure.AmountPaidDuringGroup;
            }

            //Calculate AmountAlreadyPaid by every UserGroup
            List<InPayment> ListOfInPayments = await context.InPayments
                .Include(InPayment => InPayment.UserGroup)
                .Where(UserGroup => UserGroup.UserGroup.GroupId == GroupID)
                .ToListAsync();

            //Get the list of unique UserIDs in the ListOfInPayments
            List<int> ListOfDistinctInPaymentusers =
                ListOfInPayments.Select(entry => (int)entry.UserGroup.UserId).Distinct().ToList();

            //Calculate the total in payments for each user in the group
            //Assign it to the correct MoneyOwedByUserGroupDto for that user

            foreach (var UserID in ListOfDistinctInPaymentusers)
            {
                //Total for that user
                double TotalInPayment = (double)ListOfInPayments.Where(row => row.UserGroup.UserId == UserID)
                    .Sum(entries => entries.Amount);

                //Find the user in the ListOfMoneyOwedByUsersGroup and assign the AmountAlreadyPaid to TotalPayment
                MoneyOwedByUserGroupDto CurrentUser = ListOfMoneyOwedByUsersGroup.First(user => user.UserID == UserID);
                CurrentUser.AmountAlreadyPaid = TotalInPayment;
            }

            //Recalculate the FinalAmountOwed by subtracting the AmountAlreadyPaid from it

            foreach (var user in ListOfMoneyOwedByUsersGroup)
            {
                user.FinalAmountOwed -= user.AmountAlreadyPaid;
            }

            return Ok(ListOfMoneyOwedByUsersGroup);
        }

        [HttpDelete("RemoveMemberFromGroup")]
        public async Task<ActionResult<List<UserGroup>>> RemoveMemberFromGroup(UserGroupDto request)
        {
            //TODO: IMPROVE THE SPEED OF THIS QUERY.
            //Delete related expenses and inpayments

            //Delete expenses
            //Get list of UserGroups
            List<UserGroup> usergroups = await context.UserGroups.Where(entry =>
                entry.UserId == request.UserID && entry.GroupId == request.GroupID).ToListAsync();
            //Using the usergroups, delete the expenses

            foreach (UserGroup usergroup in usergroups)
            {
                await context.Expenses.Where(entry => entry.UserGroupId == usergroup.UserGroupId)
                    .ExecuteDeleteAsync();
                await context.SaveChangesAsync();
            }


            //delete inpayments
            foreach (UserGroup usergroup in usergroups)
            {
                await context.InPayments.Where(entry => entry.UserGroupId == usergroup.UserGroupId)
                    .ExecuteDeleteAsync();
                await context.SaveChangesAsync();
            }
            
            //Delete invites from that user
            foreach (UserGroup userGroup in usergroups)
            {
                await context.Invites.Where(entry => entry.FromUserGroupId == userGroup.UserGroupId)
                    .ExecuteDeleteAsync();
                await context.SaveChangesAsync();
            }
            
            //Update group's last active date
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(request.GroupID);
            TheGroup.LastActiveDate = DateTime.Now;
            await context.SaveChangesAsync();

            //Have to delete all entries from UserGroup where the userID and GroupID match
            context.UserGroups.RemoveRange(context.UserGroups.Where(usergroup => usergroup.UserId == request.UserID && usergroup.GroupId == request.GroupID));
            await context.SaveChangesAsync();
            return await GetAllGroupMembers((int)request.GroupID);
        }

        [HttpDelete("DeleteUser/{UserID}")]
        public async Task<ActionResult<string>> DeleteUserOnUserID(int UserID)
        {
            //Get All the groups which this user is in (GroupIDs)
            List<UserGroup> ListOfParticipatingGroups = await context.UserGroups.Where(entry => entry.UserId == UserID).ToListAsync();
            //Then just user RemoveMemberFromGroup for everyone of those group IDs
            foreach (var usergroup in ListOfParticipatingGroups)
            {
                UserGroupDto UserGroupToRemove = new UserGroupDto
                {
                    UserID = usergroup.UserId,
                    GroupID = usergroup.GroupId
                };
                await RemoveMemberFromGroup(UserGroupToRemove);
            }

            await context.Invites.Where(entry => entry.ToUserId == UserID).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            
            
            //Then delete the user
            await context.ShareUsers.Where(entry => entry.UserId == UserID).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            return Ok("Deleted");
        }

        [HttpDelete("DeleteGroup/{GroupID}")]
        public async Task<ActionResult<string>> DeleteGroupOnGroupID(int GroupID)
        {
            //To delete a group, you need to first remove all members from that group

            //So find all the members in the group,
            List<UserGroup> ListOfParticipatingGroupMembers =
                await context.UserGroups.Where(entry => entry.GroupId == GroupID).ToListAsync();
            //Then user RemoveMemberFromGroup for every one of those members
            foreach (var usergroup in ListOfParticipatingGroupMembers)
            {
                UserGroupDto UserGroupToRemove = new UserGroupDto
                {
                    UserID = usergroup.UserId,
                    GroupID = usergroup.GroupId
                };
                await RemoveMemberFromGroup(UserGroupToRemove);
            }

            //Then delete the group
            await context.ShareGroups.Where(entry => entry.GroupId == GroupID).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            return Ok("Deleted");
        }

        //TODO transfer ownership of group
        [HttpPut("TransferGroupOwnership")]
        public async Task<ActionResult<string>> TransferGroupOwnership(TransferGroupOwnershipDto request)
        {
            //Find the previous owner's UsergroupID
            List<UserGroup> GetCurrentOwneruserGroupQuery = await context.UserGroups
                .Where(entry => entry.UserId == request.PreviousOwnerUserID && entry.GroupId == request.GroupID)
                .ToListAsync();

            UserGroup CurrentOwnerUserGroup = GetCurrentOwneruserGroupQuery.First();
            //Set IsOwner to false
            CurrentOwnerUserGroup.IsOwner = false;
            //save changes
            await context.SaveChangesAsync();

            //Find the new Owner's Usergroup
            List<UserGroup> GetNewOwnerUserGroupQuery = await context.UserGroups
                .Where(entry => entry.UserId == request.NewOwnerUserID && entry.GroupId == request.GroupID)
                .ToListAsync();

            UserGroup NewOwnerUserGroup = GetNewOwnerUserGroupQuery.First();
            //Set IsOwner to true
            NewOwnerUserGroup.IsOwner = true;
            //save changes
            await context.SaveChangesAsync();

            return Ok("Transferred Ownership");
        }

        [HttpGet("GetPDFSummaryOnGroupID/{GroupID}")]
        public async Task<ActionResult<Byte[]>> GetPDFSummaryOnGroupID(int GroupID)
        {
            List<MoneyOwedByUserGroupDto> ListOfAllShares = CalculateUserGroupSharesSync.CalculateShares(context, GroupID);

            PDFCreator PDFBuilder = new PDFCreator(ListOfAllShares, context);
            string FileName = PDFBuilder.CreatePDF();
            
            string FilePath = FileName;
            byte[] PDFBytes = System.IO.File.ReadAllBytes(FilePath);
            System.IO.File.Delete(FilePath);
            
            return PDFBytes;
        }
    }
}
