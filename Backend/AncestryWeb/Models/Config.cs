using System;
using System.Collections.Generic;

namespace AncestryWeb.Models
{
    public static class Config
    {
        public static string BaseUrl = "http://tensionx-002-site8.btempurl.com/";
        //public static string BaseUrl = "https://localhost:44336/";
        //public static string connectionString = "Data Source=den1.mssql8.gear.host;Initial Catalog=extensiontask;User ID=extensiontask;Password='Rb7s?7?SQHZU'";
        public static string connectionString = "Data Source=den1.mssql8.gear.host;Initial Catalog=testscreenshot;User ID=testscreenshot;Password='Mn4L~-a7k5UP'";

        public static string CaptchaSecretKey = "6LctJAEVAAAAAGas12512FXnZFv9G6Tbq4cZN71";
        //public string connectionString = "Initial Catalog=KeyManager;Data Source=localhost\\SQLEXPRESS;Integrated Security=SSPI;";

        public static string GetMessage(string id)
        {
            switch (id)
            {
                case "ConfirmToSignin":
                    return "Confirm your email to sign in";

                case "IncorrectCredentials":
                    return "Incorrect login or password";

                case "RegisterSuccessful":
                    return "Sign up successful. Check your email inbox for activation email.";

                case "ActivationSuccessful":
                    return "Account activated. Use extension popup to sign in";

                case "ActivationFailed":
                    return "Incorrect activation link. Please try again";
                case "ActivationExpired":
                    return "Activation link is expired. Please try again";

                case "RestorationFailed":
                    return "Incorrect password recovery link. Please try again";
                case "RestorationExpired":
                    return "Password recovery link is expired. Please try again";


                case "IncorrectPassword":
                    return "Password should be 8 - 30 characters long";

                case "PasswordChangeSuccessful":
                    return "Password has been successfully changed";

                case "RestoreEmailSent":
                    return "Password recovery email sent to your inbox";
                case "RestoreEmailNotFould":
                    return "There is no account associated with this email";
                case "IncorrectEmail":
                    return "Incorrect email address";
                case "ErrorGlobal":
                    return "Error occurred please contact techical support team";
                case "Incorrect Captcha":
                    return "Incorrect captcha";
                case "VerifyEmail":
                    return $@"Thank you for joining <b>Ancestry Helper Community</b><br>
Please activate your account by visiting a <a href='{BaseUrl}Home/Activate?hash={{0}}&userId={{1}}'>confirmation link</a>.".Replace("'", "''");

                case "RecoverPassword":
                    return $@"We've received a request for restoring your <b>Ancestry Helper</b> account access<br>
Visit the <a href='{BaseUrl}Home/Restore?hash={{0}}&userId={{1}}'>recovery link</a> to restore your access.".Replace("'", "''");
                default:
                    return "Incorrect String ID";


            }
        }

        public static string GetDBStatement(string id)
        {
            switch (id)
            {

                case "PushSignupVefirication":
                    return @"INSERT INTO UserActivate (UserId, Code) VALUES({0}, '{1}') SELECT * FROM [User] where Id = {0}";
                case "PushRestore":
                    return @"INSERT INTO UserRestore (UserId, Code) VALUES({0}, '{1}')";
                case "PushEmail":
                    return "INSERT INTO EmailQueue (Email, Subject, Message) values('{0}', '{1}', '{2}')";
                default:
                    return "Incorrect String ID";
            }
        }


    }
}
