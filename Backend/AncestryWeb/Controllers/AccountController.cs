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
using System.Text.RegularExpressions;

namespace AncestryWeb.Controllers
{
    public class AccountController : Controller
    {
        //Initing Gmail Handler
        public ActionResult LoginUser(User userData)
        {
            try
            {
                var query = $"select *  from [User] where LOWER(Login) = '{userData.Login.ToLower()}' and Password = '{userData.Password}'";
                
                var result = DB.ExecuteQuery(query).DataTableToList<User>();

                var success = result.Count > 0 && result[0].Activated == 1;

                return new JsonResult { Data = new { success, userData = success ? new User() { Login = result[0].Login } : null, message = result.Count > 0 ? Config.GetMessage("ConfirmToSignin") : Config.GetMessage("IncorrectCredentials") } };
            } 
            catch (Exception ex)
            {
                return new JsonResult { Data = new { success = false, message = ex.Message } };
            }
        }
        
        public ActionResult RegisterUser(User userData)
        {
            //Validate Credentials
            var error = UserIncorrect(userData);

            if (error != "")
            {
                return new JsonResult { Data = new { success = false, message = error } };
            }

            //Make sure we don't create duplicate
            error = UserExists(userData);
            if (error != "")
            {
                return new JsonResult { Data = new { success = false, message = error } };
            }

            //Insert query
            var query = $@"INSERT INTO [User] (Login, Password, Email) OUTPUT INSERTED.Id values('{userData.Login}', '{userData.Password}', '{userData.Email}')
                           ";

            var userId = DB.ExecuteScalar(query);
            SendSignupValidation(userId);
            //EmailController.SendEmailValidation(userId);
            return new JsonResult { Data = new { success = true, message = Config.GetMessage("RegisterSuccessful"), userData = new User() {Login = userData.Login } } , JsonRequestBehavior= JsonRequestBehavior.AllowGet};
        }

        public static void SendSignupValidation(int userId)
        {
            var code = Guid.NewGuid().ToString();
            var query = string.Format(Config.GetDBStatement("PushSignupVefirication"), userId, code);
            var user = DB.ExecuteQuery(query).Rows[0];
            var message = string.Format(Config.GetMessage("VerifyEmail"), code, userId);
            EmailController.PushEmail(user["Email"].ToString(), "Confirm Your Email", message);
        }

        public  ActionResult RecoverPassword()
        {
            try
            {
                var email = Request["email"];

                if (!RegexHelper.IsValidEmail(email))
                {
                    return new JsonResult { Data = new { success = false, message = Config.GetMessage("IncorrectEmail") } };
                }
                var query = $"select Id, Email from [User] where LOWER(Email) = LOWER('{email}')";
                var result = DB.ExecuteQuery(query).DataTableToList<User>(); 
                if (result.Count > 0)
                {
                    SendRecoveryEmail(result[0]);
                    return new JsonResult { Data = new { success = true, message = Config.GetMessage("RestoreEmailSent") } };
                }
                return new JsonResult { Data = new { success = false, message = Config.GetMessage("RestoreEmailNotFould") } };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { success = false, message = Config.GetMessage("ErrorGlobal") } };
            }
        }

        public string UserIncorrect(User userData)
        {
            if (!RegexHelper.IsValidEmail(userData.Email))
            {
                return "Incorrect email address";
            }
            else
            {
                //Using if-else because of switch limitation
                if (userData.Login.Length < 5 || userData.Login.Length > 30)
                {
                    return "Login should be 5-30 characters long";
                }
                else if (userData.Password.Length < 8 || userData.Password.Length > 30)
                {
                    return "Password should be 8-30 characters long";
                }
            }

            return "";
        }

        public string UserExists(User userData)
        {
            var existsQuery = $"Select top 1 * from [User] where Login = '{userData.Login}' and Activated = 1";
            var data = DB.ExecuteQuery(existsQuery);
            if (data.Rows.Count > 0)
            {
                return "User with this login already exists";
            }
            data = DB.ExecuteQuery($"Select top 1 * from [User] where Email = '{userData.Email}' and Activated = 1");
            if (data.Rows.Count > 0)
            {
                return "User with this email already exists";
            }

            return "";
        }

        public ActionResult VerifyUserActivation()
        {
            try
            {
                var code = Request["code"];
                var userId = Request["userId"];
                var query = $"Delete from UserActivate OUTPUT Deleted.UserId, case when Deleted.Timeout > DATEDIFF(minute, Deleted.CreateTime, getDate()) then 0 else 1 end [Expired] where UserId = {userId} and Code = '{code}'";
                var activatedUser = DB.ExecuteQuery(query).Rows;
                if (activatedUser.Count > 0)
                {
                    if(Convert.ToInt32(activatedUser[0]["Expired"]) == 1)
                    {
                        return new JsonResult { Data = new { success = false, message = Config.GetMessage("ActivationExpired") } };
                    }
                    DB.ExecuteNonQuery($"declare @email table(email nvarchar(256)); UPDATE [User]  set Activated = 1 output INSERTED.Email into @email where Id = {activatedUser[0]["UserId"]} DELETE FROM [USER] where Email = (select top 1 Email from @email) and Id <> {activatedUser[0]["UserId"]}");
                    return new JsonResult { Data = new { success = true, message = Config.GetMessage("ActivationSuccessful") } };
                }
                return new JsonResult { Data = new { success = false, message = Config.GetMessage("ActivationFailed") } };
            }
            catch(Exception ex)
            {
                return new JsonResult { Data = new { success = false, message = Config.GetMessage("ActivationFailed") } };
            }
        }

        public ActionResult VerifyAccountRestore()
        {
            try
            {
                var code = Request["code"];
                var userId = Request["userId"];
                var query = $"Select top 1 case when ur.Timeout > DATEDIFF(minute, ur.CreateTime, getDate()) then 0 else 1 end [Expired] from UserRestore ur where UserId = {userId} and Code = '{code}'";

                var restoreEntry = DB.ExecuteQuery(query).Rows;
                if (restoreEntry.Count > 0)
                {
                    if (Convert.ToInt32(restoreEntry[0]["Expired"]) == 1)
                    {
                        return new JsonResult { Data = new { success = false, message = Config.GetMessage("RestorationExpired") } };
                    }
                    //DB.ExecuteNonQuery($"declare @email table(email nvarchar(256)); UPDATE [User]  set Activated = 1 output INSERTED.Email into @email where Id = {activatedUser.Rows[0]["UserId"]} DELETE FROM [USER] where Email = (select top 1 Email from @email) and Id <> {activatedUser.Rows[0]["UserId"]}");
                    return new JsonResult { Data = new { success = true } };
                }
                return new JsonResult { Data = new { success = false, message = Config.GetMessage("RestorationFailed") } };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { success = false, message = Config.GetMessage("RestorationFailed") } };
            }
        }

        public ActionResult ChangePassword()
        {
            var code = Request["code"];
            var userId = Request["userId"];
            var password = Request["password"];

            try
            {
                if (password.Length < 8 || password.Length > 30)
                {
                    return new JsonResult { Data = new { success = false, message = Config.GetMessage("IncorrectPassword") } };
                }


                var query = $"Delete from UserRestore OUTPUT Deleted.UserId, case when Deleted.Timeout > DATEDIFF(minute, Deleted.CreateTime, getDate()) then 0 else 1 end [Expired] where UserId = {userId} and Code = '{code}'";
                var targetUser = DB.ExecuteQuery(query).Rows;
                if (targetUser.Count > 0)
                {
                    if (Convert.ToInt32(targetUser[0]["Expired"]) == 1)
                    {
                        return new JsonResult { Data = new { success = false, message = Config.GetMessage("RestorationExpired") } };
                    }
                    DB.ExecuteNonQuery($"UPDATE [User]  set Password = '{password}' where Id = {targetUser[0]["UserId"]}");
                    return new JsonResult { Data = new { success = true, message = Config.GetMessage("PasswordChangeSuccessful") } };
                }
                return new JsonResult { Data = new { success = false, message = Config.GetMessage("RestorationFailed") } };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { success = false, message = Config.GetMessage("RestorationFailed") } };
            }
        }

        public ActionResult ValidateSession(User userData)
        {
            //placeholder for session validation
            return new JsonResult { Data = new { expired = userData.Login.ToLower() == "vladimir" ? true : false } };
        }

        public void SendRecoveryEmail(User user)
        {
            var code = Guid.NewGuid().ToString();
            var query = string.Format(Config.GetDBStatement("PushRestore"), user.Id, code);
            DB.ExecuteNonQuery(query);
            var message = string.Format(Config.GetMessage("RecoverPassword"), code, user.Id);
            EmailController.PushEmail(user.Email, "Recover Ancestry Helper Password", message);
        }
    }
}
