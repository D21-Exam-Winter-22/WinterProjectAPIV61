using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Timers;
using WinterProjectAPIV61.Models;
using Timer = System.Timers.Timer;

namespace WinterProjectAPIV61.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermsOfServiceController : ControllerBase
    {
        private readonly PaymentApidb3Context context;

        public TermsOfServiceController(PaymentApidb3Context context)
        {
            this.context = context;

            Timer t = new Timer(2);
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            //t.Start();

        }

        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            InsertNewTermsOfService("Timer test");
        }

        [HttpGet("GetAllTermsOfServices")]
        public async Task<ActionResult<List<TermsOfService>>> GetAllTermsOfServices()
        {
            return Ok(await context.TermsOfServices.ToListAsync());
        }

        [HttpPost("InsertNewTermsOfService")]
        public async Task<ActionResult<string>> InsertNewTermsOfService(string NewToSContent)
        {
            TermsOfService ToSToInsert = new TermsOfService
            {
                CreationDate = DateTime.Now,
                LastModificationDate = DateTime.Now,
                Content = NewToSContent
            };

            await context.AddAsync(ToSToInsert);
            await context.SaveChangesAsync();

            return Ok("Inserted ToS");
        }

        [HttpPut("EditATermsOfService")]
        public async Task<ActionResult<string>> EditTermsOfService(TermsOfService request)
        {
            //Get the terms of service to edit
            TermsOfService TheToS = await context.TermsOfServices.FindAsync(request.ToSid);
            
            //If the ToS is not found
            if (TheToS == null)
            {
                return Ok(string.Format("ToS not found on ID: {0}", request.ToSid));
            }
            
            //Edit the ToS
            {
                TheToS.LastModificationDate = DateTime.Now;
                TheToS.Content = request.Content;
            }
            
            //Save the changes
            await context.SaveChangesAsync();
            return Ok("ToS successfully edited");

        }
        

        [HttpDelete("DeleteToSOnID/{TermsOfServiceID}")]
        public async Task<ActionResult<string>> DeleteToSOnID(int TermsOfServiceID)
        {
            //Get the ToS
            TermsOfService TheToS = await context.TermsOfServices.FindAsync(TermsOfServiceID);

            if (TheToS == null)
            {
                return Ok(string.Format("ToS not found on ID: {0}", TermsOfServiceID));
            }

            context.Remove(TheToS);
            await context.SaveChangesAsync();
            
            return Ok("Terms of service deleted");
        }

        [HttpGet("GetLatestToS")]
        public async Task<ActionResult<TermsOfService>> GetLatestToS()
        {
            TermsOfService CurrentTermsOfService =
                await context.TermsOfServices.OrderByDescending(service => service.CreationDate).FirstAsync();

            return Ok(CurrentTermsOfService);
            
        }
    }
}
