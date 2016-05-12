using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using ProjectApi.Models;

namespace ProjectApi.Controllers
{
    public class UserController : ApiController
    {
        
        [HttpGet]
        [ActionName("GetDrivers")]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(List<UserModel>))]
        public HttpResponseMessage GetDrivers()
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var users = db.User.ToList();
                        List<UserModel> userList = new List<UserModel>();
                        foreach (var item in users)
                        {
                            UserModel user = new UserModel
                            {
                                UserName = item.UserName,
                                UserId = item.Id
                            };
                            userList.Add(user);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, userList);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("Login")]
        [ResponseType(typeof(UserModel))]        
        [Authorize(Roles = "User")]
        public HttpResponseMessage Login([FromBody]UserModel user)
        {
            try
            {
                using (var db = new WpfprojectEntities())
                {
                    string userSalt = db.User.Where(x => x.UserName == user.UserName).Select(x => x.Salt).SingleOrDefault();
                    string hashedPassword;

                    using (MD5 md5Hash = MD5.Create())
                    {
                        hashedPassword = GetMd5Hash(md5Hash, user.Password + userSalt);
                    }

                    if (db.User.Any(x => x.UserName == user.UserName && x.Password == hashedPassword))
                    {
                        var loggedInUser = db.User.FirstOrDefault(x => x.UserName == user.UserName && x.Password == hashedPassword);
                        if (loggedInUser != null)
                        {
                            bool? isadmin = loggedInUser.IsAdmin;
                            user.UserId = loggedInUser.Id;
                            user.IsAdmin = (bool)isadmin;
                            user.Password = user.Password;
                            user.UserName = loggedInUser.UserName;
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, user);
                    }
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        
        [HttpPost]
        [ActionName("Register")]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage Register([FromBody]UserModel user)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        if (db.User.Any(x => x.UserName == user.UserName))
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        }
                        string salt = PasswordSalt();
                        string hashedPassword;
                        using (MD5 md5Hash = MD5.Create())
                        {
                            hashedPassword = GetMd5Hash(md5Hash, user.Password + salt);
                        }
                        User newUser = new User
                        {
                            UserName = user.UserName,
                            Password = hashedPassword,
                            Salt = salt,
                            IsAdmin = user.IsAdmin
                        };
                        db.User.Add(newUser);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public static string PasswordSalt()
        {
            var rng = new RNGCryptoServiceProvider();
            var salt = new byte[16];
            rng.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }
    }
}
