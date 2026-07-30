using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AirConServicingManagementSystem.Extensions
{
    public static class SessionExtension
    {

        public static void SetObject<T>
        (
            this ISession session,
            string key,
            T value
        )
        {
            session.SetString(
                key,
                JsonConvert.SerializeObject(value)
            );
        }



        public static T? GetObject<T>
        (
            this ISession session,
            string key
        )
        {

            var value=session.GetString(key);


            if(value==null)
                return default;



            return JsonConvert.DeserializeObject<T>(value);

        }

    }
}