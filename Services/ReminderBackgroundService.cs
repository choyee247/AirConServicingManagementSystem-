using AirConServicingManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AirConServicingManagementSystem.Services
{
    public class ReminderBackgroundService : BackgroundService
    {

        private readonly IServiceScopeFactory _scopeFactory;


        public ReminderBackgroundService(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }




        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {


            while (!stoppingToken.IsCancellationRequested)
            {

                using (var scope = _scopeFactory.CreateScope())
                {

                    var context =
                        scope.ServiceProvider
                        .GetRequiredService<DBContext>();


                    var smsService =
                        scope.ServiceProvider
                        .GetRequiredService<SmsService>();





                    // =====================================
                    // Get Pending Reminder
                    // =====================================

                    var reminders =
                        await context.ServiceReminders
                        .Where(x =>
                            (x.IsDeleted == false ||
                             x.IsDeleted == null)

                            &&

                            (x.SentStatus == false ||
                             x.SentStatus == null)

                            &&

                            x.ReminderDate != null

                            &&

                            x.ReminderDate <= DateTime.Now.AddDays(7)

                        )
                        .ToListAsync();




                    Console.WriteLine(
                        $"Pending Reminder : {reminders.Count}"
                    );






                    foreach (var reminder in reminders)
                    {



                        // =====================================
                        // ADMIN NOTIFICATION ONLY
                        // =====================================


                        var admins =
                            await context.Users
                            .Where(x =>
                                x.Role == "Admin")
                            .ToListAsync();



                        foreach (var admin in admins)
                        {


                            var adminNotification =
                            new Notification
                            {

                                UserId =
                                admin.Id,


                                Title =
                                "Service Reminder",


                                Message =
                                $"Customer ID {reminder.CustomerId} service reminder is due.",


                                Type =
                                "Reminder",


                                IsRead =
                                false,


                                CreatedAt =
                                DateTime.Now,


                                ServiceReminderId =
                                reminder.Id,


                                ServiceRequestId =
                                reminder.ServiceRequestId

                            };



                            context.Notifications
                            .Add(adminNotification);


                        }








                        // =====================================
                        // TECHNICIAN NOTIFICATION + SMS
                        // =====================================


                        if (reminder.ServiceRequestId != null)
                        {


                            var serviceRequest =
                                await context.ServiceRequests
                                .FirstOrDefaultAsync(x =>
                                    x.ServiceId ==
                                    reminder.ServiceRequestId);



                            if (serviceRequest != null &&
                               serviceRequest.TechnicianId != null)
                            {



                                var technicianUser =
                                    await context.Users
                                    .FirstOrDefaultAsync(x =>
                                        x.TechnicianId ==
                                        serviceRequest.TechnicianId);





                                if (technicianUser != null)
                                {



                                    // -----------------------------
                                    // Technician Bell Notification
                                    // -----------------------------


                                    var technicianNotification =
                                    new Notification
                                    {

                                        UserId =
                                        technicianUser.Id,


                                        Title =
                                        "Upcoming Service Reminder",


                                        Message =
                                        "Your assigned customer service reminder is due.",


                                        Type =
                                        "Reminder",


                                        IsRead =
                                        false,


                                        CreatedAt =
                                        DateTime.Now,


                                        ServiceReminderId =
                                        reminder.Id,


                                        ServiceRequestId =
                                        reminder.ServiceRequestId

                                    };



                                    context.Notifications
                                    .Add(technicianNotification);








                                    // -----------------------------
                                    // Technician SMS
                                    // -----------------------------


                                    if (technicianUser.TechnicianId != null)
                                    {


                                        var technician =
                                            await context.Technicians
                                            .FirstOrDefaultAsync(x =>
                                                x.TechnicianId ==
                                                technicianUser.TechnicianId);



                                        if (technician != null &&
                                           !string.IsNullOrEmpty(
                                           technician.PhoneNumber))
                                        {



                                            await smsService.SendSms(

                                                technician.PhoneNumber,

                                                "AirCon Service Reminder: You have an upcoming customer service."

                                            );


                                        }


                                    }





                                }



                            }


                        }







                        // =====================================
                        // Update Sent Status
                        // =====================================

                        reminder.SentStatus = true;


                    }





                    await context.SaveChangesAsync();



                }






                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);

            }


        }


    }
}