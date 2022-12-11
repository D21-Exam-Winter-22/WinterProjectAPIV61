using iTextSharp.text;
using iTextSharp.text.pdf;
using WinterProjectAPIV61.DataTransferObjects;
using WinterProjectAPIV61.Models;

namespace WinterProjectAPIV61.PDFGenerator
{
    public class PDFCreator : IBuilder
    {
        private List<MoneyOwedByUserGroupDto> ListOfUserGroupShares;
        private PaymentApidb3Context context;
        public PDFCreator(List<MoneyOwedByUserGroupDto> ListOfUserGroupShares, PaymentApidb3Context context)
        {
            this.ListOfUserGroupShares = ListOfUserGroupShares;
            this.context = context;
        }

        public string CreatePDF()
        {
            string FileName = string.Format("{0} Summary.pdf", GetGroupame());
            FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
            Document doc = new Document();
            using (PdfWriter writer = PdfWriter.GetInstance(doc, fs))
            {
                doc.Open();
                AddTitle(doc);
                AddGroupNameDescription(doc);
                AddEmptyLines(doc, 1);
                AddGroupsTotalExpenses(doc);
                AddGroupsTotalInPayments(doc);
                AddGroupsTotalPendingDebt(doc);
                AddEmptyLines(doc, 2);
                AddAllIndividualAccounting(doc);
                AddDueDateIfGroupConcluded(doc);
                doc.Close();
            }

            return FileName;
        }

        public string GetGroupame()
        {
            string GroupName = ListOfUserGroupShares.First().GroupName;
            return GroupName;
        }

        public string GetGroupDescription()
        {
            string Description = ListOfUserGroupShares.First().GroupDescription;
            return Description;
        }

        public void AddEmptyLines(Document doc, int NumOfLines)
        {
            for (int i = 0; i < NumOfLines; i++)
            {
                doc.Add(Chunk.NEWLINE);
            }
        }

        public void InsertCustomParagraphIntoDocument(Document doc, string text, Font.FontFamily fontFamily, BaseColor Color, int FontSize, int Alignment) //0 is left, 1 is center, 2 is right
        {
            Font CustomFont = new Font(fontFamily);
            CustomFont.Color = Color;
            CustomFont.Size = FontSize;
            Paragraph CustomParagraph = new Paragraph(text, CustomFont);
            CustomParagraph.Alignment = Alignment;
            doc.Add(CustomParagraph);
        }

        public void AddTitle(Document doc)
        {
            string Title = string.Format("{0}", GetGroupame());
            InsertCustomParagraphIntoDocument(doc, Title, Font.FontFamily.COURIER, BaseColor.DARK_GRAY, 20, Element.ALIGN_CENTER);
        }

        public void AddGroupNameDescription(Document doc)
        {
            string Description = "1. Calcuations summary";
            
            InsertCustomParagraphIntoDocument(doc, Description, Font.FontFamily.COURIER, BaseColor.BLACK, 15, Element.ALIGN_LEFT);
        }

        public void AddGroupsTotalExpenses(Document doc)
        {
            //Calculate the total expenses of the group
            double TotalExpenses = 0;
            ListOfUserGroupShares.ForEach(entry => TotalExpenses += entry.AmountPaidDuringGroup);

            string TotalExpensesString = string.Format("Total Expenses of the group = {0} DKK", TotalExpenses);
            
            InsertCustomParagraphIntoDocument(doc, TotalExpensesString, Font.FontFamily.TIMES_ROMAN, BaseColor.BLACK, 12, Element.ALIGN_LEFT);
        }

        public void AddGroupsTotalInPayments(Document doc)
        {
            //Calculate the total In payments
            double TotalInPayments = 0;
            ListOfUserGroupShares.ForEach(entry => TotalInPayments += entry.AmountAlreadyPaid);
            
            string TotalInPaymentsString = string.Format("Total In-payments of the group = {0} DKK", TotalInPayments);
            
            InsertCustomParagraphIntoDocument(doc, TotalInPaymentsString, Font.FontFamily.TIMES_ROMAN, BaseColor.BLACK, 12, Element.ALIGN_LEFT);
        }

        public void AddGroupsTotalPendingDebt(Document doc)
        {
            //Calculate the total amount owed
            double TotalDebt = 0;
            
            foreach (MoneyOwedByUserGroupDto Share in ListOfUserGroupShares)
            {
                if (Share.FinalAmountOwed > 0)
                {
                    TotalDebt += Share.FinalAmountOwed;
                }
            }
            
            string TotalDebtString = string.Format("Total debt of the group = {0} DKK", TotalDebt);
            
            InsertCustomParagraphIntoDocument(doc, TotalDebtString, Font.FontFamily.TIMES_ROMAN, BaseColor.BLACK, 12, Element.ALIGN_LEFT);
        }

        public void AddAllIndividualAccounting(Document doc)
        {
            string IndividualAccountingSubheader = "2. Individual Accounting for all members of the group:";
            
            InsertCustomParagraphIntoDocument(doc, IndividualAccountingSubheader, Font.FontFamily.COURIER, BaseColor.BLACK, 15, Element.ALIGN_LEFT);
            AddEmptyLines(doc, 1);

            //Create paragraphs for each user and add them to the document
            foreach (MoneyOwedByUserGroupDto UsersAccounting in ListOfUserGroupShares)
            {
                //Insert name of user
                string NameOfUser = string.Format("Username: {0}", UsersAccounting.UserName);
                InsertCustomParagraphIntoDocument(doc, NameOfUser, Font.FontFamily.TIMES_ROMAN, BaseColor.BLACK, 12, Element.ALIGN_LEFT);
                //Insert His total Expenses
                string UsersExpense = string.Format("Total amount paid during group: {0}", UsersAccounting.AmountPaidDuringGroup);
                InsertCustomParagraphIntoDocument(doc, UsersExpense, Font.FontFamily.TIMES_ROMAN, BaseColor.BLACK, 12, Element.ALIGN_LEFT);
                //insert his total In payments
                string UsersInPayments = string.Format("Total amount paid into group pool: {0}",UsersAccounting.AmountAlreadyPaid);
                InsertCustomParagraphIntoDocument(doc, UsersInPayments, Font.FontFamily.TIMES_ROMAN, BaseColor.BLACK, 12, Element.ALIGN_LEFT);
                
                
                //Insert his total money owed
                double AmountOwed = UsersAccounting.FinalAmountOwed;
                //If AmountOwed is positive, he needs to pay out
                //If AmountOwed is negative, he needs to recieve
                string AmountOwedString = "";
                BaseColor Color = BaseColor.BLACK;
                if (AmountOwed > 0)
                {
                    AmountOwedString = string.Format("Amount to pay to group: {0}", UsersAccounting.FinalAmountOwed);
                    Color = BaseColor.RED;
                }else if (AmountOwed == 0)
                {
                    AmountOwedString = "Not required to pay or receive any money";
                    Color = BaseColor.BLACK;
                }else if (AmountOwed < 0)
                {
                    AmountOwedString = string.Format("Amount to receive from the group: {0}",  -UsersAccounting.FinalAmountOwed);
                    Color = BaseColor.GREEN;
                }

                InsertCustomParagraphIntoDocument(doc, AmountOwedString, Font.FontFamily.TIMES_ROMAN, Color, 12, Element.ALIGN_LEFT);
                
                //Insert empty space
                AddEmptyLines(doc, 2);
                
            }
        }

        public void AddDueDateIfGroupConcluded(Document doc)
        {
            //Get the group id and then check if it's concluded
            //If it is concluded, set the payment deadline to 30 days into the future
            MoneyOwedByUserGroupDto SingleEntry = ListOfUserGroupShares.First();
            int GroupID = SingleEntry.GroupID;
            ShareGroup TheGroup = context.ShareGroups.Find(GroupID);
            bool HasConcluded = (bool)TheGroup.HasConcluded;

            string BottomText;

            if (HasConcluded)
            {
                //Get the conclusion date
                //Set the due date 30 days into the future
                DateTime ConclusionDate = (DateTime)TheGroup.ConclusionDate;
                DateTime DueDate = ConclusionDate.AddDays(30);
                BottomText = string.Format("Individual shares have to be paid by {0} or incur the wrath of a siberian prison", DueDate.ToString());
            }
            else
            {
                //Write that the group is ongoing and shares are not final
                BottomText = "Group is on-going so the shares are are subject to change";
            }
            InsertCustomParagraphIntoDocument(doc, BottomText, Font.FontFamily.TIMES_ROMAN, BaseColor.BLACK, 12, Element.ALIGN_LEFT);
        }

    }
}
