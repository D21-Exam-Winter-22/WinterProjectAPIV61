using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterProjectAPIV61.Models;

namespace WinterProjectAPIV61.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityQuestionController : ControllerBase
    {
        private readonly PaymentApidb3Context context;

        public SecurityQuestionController(PaymentApidb3Context context)
        {
            this.context = context;
        }

        [HttpPost("InserNewSecurityQuestion")]
        public async Task<ActionResult<string>> InserNewSecurityQuestion(string question)
        {
            SecurityQuestion QuestionToInsert = new SecurityQuestion
            {
                Question = question
                
            };
            await context.AddAsync(QuestionToInsert);
            await context.SaveChangesAsync();
            return Ok("Inserted");
        }

        [HttpGet("GetAllSecurityQuestions")]
        public async Task<ActionResult<List<SecurityQuestion>>> GetAllSecurityQuestions()
        {
            List<SecurityQuestion> ListOfSecurityQuestions = await context.SecurityQuestions.ToListAsync();
            return Ok(ListOfSecurityQuestions);
        }

        [HttpPut("EditSecurityQuestionOnID")]
        public async Task<ActionResult<string>> EditSecurityQuestion(SecurityQuestion request)
        {
            //Get the SecurityQuestion to update
            SecurityQuestion TheQuestion = await context.SecurityQuestions.FindAsync(request.QuestionId);

            if (TheQuestion == null)
            {
                return Ok("Question not found");
            }
            
            //Edit the body
            TheQuestion.Question = request.Question;
            await context.SaveChangesAsync();
            return Ok("Question successfully Edited");
        }

        [HttpDelete("DeleteQuestionOnID/{QuestionID}")]
        public async Task<ActionResult<string>> DeleteQuestionOnID(int QuestionID)
        {
            SecurityQuestion TheQuestion = await context.SecurityQuestions.FindAsync(QuestionID);

            if (TheQuestion == null)
            {
                return Ok("Question not found");
            }
            
            //Delete it
            context.Remove(TheQuestion);
            await context.SaveChangesAsync();
            return Ok("Question Deleted");
        }
        
        
    }
}
