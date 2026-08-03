namespace AirConServicingManagementSystem.Services
{
    public class SmsService
    {


        public async Task SendSms(
            string phone,
            string message)
        {


            Console.WriteLine(
                $"SMS TO {phone}: {message}"
            );


            await Task.CompletedTask;

        }


    }
}