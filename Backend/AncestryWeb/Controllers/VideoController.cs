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
    public class VideoController : Controller
    {
        static Account account = new Account(
            "scr-capt-rec",
            "377884457262522",
            "lwZOnLWp1KaEagB7unQVblPy40A");
        static Cloudinary cloudinary = new Cloudinary(account);

        static string linkBase = Config.BaseUrl + "Video?id=";
        static string linkSource = "https://res.cloudinary.com/scr-capt-rec/video/upload/v1608642998/";


        // GET: Screenshot
        public ActionResult Index(string id)
        {
            //real video - https://localhost:44336/Video?id=186E1447-451B-4FFE-8329-57B718160A52
            //empty link - https://localhost:44336/Video?id=186E1447-451B-4FFE-8329-57B7181AAAAA
            if (id != null)
            {
                string GUID = id;
                Debug.WriteLine(GUID);
                VideoModel video = GetVideoByGUID(GUID);
                video.Link = GetFullReference(video);

                if (video.Name == null || video.GUID == null || video.Link == null)
                {
                    if(id == "1") { return View("~/Views/Video/NoVideoFound.cshtml"); }
                    if (id == "0") { return View("~/Views/Error.cshtml"); }
                    else { return View("~/Views/Error.cshtml"); }
                }
                else
                {
                    return View(video);
                }
            }
            return View("~/Views/Error.cshtml");
        }


        static public List<VideoModel> GetVideosByUserID(int UserID)
        {
            var query = $@"SELECT * FROM [dbo].[Video] WHERE UserID = {UserID}";
            var result = DB.ExecuteQuery(query);
            var returnVideos = new List<VideoModel>();
            foreach (DataRow row in result.Rows)
            {
                VideoModel current = new VideoModel();
                current.Name = (string)row["Name"];
                current.GUID = row["GUID"].ToString();
                current.Link = (string)row["Link"];
                current.UserID = (int)row["UserID"];
                current.CreatedOn = (string)row["CreatedOn"];
                returnVideos.Add(current);
            }
            return returnVideos;
        }

        static public VideoModel GetVideoByGUID(string GUID)
        {
            var query = $@"SELECT * FROM [dbo].[Video] WHERE GUID = '{GUID}'";
            var result = DB.ExecuteQuery(query);
            VideoModel returnVideo = new VideoModel();
            foreach (DataRow row in result.Rows)
            {
                returnVideo.Name = (string)row["Name"];
                returnVideo.GUID = row["GUID"].ToString();
                returnVideo.Link = (string)row["Link"];
                returnVideo.UserID = (int)row["UserID"];
                returnVideo.CreatedOn = (string)row["CreatedOn"];
            }
            return returnVideo;
        }

        static public VideoModel GetLatestVideoByUserID(int UserID)
        {
            var query = $@"SELECT TOP 1 * FROM[dbo].[Video]
                        WHERE UserID = {UserID} ORDER BY Name DESC";
            var result = DB.ExecuteQuery(query);
            VideoModel returnVideo = new VideoModel();
            foreach (DataRow row in result.Rows)
            {
                returnVideo.Name = (string)row["Name"];
                returnVideo.GUID = row["GUID"].ToString();
                returnVideo.Link = (string)row["Link"];
                returnVideo.UserID = (int)row["UserID"];
                returnVideo.CreatedOn = (string)row["CreatedOn"];
            }
            return returnVideo;
        }

        static public string GetFullReference(VideoModel video)
        {
            string toReturn = linkSource;
            toReturn += video.Link;
            return toReturn;
        }

        static public VideoModel GetVideoByName(string name)
        {
            var query = $@"SELECT * FROM [dbo].[Video] WHERE Name = '{name}'";
            var result = DB.ExecuteQuery(query);
            VideoModel returnVideo = new VideoModel();
            foreach (DataRow row in result.Rows)
            {
                returnVideo.Name = (string)row["Name"];
                returnVideo.GUID = row["GUID"].ToString();
                returnVideo.Link = (string)row["Link"];
                returnVideo.UserID = (int)row["UserID"];
                returnVideo.CreatedOn = (string)row["CreatedOn"];
            }
            return returnVideo;
        }


        static public void RenameVideo(VideoModel video, string newName)
        {
            var query = $@"UPDATE [dbo].[Video] SET Name = '{newName}.jpeg' WHERE GUID = {video.GUID}";
            var result = DB.ExecuteQuery(query);
        }

        [HttpPost]
        static public string DeleteVideoByName()
        {
            HttpContext context = System.Web.HttpContext.Current;
            var videoName = context.Request.Params["name"];

            VideoModel videoToDelete = GetVideoByName(videoName);

            var query = $@"DELETE FROM [dbo].[Video] WHERE Name = '{videoName}'";
            var result = DB.ExecuteQuery(query);

            if (!result.HasErrors)
            {
                var delResParams = new DelResParams()
                {
                    PublicIds = new List<string> { videoToDelete.Link }
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
        public string GetVideoLinkByName()
        {
            try
            {
                HttpContext context = System.Web.HttpContext.Current;
                var videoName = context.Request.Params["name"];

                VideoModel current = GetVideoByName(videoName);

                return GetFullReference(current);
            }

            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return "<null_link_on_error>";
            }
        }

        [HttpPost]
        public string GetVideoHashByName()
        {
            try
            {
                HttpContext context = System.Web.HttpContext.Current;
                var videoName = context.Request.Params["name"];

                VideoModel current = GetVideoByName(videoName);

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