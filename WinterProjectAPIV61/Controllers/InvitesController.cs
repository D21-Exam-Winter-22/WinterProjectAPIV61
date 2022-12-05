using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterProjectAPIV61.DataTransferObjects;
using WinterProjectAPIV61.Models;

namespace WinterProjectAPIV61.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitesController : ControllerBase
    {
        private readonly PaymentApidb3Context context;

        public InvitesController(PaymentApidb3Context context)
        {
            this.context = context;
        }

        [HttpGet("GetAllInvitesToUser/{UserID}")]
        public async Task<ActionResult<List<InviteToUserDTO>>> GetAllInvitesToUser(int UserID)
        {
            List<Invite> ListOfInvitesToUser = await context.Invites.Include(invite => invite.FromUserGroup.Group)
                .Include(invite => invite.FromUserGroup.User)
                .Where(entry => entry.ToUserId == UserID).ToListAsync();

            List<InviteToUserDTO> ListOfInvites = new List<InviteToUserDTO>();

            foreach (Invite entry in ListOfInvitesToUser)
            {
                InviteToUserDTO AnInvite = new InviteToUserDTO
                {
                    InviteID = entry.InviteId,
                    IsPending = entry.IsPending,
                    InviteTime = entry.InviteTime,
                    RecieverID = entry.ToUserId,
                    Message = entry.Message,
                    GroupID = (int)entry.FromUserGroup.GroupId,
                    GroupName = entry.FromUserGroup.Group.Name,
                    Description = entry.FromUserGroup.Group.Description,
                    HasConcluded = entry.FromUserGroup.Group.HasConcluded,
                    CreationDate = entry.FromUserGroup.Group.LastActiveDate,
                    LastActiveDate = entry.FromUserGroup.Group.LastActiveDate
                };
                ListOfInvites.Add(AnInvite);
            }

            return Ok(ListOfInvites);
        }

        [HttpPost("InsertInvite")]
        public async Task<ActionResult<string>> InsertInvite(InsertInviteDTO request)
        {
            //Get the usergroupid for that user and group id
            List<UserGroup> UserGroupList = await context.UserGroups
                .Where(entry => entry.UserId == request.FromUserID && entry.GroupId == request.FromGroupID)
                .ToListAsync();

            UserGroup TheUserGroup = UserGroupList.First();

            if (TheUserGroup == null)
            {
                return Ok("Invite sent from a group that the user doesn't belong to");
            }
            
            Invite InviteToInsert = new Invite
            {
                IsPending = true,
                InviteTime = DateTime.Now,
                ToUserId = request.ToUserId,
                FromUserGroupId = TheUserGroup.UserGroupId,
                Message = request.Message
            };
            
            //add it to db
            await context.AddAsync(InviteToInsert);
            await context.SaveChangesAsync();
            //Save db
            
            //Update the group's last active date here
            //get the group
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(request.FromGroupID);
            TheGroup.LastActiveDate = DateTime.Now;

            return Ok("Invite Successfully saved");
        }

        [HttpDelete("DeleteInvite/{InviteID}")]
        public async Task<ActionResult<string>> DeleteInvite(int InviteID)
        {
            //Find the invite
            Invite InviteTODelete = await context.Invites.FindAsync(InviteID);
            context.Invites.Remove(InviteTODelete);
            await context.SaveChangesAsync();
            return Ok("Invite deleted");
        }
    }
}
