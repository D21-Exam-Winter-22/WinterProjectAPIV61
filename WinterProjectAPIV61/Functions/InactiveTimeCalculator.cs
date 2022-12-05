using WinterProjectAPIV61.Models;

namespace WinterProjectAPIV61.Functions;

public class InactiveTimeCalculator
{
    public static TimeSpan CalculateDormantTimeSpan(DateTime LastActiveDateTime)
    {
        TimeSpan DifferenceInDates = DateTime.Now - LastActiveDateTime;
        return DifferenceInDates;
    }

    public static double CalculateDormantDays(DateTime? LastActiveDateTime)
    {
        TimeSpan DifferenceInDates = (TimeSpan)(DateTime.Now - LastActiveDateTime);
        double DifferenceInDays = DifferenceInDates.TotalDays;
        return DifferenceInDays;
    }

    public static List<ShareGroup> GetListOfDormantShareGroups(List<ShareGroup> ListOfAllGroups)
    {
        List<ShareGroup> ListOfAllDormantGroups = new List<ShareGroup>();
        foreach (ShareGroup Group in ListOfAllGroups)
        {
            //Calculate the amount of inactive time in days
            double DifferenceInDays = CalculateDormantDays(Group.LastActiveDate);
                
            //If the groups have been dormant for more than 30 days, conclude them
            if (DifferenceInDays >= 30)
            {
               ListOfAllDormantGroups.Add(Group);
            }
        }

        return ListOfAllDormantGroups;
    } 
}