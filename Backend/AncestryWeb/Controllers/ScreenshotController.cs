using AncestryWeb.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;

namespace AncestryWeb.Controllers
{
    public class ScreenshotController : Controller
    {
        static Account account = new Account(
            "scr-capt-rec",
            "377884457262522",
            "lwZOnLWp1KaEagB7unQVblPy40A");
        static Cloudinary cloudinary = new Cloudinary(account);

        static string linkBase = Config.BaseUrl + "Screenshot?id=";
        static string linkSource = "https://res.cloudinary.com/scr-capt-rec/";


        // GET: Screenshot
        public ActionResult Index(string id)
        {
            //real image - https://localhost:44336/Screenshot?id=186E1447-451B-4FFE-8329-57B718160A52
            //empty link - https://localhost:44336/Screenshot?id=186E1447-451B-4FFE-8329-57B7181AAAAA
            if (id != null)
            {
                string GUID = id;
                Debug.WriteLine(GUID);
                ImageModel image = GetImageByGUID(GUID);
                image.Link = GetFullReference(image);

                if (image.Name == null || image.GUID == null || image.Link == null)
                {
                    if(id == "1") { return View("~/Views/Screenshot/NoImageFound.cshtml"); }
                    if (id == "0") { return View("~/Views/Error.cshtml"); }
                    else { return View("~/Views/Error.cshtml"); }
                }
                else
                {
                    return View(image);
                }
            }
            return View("~/Views/Error.cshtml");
        }





        static public List<ImageModel> GetImagesByUserID(int UserID)
        {
            var query = $@"SELECT * FROM [dbo].[Image] WHERE UserID = {UserID}";
            var result = DB.ExecuteQuery(query);
            var returnImages = new List<ImageModel>();
            foreach (DataRow row in result.Rows)
            {
                ImageModel current = new ImageModel();
                current.Name = (string)row["Name"];
                current.GUID = row["GUID"].ToString();
                current.Link = (string)row["Link"];
                current.UserID = (int)row["UserID"];
                current.CreatedOn = (string)row["CreatedOn"];
                returnImages.Add(current);
            }
            return returnImages;
        }

        static public ImageModel GetImageByGUID(string GUID)
        {
            var query = $@"SELECT * FROM [dbo].[Image] WHERE GUID = '{GUID}'";
            var result = DB.ExecuteQuery(query);
            ImageModel returnImage = new ImageModel();
            foreach (DataRow row in result.Rows)
            {
                returnImage.Name = (string)row["Name"];
                returnImage.GUID = row["GUID"].ToString();
                returnImage.Link = (string)row["Link"];
                returnImage.UserID = (int)row["UserID"];
                returnImage.CreatedOn = (string)row["CreatedOn"];
            }
            return returnImage;
        }

        static public ImageModel GetLatestImageByUserID(int UserID)
        {
            var query = $@"SELECT TOP 1 * FROM[dbo].[Image]
                        WHERE UserID = {UserID} ORDER BY Name DESC";
            var result = DB.ExecuteQuery(query);
            ImageModel returnImage = new ImageModel();
            foreach (DataRow row in result.Rows)
            {
                returnImage.Name = (string)row["Name"];
                returnImage.GUID = row["GUID"].ToString();
                returnImage.Link = (string)row["Link"];
                returnImage.UserID = (int)row["UserID"];
                returnImage.CreatedOn = (string)row["CreatedOn"];
            }
            return returnImage;
        }

        static public string GetFullReference(ImageModel image)
        {
            string toReturn = linkSource;
            toReturn += image.Link;
            return toReturn;
        }

        static public ImageModel GetImageByName(string name)
        {
            var query = $@"SELECT * FROM [dbo].[Image] WHERE Name = '{name}'";
            var result = DB.ExecuteQuery(query);
            ImageModel returnImage = new ImageModel();
            foreach (DataRow row in result.Rows)
            {
                returnImage.Name = (string)row["Name"];
                returnImage.GUID = row["GUID"].ToString();
                returnImage.Link = (string)row["Link"];
                returnImage.UserID = (int)row["UserID"];
                returnImage.CreatedOn = (string)row["CreatedOn"];
            }
            return returnImage;
        }


        static public void RenameImage(ImageModel image, string newName)
        {
            var query = $@"UPDATE [dbo].[Image] SET Name = '{newName}.jpeg' WHERE GUID = {image.GUID}";
            var result = DB.ExecuteQuery(query);
        }

        [HttpPost]
        static public string DeleteImageByName()
        {
            HttpContext context = System.Web.HttpContext.Current;
            var imgName = context.Request.Params["name"];

            ImageModel imageToDelete = GetImageByName(imgName);

            var query = $@"DELETE FROM [dbo].[Image] WHERE Name = '{imgName}'";
            var result = DB.ExecuteQuery(query);

            if (!result.HasErrors)
            {
                var delResParams = new DelResParams()
                {
                    PublicIds = new List<string> { imageToDelete.Link }
                };
                if (cloudinary.DeleteResources(delResParams) != null)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                return "0";
            }
        }


        [HttpPost]
        public string GetImageLinkByName()
        {
            try
            {
                HttpContext context = System.Web.HttpContext.Current;
                var imgName = context.Request.Params["name"];

                ImageModel current = GetImageByName(imgName);

                return GetFullReference(current);
            }

            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return "<null_link_on_error>";
            }
        }

        [HttpPost]
        public string GetImageHashByName()
        {
            try
            {
                HttpContext context = System.Web.HttpContext.Current;
                var imgName = context.Request.Params["name"];

                ImageModel current = GetImageByName(imgName);

                return current.GUID;
            }

            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return "<null_link_on_error>";
            }
        }
    }
}