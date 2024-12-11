using System.Text.Json.Serialization;
using System.Text.Json;
using PhoneShopSharedLibrary.Models;

namespace PhoneShopeClient.Services
{ 
    public static class General
    {
    public static string SerializeObj(object modelObject) => JsonSerializer.Serialize(modelObject, JsonOptions());
    public static T DeserializeJsonString<T>(string jsonString) => JsonSerializer.Deserialize<T>(jsonString, JsonOptions())!;
    public static StringContent GenerateStringContent(string serialiazedObj) => new(serialiazedObj, System.Text.Encoding.UTF8, "application/json");
    public static IEnumerable<T> DeszrializeJsonStringList<T>(string jsonString) => JsonSerializer.Deserialize<IEnumerable<T>>(jsonString, JsonOptions())!;//.ToList()
    
        public static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
        };
    }

        public static string  GetDescription(string description)
        {
            string appendDots = "...";
            int maxLenth = 100;
            int descriptionLength = description.Length;
            return descriptionLength > maxLenth ? $"{description.Substring(0, 100)}{appendDots}" : description;
        }
    }
}
