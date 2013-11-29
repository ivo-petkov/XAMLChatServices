using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using Chat.DataLayer;
using Chat.Models;
using Chat.Repositories;
using Chat.Services.Models;
using Forum.WebApi.Attributes;

namespace Chat.Services.Controllers
{
    public class ConversationsController : ApiController
    {
        private ConversationsRepository conversationsRepository;
        private UsersRepository usersRepository;

        public ConversationsController()
        {
            var context = new ChatDatabaseContext();
            this.conversationsRepository = new ConversationsRepository(context);
            this.usersRepository = new UsersRepository(context);
        }

        [HttpPost]
        [ActionName("start")]
        public HttpResponseMessage Start([FromBody]Conversation conversationData,
            [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {
            var user = usersRepository.GetBySessionKey(sessionKey);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
            }

            User[] users = new User[2];
            users[0] = conversationData.FirstUser;
            users[1] = conversationData.SecondUser;

            var conversation = GetByUsers(users);
            if(conversation == null)
            {
                conversationsRepository.Add(new Conversation()
                                                {
                                                    FirstUser = usersRepository.GetByUsername(users[0].Username),
                                                    SecondUser = usersRepository.GetByUsername(users[1].Username)
                                                });

                return Request.CreateResponse(HttpStatusCode.OK, GetByUsers(users));
            }

            if(user.MissedConversations.Any(m => m.ConversationId == conversation.Id))
            {
                var missed = user.MissedConversations.FirstOrDefault(m => m.ConversationId == conversation.Id);
                conversationsRepository.MarkRead(missed.Id);
            }

            return Request.CreateResponse(HttpStatusCode.OK, conversation);
        }

        [HttpGet]
        [ActionName("markread")]
        public HttpResponseMessage ReadMissed(int id, 
            [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {
            var user = usersRepository.GetBySessionKey(sessionKey);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
            }

            conversationsRepository.MarkRead(id);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [ActionName("missed")]
        public HttpResponseMessage GetMissedConversations(int id,
             [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {
            var user = usersRepository.GetBySessionKey(sessionKey);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
            }

            var missed = usersRepository.GetMissedConversations(user, id);
            return Request.CreateResponse(HttpStatusCode.OK, missed);
        }
       
        private Conversation GetByUsers(User[] users)
        {
            var firstUsername = users[0].Username;
            var secondUsername = users[1].Username;

            var conversation = conversationsRepository.GetByUsers(firstUsername, secondUsername);
            if(conversation != null)
            {
                
                return new Conversation()
                           {
                               Id = conversation.Id,
                               FirstUser = new User()
                                               {
                                                   Id = conversation.FirstUser.Id,
                                                   Username = conversation.FirstUser.Username,
                                                   FirstName = conversation.FirstUser.FirstName,
                                                   LastName = conversation.FirstUser.LastName,
                                                   ProfilePictureUrl = conversation.FirstUser.ProfilePictureUrl
                                               },
                               SecondUser = new User()
                               {
                                   Id = conversation.SecondUser.Id,
                                   Username = conversation.SecondUser.Username,
                                   FirstName = conversation.SecondUser.FirstName,
                                   LastName = conversation.SecondUser.LastName,
                                   ProfilePictureUrl = conversation.SecondUser.ProfilePictureUrl
                               },
                           };
            }

            return conversation;
        }
    }
}
