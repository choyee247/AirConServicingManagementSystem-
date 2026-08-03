using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.Services
{
    public class NotificationService
    {
        private readonly DBContext _context;

        public NotificationService(DBContext context)
        {
            _context = context;
        }

        public async Task CreateNotification(
            int userId,
            string title,
            string message,
            string type,
            int? reminderId)
        {

            Notification noti =
                new Notification
                {

                    UserId = userId,

                    Title = title,

                    Message = message,

                    Type = type,

                    IsRead = false,

                    CreatedAt = DateTime.Now,

                    ServiceReminderId = reminderId

                };

            _context.Notifications.Add(noti);

            await _context.SaveChangesAsync();

        }

    }
}
