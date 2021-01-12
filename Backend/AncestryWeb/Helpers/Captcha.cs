using AncestryWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AncestryWeb.Helpers
{
    public static class Captcha
    {
        /// <summary>
        /// Converts a DataTable to a list with generic objects
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>List with generic objects</returns>
        async public static Task<bool> IsValid(string response) {

            var values = new Dictionary<string, string>
            {
                { "secret", Config.CaptchaSecretKey },
                { "response", response }
            };

            var content = new FormUrlEncodedContent(values);
            HttpClient client = new HttpClient();
            var result = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            var responseString = await result.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
            return responseObject.success;
        }
    }
}
