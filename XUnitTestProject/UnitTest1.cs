using System.Buffers.Text;
using WinterProjectAPIV61.Functions;
using WinterProjectAPIV61.Models;
using Base64 = WinterProjectAPIV61.Functions.Base64;

namespace XUnitTestProject;

public class UnitTest1
{
    [Fact]
    public void TokenEncodeTest()
    {
        string StringToEncode = "UsernamePassword";

        string ExpectedEncoding = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(StringToEncode));

        string ActualEncoding = Base64.Encode(StringToEncode);

        Assert.Equal(ExpectedEncoding, ActualEncoding);
    }

    [Fact]
    public void RandomStringGeneratorTest1()
    {
        string ActualPassword = GenerateRandomString.CreateString(10);

        Assert.DoesNotContain("Ø", ActualPassword);
    }

    [Fact]
    public void RandomStringGeneratorTest2()
    {
        string ActualPassword = GenerateRandomString.CreateString(10);
        Assert.DoesNotContain("Å", ActualPassword);
    }
    
    [Fact]
    public void RandomStringGeneratorTest3()
    {
        string ActualPassword = GenerateRandomString.CreateString(10);
        Assert.DoesNotContain("Æ", ActualPassword);
    }

    [Fact]
    public void CalculateDormantDaysTest1()
    {
        //Testing negative difference
        double ExpectedDifference = -60.0;
        
        DateTime CurrentTime = DateTime.Now;
        DateTime FutureTime = CurrentTime.AddDays(-ExpectedDifference);

        double ActualDifferenceInDays = InactiveTimeCalculator.CalculateDormantDays(FutureTime);
        
        Assert.Equal(ExpectedDifference, ActualDifferenceInDays, 5);

    }
    
    [Fact]
    public void CalculateDormantDaysTest2()
    {
        //Testing no difference
        double ExpectedDifference = 0;
        DateTime CurrentTime = DateTime.Now;

        double ActualDifference = InactiveTimeCalculator.CalculateDormantDays(CurrentTime);
        
        Assert.Equal(ExpectedDifference, ActualDifference, 5);

    }
    
    [Fact]
    public void CalculateDormantDaysTest3()
    {
        //Testing positive difference
        double ExpectedDifference = 60.0;
        
        DateTime CurrentTime = DateTime.Now;
        DateTime FutureTime = CurrentTime.AddDays(-ExpectedDifference);
        
        double ActualDifferenceInDays = InactiveTimeCalculator.CalculateDormantDays(FutureTime);
        
        Assert.Equal(ExpectedDifference, ActualDifferenceInDays, 5);
    }

    [Fact]
    public void GetListOfDormantShareGroupsTest0()
    {
        //Create an empty list of groups
        List<ShareGroup> ListOfAllGroups = new List<ShareGroup>();
        
        //The method should be able to return an empty list without throwing errors.
        List<ShareGroup> ActualListOfDormantGroups = InactiveTimeCalculator.GetListOfDormantShareGroups(ListOfAllGroups);
        
        //The list should be empty because None of them were inactive for long enough
        
        Assert.Empty(ActualListOfDormantGroups);
    }

    [Fact]
    public void GetListOfDormantShareGroupsTest1()
    {
        //insert 3 ShareGroups into the list
        //Make all 3 LastActiveDateTimes below the threshold so that none of them are added to the dormant list
        
        ShareGroup Group1 = new ShareGroup
        {
            GroupId = 1,
            LastActiveDate = DateTime.Now.AddDays(-15)
        };

        ShareGroup Group2 = new ShareGroup
        {
            GroupId = 2,
            LastActiveDate = DateTime.Now.AddDays(-20)
        };

        ShareGroup Group3 = new ShareGroup
        {
            GroupId = 3,
            LastActiveDate = DateTime.Now.AddDays(-25)
        };

        List<ShareGroup> ListOfAllGroups = new List<ShareGroup>();
        ListOfAllGroups.Add(Group1);
        ListOfAllGroups.Add(Group2);
        ListOfAllGroups.Add(Group3);
        
        List<ShareGroup> ActualListOfDormantGroups = InactiveTimeCalculator.GetListOfDormantShareGroups(ListOfAllGroups);
        
        //The list should be empty because None of them were inactive for long enough
        
        Assert.Empty(ActualListOfDormantGroups);
    }
    
    [Fact]
    public void GetListOfDormantShareGroupsTest2()
    {
        //insert 3 ShareGroups into the list
        //Make all 3 lastActiveDateTimes above the threshold so that all of them are added to the dormant list

        ShareGroup Group1 = new ShareGroup
        {
            GroupId = 1,
            LastActiveDate = DateTime.Now.AddDays(-60)
        };

        ShareGroup Group2 = new ShareGroup
        {
            GroupId = 2,
            LastActiveDate = DateTime.Now.AddDays(-45)
        };

        ShareGroup Group3 = new ShareGroup
        {
            GroupId = 3,
            LastActiveDate = DateTime.Now.AddDays(-90)
        };

        List<ShareGroup> ListOfAllGroups = new List<ShareGroup>();
        ListOfAllGroups.Add(Group1);
        ListOfAllGroups.Add(Group2);
        ListOfAllGroups.Add(Group3);

        List<ShareGroup> ActualListOfDormantGroups = InactiveTimeCalculator.GetListOfDormantShareGroups(ListOfAllGroups);
        
        List<ShareGroup> ExpectedListOfDormantGroups = new List<ShareGroup>();
        ExpectedListOfDormantGroups.Add(Group1);
        ExpectedListOfDormantGroups.Add(Group2);
        ExpectedListOfDormantGroups.Add(Group3);
        
        Assert.Equal(ExpectedListOfDormantGroups, ActualListOfDormantGroups);
    }
    
    [Fact]
    public void GetListOfDormantShareGroupsTest3()
    {
        //insert 3 ShareGroups into the list
        //Make all 3 groups have lastActiveDateTimes as the threshold so they enter the if statement
        
        ShareGroup Group1 = new ShareGroup
        {
            GroupId = 1,
            LastActiveDate = DateTime.Now.AddDays(-30)
        };

        ShareGroup Group2 = new ShareGroup
        {
            GroupId = 2,
            LastActiveDate = DateTime.Now.AddDays(-30)
        };

        ShareGroup Group3 = new ShareGroup
        {
            GroupId = 3,
            LastActiveDate = DateTime.Now.AddDays(-30)
        };

        List<ShareGroup> ListOfAllGroups = new List<ShareGroup>();
        ListOfAllGroups.Add(Group1);
        ListOfAllGroups.Add(Group2);
        ListOfAllGroups.Add(Group3);
        
        List<ShareGroup> ActualListOfDormantGroups = InactiveTimeCalculator.GetListOfDormantShareGroups(ListOfAllGroups);
        
        List<ShareGroup> ExpectedListOfDormantGroups = new List<ShareGroup>();
        ExpectedListOfDormantGroups.Add(Group1);
        ExpectedListOfDormantGroups.Add(Group2);
        ExpectedListOfDormantGroups.Add(Group3);
        
        Assert.Equal(ExpectedListOfDormantGroups, ActualListOfDormantGroups);
    }
    
    [Fact]
    public void GetListOfDormantShareGroupsTest4()
    {
        //insert 3 ShareGroups into the list
        //1 group should be dormant and 2 groups should be active
        
        ShareGroup Group1 = new ShareGroup
        {
            GroupId = 1,
            LastActiveDate = DateTime.Now.AddDays(-90)
        };

        ShareGroup Group2 = new ShareGroup
        {
            GroupId = 2,
            LastActiveDate = DateTime.Now.AddDays(-25)
        };

        ShareGroup Group3 = new ShareGroup
        {
            GroupId = 3,
            LastActiveDate = DateTime.Now.AddDays(-5)
        };

        List<ShareGroup> ListOfAllGroups = new List<ShareGroup>();
        ListOfAllGroups.Add(Group1);
        ListOfAllGroups.Add(Group2);
        ListOfAllGroups.Add(Group3);
        
        List<ShareGroup> ActualListOfDormantGroups = InactiveTimeCalculator.GetListOfDormantShareGroups(ListOfAllGroups);
        
        List<ShareGroup> ExpectedListOfDormantGroups = new List<ShareGroup>();
        ExpectedListOfDormantGroups.Add(Group1);
        
        Assert.Equal(ExpectedListOfDormantGroups, ActualListOfDormantGroups);
    }
    
    [Fact]
    public void GetListOfDormantShareGroupsTest5()
    {
        //insert 3 ShareGroups into the list
        //2 groups should be dormant and 1 group should be active
        
        ShareGroup Group1 = new ShareGroup
        {
            GroupId = 1,
            LastActiveDate = DateTime.Now.AddDays(-90)
        };

        ShareGroup Group2 = new ShareGroup
        {
            GroupId = 2,
            LastActiveDate = DateTime.Now.AddDays(-60)
        };

        ShareGroup Group3 = new ShareGroup
        {
            GroupId = 3,
            LastActiveDate = DateTime.Now.AddDays(-5)
        };

        List<ShareGroup> ListOfAllGroups = new List<ShareGroup>();
        ListOfAllGroups.Add(Group1);
        ListOfAllGroups.Add(Group2);
        ListOfAllGroups.Add(Group3);
        
        List<ShareGroup> ActualListOfDormantGroups = InactiveTimeCalculator.GetListOfDormantShareGroups(ListOfAllGroups);
        
        List<ShareGroup> ExpectedListOfDormantGroups = new List<ShareGroup>();
        ExpectedListOfDormantGroups.Add(Group1);
        ExpectedListOfDormantGroups.Add(Group2);
        
        Assert.Equal(ExpectedListOfDormantGroups, ActualListOfDormantGroups);
    }

    [Fact]
    public void LogOutTest0()
    {
        //Testing when there are no key value pairs in the dictionary
        Dictionary<int, string> TokenDictionary = new Dictionary<int, string>();
        string ActualResponse = LogOutClass.LogOut(TokenDictionary, 1);
        string ExpectedResponse = "No users currently logged in";
        Assert.Equal(ExpectedResponse, ActualResponse);

    }

    [Fact]
    public void LogOutTest1()
    {
        //Testing when there is the correct key value pair in the dictionary
        Dictionary<int, string> TokenDictionary = new Dictionary<int, string>();
        TokenDictionary.Add(1, "Token1");
        TokenDictionary.Add(2, "Token2");
        TokenDictionary.Add(3, "Token3");
        string ActualResponse = LogOutClass.LogOut(TokenDictionary, 1);
        string ExpectedResponse = "Successfully Logged out";
        Assert.Equal(ExpectedResponse, ActualResponse);
    }

    [Fact]
    public void LogOuttest2()
    {
        //Testing when the correct key value pair is not present in the dictionary
        Dictionary<int, string> TokenDictionary = new Dictionary<int, string>();
        TokenDictionary.Add(1, "Token1");
        TokenDictionary.Add(2, "Token2");
        TokenDictionary.Add(3, "Token3");
        string ActualResponse = LogOutClass.LogOut(TokenDictionary, 4);
        string ExpectedResponse = "Token not found";
        Assert.Equal(ExpectedResponse, ActualResponse);
    }
}