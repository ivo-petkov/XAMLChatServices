using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using Chat.DataLayer;
using Chat.Models;
using Chat.Repositories;
using Chat.Services.Models;
using Forum.WebApi.Attributes;

namespace Chat.Services.Controllers
{
    public class UsersController : ApiController
    {
        private UsersRepository usersRepository;
        private const int SessionKeyLength = 50;
        private const string SessionKeyChars =
            "qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM";
        private static readonly Random rand = new Random();

        public UsersController()
        {
            var context = new ChatDatabaseContext();
            this.usersRepository = new UsersRepository(context);
        }

        [HttpGet]
        [ActionName("all")]
        public HttpResponseMessage GetAllUsers(
            [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {
            var user = usersRepository.GetBySessionKey(sessionKey);
            if(user != null)
            {
                var users = usersRepository.All()
                    .Select(u => new UserModel()
                                     {
                                         Id = u.Id,
                                         Username = u.Username,
                                         FirstName = u.FirstName,
                                         LastName = u.LastName,
                                         ProfilePictureUrl = u.ProfilePictureUrl
                                     });

                return Request.CreateResponse(HttpStatusCode.OK, users);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
        }

        [HttpPost]
        [ActionName("search")]
        public HttpResponseMessage SearchUsers([FromBody]QueryModel value, 
            [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {

            var user = usersRepository.GetBySessionKey(sessionKey);
            if(user != null)
            {
                var users = usersRepository.All()
                    .Where(u => u.Username.ToLower().Contains(value.QueryText.ToLower()))
                    .Where(u => u.Username != user.Username)
                    .Select(u => new UserModel()
                                     {
                                         Id = u.Id,
                                         Username = u.Username,
                                         FirstName = u.FirstName,
                                         LastName = u.LastName,
                                         ProfilePictureUrl = u.ProfilePictureUrl
                                     }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, users);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
        }

        [HttpGet]
        [ActionName("byid")]
        public User GetById(int id)
        {
            return usersRepository.Get(id);
        }

        [HttpPost]
        [ActionName("session")]
        public HttpResponseMessage ValidateSessionKey([FromBody]User value)
        {
            var user = usersRepository.GetBySessionKey(value.SessionKey);
            if(user != null)
            {
                usersRepository.SetOnline(user, true);
                return Request.CreateResponse(HttpStatusCode.OK,
                                              new UserLoggedModel()
                                                  {Username = user.Username, SessionKey = user.SessionKey});
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
        }

        [HttpPost]
        [ActionName("edit")]
        public HttpResponseMessage EditUser([FromBody]UsedEditModel value,
            [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {
            var user = usersRepository.GetBySessionKey(sessionKey);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
            }

            var userToEdit = new User()
                                 {
                                     Id = user.Id,
                                     Username = user.Username,
                                     FirstName = value.FirstName,
                                     LastName = value.LastName,
                                     PasswordHash = value.OldPasswordHash,
                                     ProfilePictureUrl = value.ProfilePictureUrl
                                 };

            if(usersRepository.EditUser(userToEdit, value.NewPasswordHash))
            {
                var updatedUser = usersRepository.Get(userToEdit.Id);
                var userModel = new UserModel()
                {
                    Id = updatedUser.Id,
                    Username = updatedUser.Username,
                    SessionKey = sessionKey,
                    FirstName = updatedUser.FirstName,
                    LastName = updatedUser.LastName,
                    Online = true,
                    ProfilePictureUrl = updatedUser.ProfilePictureUrl
                };

                return Request.CreateResponse(HttpStatusCode.OK, userModel);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, "Could not edit user");
        }

        [HttpPost]
        [ActionName("register")]
        public HttpResponseMessage Register([FromBody]User value)
        {
            if(string.IsNullOrEmpty(value.Username) || string.IsNullOrWhiteSpace(value.Username)
                || value.Username.Length < 5 || value.Username.Length > 30)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                                              "Invalid username. Should be between 5 and 30 characters");
            }

            if(usersRepository.GetByUsername(value.Username) != null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                                              "Username already exists");
            }

            usersRepository.Add(value);
            var sessionKey = GenerateSessionKey(value.Id);
            usersRepository.SetSessionKey(value, sessionKey);
            usersRepository.SetOnline(value, true);
            var userModel = new UserModel()
                                {
                                    Id = value.Id,
                                    Username = value.Username,
                                    SessionKey = sessionKey,
                                    FirstName = value.Username,
                                    LastName = value.LastName,
                                    Online = true,
                                    ProfilePictureUrl = value.ProfilePictureUrl
                                };

            return Request.CreateResponse(HttpStatusCode.Created, userModel);
        }

        [HttpPost]
        [ActionName("login")]
        public HttpResponseMessage Login([FromBody]User value)
        {
            var user = usersRepository.CheckLogin(value.Username, value.PasswordHash);
            if (user != null)
            {
                var sessionKey = GenerateSessionKey(user.Id);
                usersRepository.SetSessionKey(user, sessionKey);
                usersRepository.SetOnline(user, true);

                var userModel = new UserModel()
                                    {
                                        Id = user.Id,
                                        SessionKey = sessionKey, 
                                        Username = user.Username,
                                        FirstName = user.FirstName,
                                        LastName = user.LastName,
                                        ProfilePictureUrl = user.ProfilePictureUrl,
                                        Online = true
                                    };

                return Request.CreateResponse(HttpStatusCode.OK, userModel);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid username or password");
            }
        }

        [HttpGet]
        [ActionName("logout")]
        public HttpResponseMessage Logout(
            [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {
            var user = usersRepository.GetBySessionKey(sessionKey);
            if(user == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
            }

            usersRepository.SetOnline(user, false);
            usersRepository.Logout(user);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ActionName("byusername")]
        public User GetByUsername([FromBody]User userData)
        {
            var user = usersRepository.GetByUsername(userData.Username);
            return user;
        }

        [HttpGet]
        [ActionName("offline")]
        public HttpResponseMessage SetUserOffline(
            [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {
            var user = usersRepository.GetBySessionKey(sessionKey);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
            }

            usersRepository.SetOnline(user, false);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private string GenerateSessionKey(int userId)
        {
            StringBuilder skeyBuilder = new StringBuilder(SessionKeyLength);
            skeyBuilder.Append(userId);
            while (skeyBuilder.Length < SessionKeyLength)
            {
                var index = rand.Next(SessionKeyChars.Length);
                skeyBuilder.Append(SessionKeyChars[index]);
            }
            return skeyBuilder.ToString();
        }
    }
}
