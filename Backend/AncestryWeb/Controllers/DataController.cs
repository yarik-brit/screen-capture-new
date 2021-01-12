using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Data;


using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using nQuant;
using AncestryWeb.Models;
using System.IO;

namespace AncestryWeb.Controllers
{
    public class DataController : Controller
    {
        static Account account = new Account(
            "scr-capt-rec",
            "377884457262522",
            "lwZOnLWp1KaEagB7unQVblPy40A");

        Cloudinary cloudinary = new Cloudinary(account);
        ImageModel currentImage;
        VideoModel currentVideo;
        static bool INIT_UPLOAD = true;


        static string error1 = "";
        static string error2 = "";
        static string error3 = "";
        static string error4 = "";
        static string error5 = "";
        static string error6 = "";

        [HttpPost]
        public string GetImageUrl()
        {
            try
            {
                currentImage = new ImageModel();

                HttpContext context = System.Web.HttpContext.Current;
                var dataUrl = context.Request.Params["fileurl"];
                SaveImageDataUrlToFile(dataUrl);
                string returningString = Config.BaseUrl + "Screenshot?id=" + currentImage.GUID;
                return returningString;
            }

            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return "Error occured! Sorry!!!";
            }
        }

        public void SaveImageDataUrlToFile(string dataUrl)
        {
            string compressedFileNamePath;
            var absPath = AppDomain.CurrentDomain.BaseDirectory + "images/";
            var format = ".jpeg";

            try
            {
                var matchGroups = Regex.Match(dataUrl, @"data:image/(?<type>.+?),(?<data>.+)").Groups;
                var base64Data = matchGroups["data"].Value;
                var binData = Convert.FromBase64String(base64Data);
                var fileName = GetFileTimeBasedName();


                var modifierString = "_original";
                if (!Directory.Exists(absPath))
                {
                    Directory.CreateDirectory(absPath);
                }
                var fullFileName = absPath + fileName + modifierString + format;
                System.IO.File.WriteAllBytes(fullFileName, binData);

                var quantizer = new WuQuantizer();
                using (var bitmap = new Bitmap(fullFileName))
                {
                    using (var quantized = quantizer.QuantizeImage(bitmap, 0, 0))
                    {
                        compressedFileNamePath = absPath + fileName + format;
                        currentImage.Name = fileName + format;
                        currentImage.CreatedOn = fileName;
                        currentImage.UserID = 1;
                        currentImage.GUID = Guid.NewGuid().ToString();
                        quantized.Save(compressedFileNamePath, ImageFormat.Jpeg);

                    }
                }

                var result = AddDefault(currentImage);
                if (result.HasErrors == false)
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(compressedFileNamePath)
                    };
                    var uploadResult = cloudinary.Upload(uploadParams);
                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        currentImage.Link = uploadResult.PublicId;
                        AddLink(currentImage);
                    }
                }

                System.IO.File.Delete(fullFileName); // removing original screenshot
                System.IO.File.Delete(compressedFileNamePath); // removing compressed screenshot

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public DataTable AddDefault(ImageModel imageData)
        {
            var query = $@"INSERT INTO [dbo].[Image] (UserID, GUID, Name, CreatedOn) OUTPUT INSERTED.Link values({imageData.UserID}, '{imageData.GUID}', '{imageData.Name}', '{imageData.CreatedOn}')";
            var result = DB.ExecuteQuery(query);

            Debug.WriteLine(result.HasErrors);

            return result;
        }

        public DataTable AddDefault(VideoModel videoData)
        {
            var query = $@"INSERT INTO [dbo].[Video] (UserID, GUID, Name, CreatedOn) values({videoData.UserID}, '{videoData.GUID}', '{videoData.Name}', '{videoData.CreatedOn}')";
            var result = DB.ExecuteQuery(query);

            Debug.WriteLine("add default to db - hasErrors - " + result.HasErrors);

            return result;
        }

        public void AddLink(ImageModel image)
        {
            var query = $@"UPDATE [dbo].[Image]
                            SET Link = '{image.Link}'
                            WHERE Name = '{image.Name}'";
            var result = DB.ExecuteQuery(query);

            Debug.WriteLine(result);
        }

        public void AddLink(VideoModel video)
        {
            try
            {
                var query = $@"UPDATE [dbo].[Video]
                            SET Link = '{video.Link}'
                            WHERE Name = '{video.Name}'";
                var result = DB.ExecuteQuery(query);

                Debug.WriteLine("add link to db - hasErrors - " + result.HasErrors);
            }
            catch (Exception e)
            {
                Debug.WriteLine("add link to db - hasErrors - " + e.Message);
            }

        }

        public string GetFileTimeBasedName()
        {
            var now = DateTime.UtcNow;
            string fileNameString = now.ToString("yyyy-MM-dd HH.mm.ss");
            return fileNameString;
        }



        public string UploadVideo()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Temp";
            string pathToFile = path + @"\video.mp4";
            try
            {
                var uploadParams = new VideoUploadParams()
                {
                    File = new FileDescription(pathToFile),
                    Format = "mp4",
                    EagerAsync = true
                };
                var uploadResult = cloudinary.UploadLarge(uploadParams, 20000000);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Debug.WriteLine(uploadResult.SecureUrl);
                    var fileName = GetFileTimeBasedName();
                    var format = ".mp4";
                    currentVideo = new VideoModel
                    {
                        Name = fileName + format,
                        CreatedOn = fileName,
                        UserID = 1,
                        GUID = Guid.NewGuid().ToString(),
                        Link = uploadResult.PublicId
                    };

                    AddDefault(currentVideo);
                    AddLink(currentVideo);

                    return Config.BaseUrl + @"Video?id=" + currentVideo.GUID;
                }
                else
                {
                    return Config.BaseUrl + @"Video?id=" + uploadResult.StatusCode;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                error1 = "UploadVideo_Method==" + e.Message;
                return Config.BaseUrl + @"Video?id=" + "__" + error1 + "__" + error2 + "__" + error3 + "__" + error4 + "__" + error5 + "__" + error6;
            }
        }

        [HttpPost]
        public string MultiUpload()
        {
            Debug.WriteLine("recieved chunk");
            string path = AppDomain.CurrentDomain.BaseDirectory + "Temp";
            try
            {
                var chunks = Request.InputStream;
                string newpath = Path.Combine(path, Path.GetRandomFileName());

                using (FileStream fs = System.IO.File.Create(newpath))
                {
                    byte[] bytes = new byte[550000];

                    int bytesRead;
                    while ((bytesRead = Request.InputStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        fs.Write(bytes, 0, bytesRead);
                    }
                }
            }
            catch (Exception e)
            {
                error2 = "MultiUpload_Method";
                Debug.WriteLine(e.Message + " : " + e.StackTrace);
            }
            return "chunk recorded";
        }

        [HttpPost]
        public string UploadComplete()
        {
            Debug.WriteLine("upload complete");
            string path = AppDomain.CurrentDomain.BaseDirectory + "Temp";
            string toReturn = "";
            try
            {
                string newpath = Path.Combine(path, "video.mp4");
                System.IO.File.WriteAllText(newpath, String.Empty);

                DirectoryInfo info = new DirectoryInfo(path);
                FileInfo[] files = info.GetFiles();

                // Sort by creation-time ascending 
                Array.Sort(files, delegate (FileInfo f1, FileInfo f2)
                {
                    return f1.CreationTime.CompareTo(f2.CreationTime);
                });

                string[] filePaths = new string[files.Length];
                for (int i = 0; i < filePaths.Length; i++)
                {
                    filePaths[i] = files[i].FullName;
                }

                Debug.WriteLine("Files saved -- " + filePaths.Length);

                foreach (string item in filePaths)
                {
                    if (item != newpath)
                    {
                        MergeFiles(newpath, item);
                    }

                }

                //RecompileVideo();

            }
            catch (Exception e)
            {
                error3 = "UploadComplete_Method";
                Debug.WriteLine(e.Message + " : " + e.StackTrace);
            }
            toReturn = UploadVideo();
            return toReturn;
            //return "asdasd";
        }

        private static void MergeFiles(string file1, string file2)
        {
            FileStream fs1 = null;
            FileStream fs2 = null;
            try
            {
                fs1 = System.IO.File.Open(file1, FileMode.Append);
                fs2 = System.IO.File.Open(file2, FileMode.Open);
                byte[] fs2Content = new byte[fs2.Length];
                fs2.Read(fs2Content, 0, (int)fs2.Length);
                fs1.Write(fs2Content, 0, (int)fs2.Length);
            }
            catch (Exception ex)
            {
                error4 = "MergeFiles_Method";
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
            }
            finally
            {
                fs1.Close();
                fs2.Close();
                System.IO.File.Delete(file2);
                Debug.WriteLine("closed streams");
            }
        }

        private static void RecompileVideo()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Temp";
            try
            {
                //string inputPath = path + @"\video.mp4";
                //string outputPath = path + @"\converted.mp4";





            }
            catch (Exception e)
            {
                error5 = "Recompile_Method - " + e.Message;
                Debug.WriteLine(e.Message);
            }
        }


        [HttpPost]
        public string PrepareDirectory()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Temp";
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string[] oldFiles = Directory.GetFiles(path);
                foreach (var item in oldFiles)
                {
                    System.IO.File.Delete(item);
                }

                return "Clearing working directory --- done!";
            }
            catch (Exception e)
            {
                error6 = "PrepareDirectory_Method";
                Debug.WriteLine(e.Message);
                return "Error occured on preparing directory!";
            }

        }
    }
}