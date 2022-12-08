using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterProjectAPIV61.DataTransferObjects;
using WinterProjectAPIV61.Models;

namespace WinterProjectAPIV61.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InPaymentController : ControllerBase
    {
        private readonly PaymentApidb3Context context;

        public InPaymentController(PaymentApidb3Context context)
        {
            this.context = context;
        }

        [HttpPost("PayIntoGroupPool")]
        public async Task<ActionResult<string>> PayIntoGroupPool(CreatePaymentDTO request)
        {
            //Get the usergroupid for that userid and groupid
            List<UserGroup> ListOfUserGroups = await context.UserGroups.Where(entry =>
                entry.UserId == request.UserId && entry.GroupId == request.GroupId).ToListAsync();

            if (ListOfUserGroups.Count == 0)
            {
                return Ok("UserGroup doesn't exist");
            }

            UserGroup TheUserGroup = ListOfUserGroups.First();
            int TheUserGroupID = TheUserGroup.UserGroupId;


            //Create an InPayment Object
            InPayment InPayment = new InPayment
            {
                UserGroupId = TheUserGroupID,
                Amount = request.Amount,
                DatePaid = DateTime.Now
            };
            //Insert it into the DB
            context.InPayments.Add(InPayment);
            await context.SaveChangesAsync();

            //Get GroupID from UserGroupID
            /*
            var GetGroupIDQuery = from usergroup in context.UserGroups
                                  where usergroup.UserGroupId == TheUserGroupID
                                  select new
                                  {
                                      usergroup.UserGroupId,
                                      usergroup.UserId,
                                      usergroup.GroupId
                                  };
            
            
            foreach (var record in GetGroupIDQuery)
            {
                GroupID = (int)record.GroupId;
            }
            */
            int GroupID = (int)request.GroupId;
            //Update the group's Last active date
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(GroupID);
            TheGroup.LastActiveDate = DateTime.Now;
            await context.SaveChangesAsync();
            //Using the GroupID, query for all expenditures in the group
            return Ok("Paid into pool");
        }

        [HttpPut("EditInPayment")]
        public async Task<ActionResult<string>> EditAnInPayment(InPaymentDto request)
        {
            //Get the particular inpayment entry
            InPayment RecordToUpdate = context.InPayments.Find(request.TransactionID);

            //Update the details
            {
                RecordToUpdate.UserGroupId = request.UserGroupId;
                RecordToUpdate.Amount = request.Amount;
                RecordToUpdate.DatePaid = request.DatePaid;
            }
            await context.SaveChangesAsync();
            //Find the group
            UserGroup TheUserGroup = await context.UserGroups.FindAsync(request.UserGroupId);
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(TheUserGroup.GroupId);
            TheGroup.LastActiveDate = DateTime.Now;

            await context.SaveChangesAsync();
            return Ok("Payment Edited");
        }

        [HttpGet("GetAllUserInPayments/{UserID}")]
        public async Task<ActionResult<List<UserInPaymentsDto>>> GetAllInPayments(int UserID)
        {
            var GetAllUserInPaymentsQuery = from inpayments in context.InPayments
                                            join usergroup in context.UserGroups on inpayments.UserGroupId equals usergroup.UserGroupId
                                            join shareUser in context.ShareUsers on usergroup.UserId equals shareUser.UserId
                                            where shareUser.UserId == UserID
                                            select new
                                            {
                                                inpayments.TransactionId,
                                                inpayments.Amount,
                                                usergroup.UserGroupId,
                                                GroupID = usergroup.GroupId,
                                                shareUser.UserId,
                                                shareUser.UserName,
                                                shareUser.FirstName,
                                                shareUser.LastName,
                                                shareUser.PhoneNumber,
                                                shareUser.Email
                                            };
            List<UserInPaymentsDto> ListOfUserInpayments = new List<UserInPaymentsDto>();
            foreach (var entry in GetAllUserInPaymentsQuery)
            {
                UserInPaymentsDto NewEntry = new UserInPaymentsDto
                {
                    TransactionID = entry.TransactionId,
                    Amount = entry.Amount,
                    UserGroupID = entry.UserGroupId,
                    GroupID = (int)entry.GroupID,
                    UserID = entry.UserId,
                    UserName = entry.UserName,
                    FirstName = entry.FirstName,
                    LastName = entry.LastName,
                    PhoneNumber = entry.PhoneNumber,
                    Email = entry.Email

                };
                ListOfUserInpayments.Add(NewEntry);
            }
            return Ok(ListOfUserInpayments);
        }

        [HttpGet("GetAllGroupInPayments/{GroupID}")]
        public async Task<ActionResult<List<InPaymentDto>>> GetAllGroupInPayments(int GroupID)
        {
            var GroupInPaymentsQuery = from inpayments in context.InPayments
                                       join usergroup in context.UserGroups on inpayments.UserGroupId equals usergroup.UserGroupId
                                       join user in context.ShareUsers on usergroup.UserId equals user.UserId
                                       join sharegroup in context.ShareGroups on usergroup.GroupId equals sharegroup.GroupId
                                       where usergroup.GroupId == GroupID
                                       select new
                                       {
                                           inpayments.TransactionId,
                                           inpayments.Amount,
                                           usergroup.UserGroupId,
                                           sharegroup.GroupId,
                                           sharegroup.Name,
                                           sharegroup.Description,
                                           user.UserId,
                                           user.UserName,
                                           user.FirstName,
                                           user.LastName,
                                           user.PhoneNumber,
                                           user.Email
                                       };

            List<UserGroupPaymentDto> ListOfGroupInpayments = new List<UserGroupPaymentDto>();

            foreach (var entry in GroupInPaymentsQuery)
            {
                UserGroupPaymentDto NewEntry = new UserGroupPaymentDto
                {
                    TransactionID = entry.TransactionId,
                    PaidAmount = entry.Amount,
                    UserGroupID = entry.UserGroupId,
                    GroupID = entry.GroupId,
                    GroupName = entry.Name,
                    GroupDescription = entry.Description,
                    UserID = entry.UserId,
                    UserName = entry.UserName,
                    FirstName = entry.FirstName,
                    LastName = entry.LastName,
                    PhoneNumber = entry.PhoneNumber,
                    Email = entry.Email
                };
                ListOfGroupInpayments.Add(NewEntry);
            }
            return Ok(ListOfGroupInpayments);
        }

        [HttpGet("GetInPayment/{TransactionID}")]
        public async Task<ActionResult<InPayment>> GetInPaymentOnTransactionID(int TransactionID)
        {
            return Ok(context.InPayments.Find(TransactionID));
        }


        [HttpDelete("DeleteInPayment/{TransactionID}")]
        public async Task<ActionResult<string>> DeleteInPaymentOnTransactionID(int TransactionID)
        {
            await context.InPayments.Where(entry => entry.TransactionId == TransactionID).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            
            //Find the usergroup on this transactionID
            InPayment TheInPayment = await context.InPayments.FindAsync(TransactionID);
            UserGroup TheUserGroup = await context.UserGroups.FindAsync(TheInPayment.UserGroupId);
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(TheUserGroup.GroupId);
            
            //Update the group's last active date
            TheGroup.LastActiveDate = DateTime.Now;
            
            await context.SaveChangesAsync();
            return Ok("Deleted");
        }
    }
}
