using System.Text.Json;

namespace T3App.Shared.Extensions
{
    public static class StringExtensions
    {
        public static T ConvertJsonTo<T>(this string jsonString)
        {
            return  JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
