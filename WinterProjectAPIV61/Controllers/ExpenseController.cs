using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterProjectAPIV61.DataTransferObjects;
using WinterProjectAPIV61.Models;

namespace WinterProjectAPIV61.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly PaymentApidb3Context context;

        public ExpenseController(PaymentApidb3Context context)
        {
            this.context = context;
        }

        [HttpGet("GetAllExpenses")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> GetAllExpenses()
        {
            var query = from expense in context.Expenses
                        join usergroup in context.UserGroups on expense.UserGroupId equals usergroup.UserGroupId
                        join sharegroup in context.ShareGroups on usergroup.GroupId equals sharegroup.GroupId
                        join shareuser in context.ShareUsers on usergroup.UserId equals shareuser.UserId
                        select new
                        {
                            expense.ExpenseId,
                            expense.Amount,
                            ExpenseName = expense.Name,
                            ExpenseDescription = expense.Description,
                            expense.DatePaid,
                            expense.ReceiptPicture,
                            usergroup.UserId,
                            usergroup.GroupId,
                            sharegroup.Name,
                            GroupDescription = sharegroup.Description,
                            shareuser.UserName,
                            shareuser.PhoneNumber,
                            shareuser.FirstName,
                            shareuser.LastName,
                            shareuser.Email
                        };

            List<GetAllExpensesDto> QueriedList = new List<GetAllExpensesDto>();
            foreach (var record in query)
            {
                QueriedList.Add(new GetAllExpensesDto
                {
                    ExpenseId = record.ExpenseId,
                    Amount = record.Amount,
                    UserId = record.UserId,
                    GroupId = record.GroupId,
                    GroupName = record.Name,
                    UserName = record.UserName,
                    PhoneNumber = record.PhoneNumber,
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    Email = record.Email,
                    ExpenseName = record.ExpenseName,
                    ExpenseDescription = record.ExpenseDescription,
                    DatePaid = record.DatePaid,
                    ReceiptPicture = record.ReceiptPicture,
                    GroupDescription = record.GroupDescription
                });
            }
            if (QueriedList.Count == 0)
            {
                return Ok(QueriedList);
            }
            else
            {
                return Ok(QueriedList);
            }
            //return Ok(await context.Expenses.ToListAsync());
        }

        [HttpGet("GetAllExpensesOnGroupID/{GroupID}")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> getAllExpensesOnGroupID(int GroupID)
        {
            var query = from expense in context.Expenses
                        join usergroup in context.UserGroups on expense.UserGroupId equals usergroup.UserGroupId
                        join sharegroup in context.ShareGroups on usergroup.GroupId equals sharegroup.GroupId
                        join shareuser in context.ShareUsers on usergroup.UserId equals shareuser.UserId
                        where usergroup.GroupId == GroupID
                        select new
                        {
                            expense.ExpenseId,
                            expense.Amount,
                            ExpenseName = expense.Name,
                            ExpenseDescription = expense.Description,
                            expense.DatePaid,
                            expense.ReceiptPicture,
                            usergroup.UserId,
                            usergroup.GroupId,
                            sharegroup.Name,
                            GroupDescription = sharegroup.Description,
                            shareuser.UserName,
                            shareuser.PhoneNumber,
                            shareuser.FirstName,
                            shareuser.LastName,
                            shareuser.Email,

                        };
            List<GetAllExpensesDto> QueriedList = new List<GetAllExpensesDto>();
            foreach (var record in query)
            {
                QueriedList.Add(new GetAllExpensesDto
                {
                    ExpenseId = record.ExpenseId,
                    Amount = record.Amount,
                    UserId = record.UserId,
                    GroupId = record.GroupId,
                    GroupName = record.Name,
                    UserName = record.UserName,
                    PhoneNumber = record.PhoneNumber,
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    Email = record.Email,
                    ExpenseName = record.ExpenseName,
                    ExpenseDescription = record.ExpenseDescription,
                    DatePaid = record.DatePaid,
                    ReceiptPicture = record.ReceiptPicture,
                    GroupDescription = record.GroupDescription
                });
            }

            if (QueriedList.Count == 0)
            {
                return NotFound(QueriedList);
            }
            else
            {
                return Ok(QueriedList);
            }
        }

        [HttpPost("InsertExpenditure")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> InsertNewExpenditure(CreateExpenditureDto request)
        {
            //Get the UserGroupID
            var query = from usergroup in context.UserGroups
                        where usergroup.GroupId == request.GroupID &&
                              usergroup.UserId == request.UserID
                        select new
                        {
                            usergroup.UserGroupId
                        };


            int UserGroupID = -1;
            foreach (var usergroup in query)
            {
                UserGroupID = usergroup.UserGroupId;
            }
            //If the Usergroup for that expenditure doesn't exist
            if (UserGroupID == -1)
            {
                return NotFound(getAllExpensesOnGroupID(request.GroupID));
                //return BadRequest("Bad request");
            }

            //Insert into Expenses using the amount and UserGroupID

            Expense ExpenseToInsert = new Expense
            {
                UserGroupId = UserGroupID,
                Amount = request.Amount,
                Name = request.Name,
                Description = request.Description,
                DatePaid = DateTime.Now,
                ReceiptPicture = request.ReceiptPicture                
            };
            
            
            //Update the last active attribute of this group
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(request.GroupID);
            TheGroup.LastActiveDate = DateTime.Now;
            await context.SaveChangesAsync();
            context.Expenses.Add(ExpenseToInsert);
            await context.SaveChangesAsync();

            return await getAllExpensesOnGroupID(request.GroupID);
        }

        [HttpDelete("DeleteExpenditure/{ExpenseID}")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> DeleteExpenditureOnID(int ExpenseID)
        {
            //Get the UserGroupID of this expenseID
            //Get the GroupID associate with the UserGroupID
            var GetUserGroupIDQuery = from expense in context.Expenses
                                      join usergroup in context.UserGroups on expense.UserGroupId equals usergroup.UserGroupId
                                      where expense.ExpenseId == ExpenseID
                                      select new
                                      {
                                          expense.UserGroupId,
                                          usergroup.UserId,
                                          usergroup.GroupId
                                      };
            int? UserGroupID = -1;
            int? UserID = -1;
            int? GroupID = -1;
            foreach (var record in GetUserGroupIDQuery)
            {
                UserGroupID = record.UserGroupId;
                UserID = record.UserId;
                GroupID = record.GroupId;
            }
            
            //Update group activity date
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(GroupID);
            TheGroup.LastActiveDate = DateTime.Now;
            await context.SaveChangesAsync();
            //Delete the expenseID from the table
            await context.Expenses.Where(x => x.ExpenseId == ExpenseID).ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            //Get all expenses for this GroupID
            return await getAllExpensesOnGroupID((int)GroupID);
        }

        [HttpGet("GetPersonalExpenses/{UserID}")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> GetAllPersonalExpenses(int UserID)
        {
            //Join Expenses to UserGroup to Group and User

            var ListOfExpensesQuery = from expense in context.Expenses
                                      join usergroup in context.UserGroups on expense.UserGroupId equals usergroup.UserGroupId
                                      join user in context.ShareUsers on usergroup.UserId equals user.UserId
                                      join sharegroup in context.ShareGroups on usergroup.GroupId equals sharegroup.GroupId
                                      where user.UserId == UserID
                                      select new
                                      {
                                          expense.ExpenseId,
                                          expense.Amount,
                                          ExpenseName = expense.Name,
                                          ExpenseDescription = expense.Description,
                                          expense.DatePaid,
                                          expense.ReceiptPicture,
                                          SelectedUserID = user.UserId,
                                          user.UserName,
                                          user.FirstName,
                                          user.LastName,
                                          user.PhoneNumber,
                                          user.Email,
                                          sharegroup.GroupId,
                                          GroupName = sharegroup.Name,
                                          GroupDescription = sharegroup.Description
                                      };

            List<GetAllExpensesDto> ListOfPersonalExpenses = new List<GetAllExpensesDto>();

            foreach (var expense in ListOfExpensesQuery)
            {
                GetAllExpensesDto PersonalExpense = new GetAllExpensesDto
                {
                    ExpenseId = expense.ExpenseId,
                    Amount = expense.Amount,
                    UserId = expense.SelectedUserID,
                    GroupId = expense.GroupId,
                    GroupName = expense.GroupName,
                    UserName = expense.UserName,
                    PhoneNumber = expense.PhoneNumber,
                    FirstName = expense.FirstName,
                    LastName = expense.LastName,
                    Email = expense.Email,
                    ExpenseName = expense.ExpenseName,
                    ExpenseDescription = expense.ExpenseDescription,
                    DatePaid = expense.DatePaid,
                    ReceiptPicture = expense.ReceiptPicture,
                    GroupDescription = expense.GroupDescription
                };
                ListOfPersonalExpenses.Add(PersonalExpense);
            }

            if (ListOfExpensesQuery.Count() == 0)
            {
                return NotFound(ListOfPersonalExpenses);
            }

            return Ok(ListOfPersonalExpenses);
        }

        [HttpPut("EditAnExpense")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> EditExpenditure(Expense request)
        {
            //Check if such an expenditure even exists
            var CheckIfExistsQuery = from expense in context.Expenses
                                     where expense.ExpenseId == request.ExpenseId
                                     select new
                                     {
                                         expense.ExpenseId,
                                         expense.UserGroupId
                                     };
            int counter = 0;
            int UserGroupID = -1;
            foreach (var entry in CheckIfExistsQuery)
            {
                counter++;
                UserGroupID = (int)entry.UserGroupId;
            }

            if (counter == 0)
            {
                return NotFound();
            }

            //Find the expenditure
            List<Expense> ListOfRecords = await context.Expenses.Where(expense => expense.ExpenseId == request.ExpenseId).ToListAsync();
            Expense RecordToUpdate = ListOfRecords.First();

            //Update them
            {
                RecordToUpdate.Amount = request.Amount;
                RecordToUpdate.Name = request.Name;
                RecordToUpdate.Description = request.Description;
                RecordToUpdate.DatePaid = request.DatePaid;
                RecordToUpdate.ReceiptPicture = request.ReceiptPicture;
            }
            await context.SaveChangesAsync();
            
            //Find the Relevant group and change it's Last active date
            UserGroup UserGroupsExpense = await context.UserGroups.FindAsync(UserGroupID);
            ShareGroup TheGroup = await context.ShareGroups.FindAsync(UserGroupsExpense.GroupId);
            TheGroup.LastActiveDate = DateTime.Now;
            
            await context.SaveChangesAsync();

            //Get the userID for the usergroupid

            UserGroup UserGroup = context.UserGroups.Find(UserGroupID);
            int UserID = (int)UserGroup.UserId;

            return await GetAllPersonalExpenses(UserID);
        }

        [HttpGet("GetExpense/{ExpenseID}")]
        public async Task<ActionResult<Expense>> GetExpenseOnExpenseID(int ExpenseID)
        {
            return Ok(context.Expenses.Find(ExpenseID));
        }

        [HttpGet("SearchExpenses/{SearchString}")]
        public async Task<ActionResult<List<GetExpenseDTO>>> SearchForExpenses(string SearchString)
        {
            List<Expense> SearchedExpenses = await context.Expenses.Where(expense => expense.Name.Contains(SearchString) || expense.Description.Contains(SearchString)).ToListAsync();

            List<GetExpenseDTO> ListOfSearchedExpenses = new List<GetExpenseDTO>();

            foreach (var SingleExpense in SearchedExpenses)
            {
                GetExpenseDTO expense = new GetExpenseDTO
                {
                    ExpenseId = SingleExpense.ExpenseId,
                    UserGroupId = SingleExpense.UserGroupId,
                    Amount = SingleExpense.Amount,
                    Name = SingleExpense.Name,
                    Description = SingleExpense.Description,
                    DatePaid = SingleExpense.DatePaid,
                    ReceiptPicture = SingleExpense.ReceiptPicture
                };
                ListOfSearchedExpenses.Add(expense);
            }
            
            return Ok(ListOfSearchedExpenses);
        }
    }
}
