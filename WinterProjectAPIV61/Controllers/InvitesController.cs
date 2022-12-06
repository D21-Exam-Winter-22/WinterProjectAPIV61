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

        [HttpGet("GetAllInvitesToUser/{UserId}")]
        public async Task<ActionResult<List<InviteToUserDTO>>> GetAllInvitesToUser(int UserId)
        {
            List<Invite> ListOfInvitesToUser = await context.Invites.Include(invite => invite.FromUserGroup.Group)
                .Include(invite => invite.FromUserGroup.User)
                .Where(entry => entry.ToUserId == UserId).ToListAsync();

            List<InviteToUserDTO> ListOfInvites = new List<InviteToUserDTO>();

            foreach (Invite entry in ListOfInvitesToUser)
            {
                InviteToUserDTO AnInvite = new InviteToUserDTO
                {
                    InviteId = entry.InviteId,
                    IsPending = entry.IsPending,
                    InviteTime = entry.InviteTime,
                    RecieverId = entry.ToUserId,
                    Message = entry.Message,
                    GroupId = (int)entry.FromUserGroup.GroupId,
                    SenderId = (int)entry.FromUserGroup.UserId,
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
                .Where(entry => entry.UserId == request.FromUserId && entry.GroupId == request.FromGroupId)
                .ToListAsync();

            if (UserGroupList.Count == 0)
            {
                return Ok("Invite sent from a group that the user doesn't belong to");
            }
            
            UserGroup TheUserGroup = UserGroupList.First();

           
            
            Invite InviteToInsert = new Invite
            {
                IsPending = true,
                InviteTime = DateTime.Now,
                ToUserId = request.ToUserId,
                FromUserGroupId = TheUserGroup.UserGroupId,
                Message = request.Message
            };
            
            //Check if there is already an invite from this person for that group
            List<Invite> ListOfExistingInvites = await context.Invites
                .Where(invite => invite.FromUserGroupId == InviteToInsert.FromUserGroupId && invite.ToUserId == request.ToUserId && invite.IsPending == true).ToListAsync();

            if (ListOfExistingInvites.Count > 0)
            {
                return Ok("User has already received invites from this user for this group");
            }
            
            //Check if the user has any invites from any members of that group
            List<Invite> ListOfInvitesToGroup =
                await context.Invites.Where(entry => entry.FromUserGroup.GroupId == request.FromGroupId && entry.IsPending == true).ToListAsync();

            if (ListOfInvitesToGroup.Count > 0)
            {
                return Ok("User has pending invites from other members in the group");
            }
            
            //Check if the reciever already belongs to that group
                //Get the list of groups that the receiver is already a part of
                //Check if the groupID of the FromUserGroupID is in the list of groups that the user is already a part of
                List<UserGroup> ListOfReceiversGroups = await context.UserGroups
                    .Where(entry => entry.GroupId == request.FromGroupId && entry.UserId == request.ToUserId).ToListAsync();

                foreach (UserGroup entry in ListOfReceiversGroups)
                {
                    if (entry.GroupId == request.FromGroupId)
                    {
                        return Ok("Receiver is already in the group");
                    }
                }                
            
            //add it to db
            await context.AddAsync(InviteToInsert);
            await context.SaveChangesAsync();
            //Save db
            
            //Update the group's last active date here
            //get the group
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(request.FromGroupId);
            TheGroup.LastActiveDate = DateTime.Now;
            await context.SaveChangesAsync();

            return Ok("Invite Successfully saved");
        }

        [HttpDelete("DeleteInvite/{InviteId}")]
        public async Task<ActionResult<string>> DeleteInvite(int InviteId)
        {
            //Find the invite
            Invite InviteTODelete = await context.Invites.FindAsync(InviteId);
            context.Invites.Remove(InviteTODelete);
            await context.SaveChangesAsync();
            return Ok("Invite deleted");
        }

        [HttpPut("EndInvite/{InviteId}")]
        public async Task<ActionResult<string>> EndInvite(int InviteId)
        {
            //Find the invite
            Invite TheInvite = await context.Invites.FindAsync(InviteId);

            if (TheInvite == null)
            {
                return Ok("Invite not found");
            }
            else
            {
                TheInvite.IsPending = false;
                await context.SaveChangesAsync();
                return Ok("Invite has been terminated");
            }
        }
    }
}
