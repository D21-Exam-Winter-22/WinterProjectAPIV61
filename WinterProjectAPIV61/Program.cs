using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WinterProjectAPIV61.Models;
using System.Threading;
using Microsoft.AspNetCore.Hosting.Server;
using WinterProjectAPIV61.Controllers;
using WinterProjectAPIV61.Controllers;
using WinterProjectAPIV61.PDFGenerator;
using WinterProjectAPIV61.Models;


public class Program
{
    static ShareGroupController controller = new ShareGroupController(new PaymentApidb3Context());
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        //The scheduled service to clean up the database
        System.Timers.Timer timer = new(1000 * 3600 * 24);
        timer.Elapsed += (sender, e) => HandleTimer();
        timer.Start();

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddDbContext<PaymentApidb3Context>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(policyBuilder =>
            policyBuilder.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                }
            )
        );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }

    public static void HandleTimer()
    {
        controller.ArchiveDormantGroups();
    }


}