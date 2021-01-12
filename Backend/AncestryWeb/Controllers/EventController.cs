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
    public class EventController : Controller
    {
        public static void PushEvent(string EventType, bool result, string Message)
        {
            string query = GetPushQuery();
            DB.ExecuteNonQuery(query);

            string GetPushQuery()
            {
                return $@"INSERT INTO EventLog([EventTypeId], [Date], [Result], [Message])
                         select top 1 isnull(Id, 0), getDate(), {(result ? 1 : 0)}, '{Message}'
                         from EventType where EventName = '{EventType}'";
            }
        }
    }
}
