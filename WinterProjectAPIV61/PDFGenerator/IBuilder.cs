using iTextSharp.text;
using iTextSharp.text.pdf;

namespace WinterProjectAPIV61.PDFGenerator
{
    public interface IBuilder
    {
       public void AddTitle(Document doc);

       public void AddGroupNameDescription(Document doc);

       public void AddGroupsTotalExpenses(Document doc);

       public void AddGroupsTotalInPayments(Document doc);

       public void AddGroupsTotalPendingDebt(Document doc);

       public void AddAllIndividualAccounting(Document doc);

       public void AddDueDateIfGroupConcluded(Document doc);

    }
}
