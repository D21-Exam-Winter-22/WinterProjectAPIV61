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
    public class ShareGroupController : ControllerBase
    {
        private readonly PaymentApidb3Context context;

        public ShareGroupController(PaymentApidb3Context context)
        {
            this.context = context;
        }

        [HttpGet("GetAllGroups")]
        public async Task<ActionResult<List<ShareGroup>>> GetAllGroups()
        {
            List<ShareGroup> AllGroupsList = await context.ShareGroups.ToListAsync();
            if (AllGroupsList == null)
            {
                return NotFound(AllGroupsList);
            }
            return Ok(AllGroupsList);
        }      

        [HttpGet("GetGroupDetailsByGroupID/{GroupID}")]
        public async Task<ActionResult<ShareGroup>> GetGroupDetails(int GroupID)
        {
            ShareGroup SelectedGroup = await context.ShareGroups.FindAsync(GroupID);
            return Ok(SelectedGroup);
        }

        [HttpPost("CreateGroup")]
        public async Task<ActionResult<List<ShareGroup>>> CreateGroup(CreateShareGroupDto request)
        {
            //TODO
            //Should we prevent groups with the same name?

            //Create a ShareGroup entry
            ShareGroup GroupToInsert = new ShareGroup
            {
                Name = request.Name,
                Description = request.Description,
                HasConcluded = false,
                IsPublic = request.IsPublic,
                CreationDate = DateTime.Now,
                ConclusionDate = request.ConclusionDate,
                LastActiveDate = DateTime.Now
            };
            //Insert the Group
            await context.ShareGroups.AddAsync(GroupToInsert);
            await context.SaveChangesAsync();

            //Get the ID of inserted ShareGroup
            int NewlyCreatedID = GroupToInsert.GroupId;

            //Create a UserGroup entry
            UserGroup UserGroupToInsert = new UserGroup
            {
                UserId = request.UserID,
                GroupId = NewlyCreatedID,
                IsOwner = true,
            };

            await context.UserGroups.AddAsync(UserGroupToInsert);
            await context.SaveChangesAsync();

            return await GetAllGroups();
        }

        [HttpPut("ConcludeGroup")]
        public async Task<ActionResult<ShareGroup>> ConcludeGroup(int GroupID)
        {
            var GroupToEnd = context.ShareGroups.Find(GroupID);
            if (GroupToEnd != null)
            {
                GroupToEnd.HasConcluded = true;
                GroupToEnd.ConclusionDate = DateTime.Now;
            }
            else
            {
                return NotFound("Group not found");
            }

            await context.SaveChangesAsync();
            ShareGroup ConcludedGroup = await context.ShareGroups.FindAsync(GroupID);
            return Ok(ConcludedGroup);
        }

        [HttpPut("UpdateGroupDetails")]
        public async Task<ActionResult<string>> UpdateGroupDetails(UpdateGroupDetailsDto request)
        {
            //Check if the record exists
            var query = from sharegroup in context.ShareGroups
                        where sharegroup.GroupId == request.GroupID
                        select new
                        {
                            sharegroup.GroupId
                        };

            int rowCounter = 0;
            foreach (var row in query)
            {
                rowCounter++;
            }

            //Find the record
            ShareGroup RecordToChange = context.ShareGroups.Find(request.GroupID);
            
            {
                RecordToChange.Name = request.Name;
                RecordToChange.Description = request.Description;
                RecordToChange.IsPublic = request.IsPublic;
                //RecordToChange.CreationDate = request.CreationDate;
                RecordToChange.ConclusionDate = request.ConclusionDate;
                RecordToChange.LastActiveDate = DateTime.Now;
            }

            //Save the changes
            await context.SaveChangesAsync();

            if (rowCounter == 0)
            {
                return NotFound("Group not found");
            }
            else
            {
                return Ok("Group details updated");
            }
        }

        [HttpPut("ChangeGroupVisibility/{GroupID}")]
        public async Task<ActionResult<string>> ChangeGroupVisibility(int GroupID)
        {
            ShareGroup? TheGroup = await context.ShareGroups.FindAsync(GroupID);
            if (TheGroup == null)
            {
                return NotFound("Invalid GroupID");
            }
            TheGroup.IsPublic = !TheGroup.IsPublic;
            TheGroup.LastActiveDate = DateTime.Now;
            await context.SaveChangesAsync();
            return Ok("Changed Visibility");
        }

        [HttpGet("SearchShareGroups/{SearchString}")]
        public async Task<ActionResult<List<ShareGroup>>> SearchForGroups(string SearchString)
        {
            List<ShareGroup> SearchedGroups = await context.ShareGroups.Where(group => group.Name.Contains(SearchString) || group.Description.Contains(SearchString)).ToListAsync();
            return Ok(SearchedGroups);
        }

        [HttpPut("ArchiveDormantGroups")]
        public async Task<ActionResult<string>> ArchiveDormantGroups()
        {
            //Initialize a counter for number of groups set to dormant
            int DormantSetterCount = 0;
            
            //Get The list of groups
            List<ShareGroup> ListOfAllGroups = await context.ShareGroups.ToListAsync();

            List<ShareGroup> ListOfInactiveGroups =  InactiveTimeCalculator.GetListOfDormantShareGroups(ListOfAllGroups);

            foreach (ShareGroup Group in ListOfInactiveGroups)
            {
                Group.HasConcluded = true;
                await context.SaveChangesAsync();
                DormantSetterCount++;
            }
            
            return Ok(string.Format("Number of groups set to dormant: {0}", DormantSetterCount));
        }

    }
}
