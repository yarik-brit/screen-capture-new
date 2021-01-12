using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AncestryWeb.Models;
using AncestryWeb.Helpers;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Web;


namespace AncestryWeb.Controllers
{
    public class EmailController : Controller
    {
        static GMailer Gmail = new GMailer();

        static bool RunEmailService = true;
        async public static void EmailService()
        {
            Debug.WriteLine("Data Service Launched");

            while (RunEmailService)
            {
                var query = $"select top 1 * from EmailQueue order by Id";
                var result = DB.ExecuteQuery(query);
                if (result.Rows.Count > 0)
                {
                    try
                    {
                        DB.ExecuteNonQuery($"DELETE FROM EmailQueue where Id = {result.Rows[0]["Id"]}");

                        Gmail.Send(
                            result.Rows[0]["Email"].ToString(),
                            result.Rows[0]["Subject"].ToString(),
                            result.Rows[0]["Message"].ToString()
                        );

                    }
                    catch (Exception ex)
                    {
                        EventController.PushEvent("Error", false, ex.Message);
                    }

                    await Task.Delay(3000);
                }
                else
                {
                    await Task.Delay(5000);
                }
            }
        }

        public static void PushEmail(string Email, string Subject, string Message) {
            DB.ExecuteNonQuery(string.Format(Config.GetDBStatement("PushEmail"), Email, Subject, Message));
        }
    }
}
