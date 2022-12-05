using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterProjectAPIV61.DataTransferObjects;
using WinterProjectAPIV61.Models;

namespace WinterProjectAPIV61.Functions;

public class CalculateUserGroupSharesSync
{
    
    public static List<MoneyOwedByUserGroupDto> CalculateShares(PaymentApidb3Context context, int GroupID)
    {
        //Get the group's name and description
            ShareGroup TheGroup = context.ShareGroups.Find(GroupID);

            //Calculating how much everyone paid from the group
            List<Expense> AllExpensesQuery = context.Expenses
                .Include(Expense => Expense.UserGroup)
                .Include(Expense => Expense.UserGroup.User)
                .Include(Expense => Expense.UserGroup.Group)
                .Where(UserGroup => UserGroup.UserGroup.GroupId == GroupID)
                .ToList();

            //Query to get the distinct userIDs
            List<UserGroup> DistinctUserIDsQuery =
                context.UserGroups.Where(usergroup => usergroup.GroupId == GroupID).ToList();

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
                List<ShareUser> CurrentUsers = context.ShareUsers.Where(entry => entry.UserId == UserID).ToList();
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
            List<InPayment> ListOfInPayments = context.InPayments
                .Include(InPayment => InPayment.UserGroup)
                .Where(UserGroup => UserGroup.UserGroup.GroupId == GroupID)
                .ToList();

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

            return ListOfMoneyOwedByUsersGroup;
    }
}