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
using Forum.WebApi.Attributes;

namespace Chat.Services.Controllers
{
    public class MessagesController : ApiController
    {
        private MessagesRepository messagesRepository;
        private UsersRepository usersRepository;

        public MessagesController()
        {
            var context = new ChatDatabaseContext();
            messagesRepository = new MessagesRepository(context);
            usersRepository = new UsersRepository(context);
        }

        [HttpPost]
        [ActionName("send")]
        public HttpResponseMessage Post([FromBody]Message value,
            [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {
            var user = usersRepository.GetBySessionKey(sessionKey);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
            }

            value.Conversation.Messages = new Collection<Message>();
            messagesRepository.Add(value);

            var receiver = (value.Conversation.FirstUser.Username == user.Username)
                               ? value.Conversation.SecondUser
                               : value.Conversation.FirstUser;

            usersRepository.AddMissedConversation(receiver, value.Conversation);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [ActionName("byconversation")]
        public HttpResponseMessage GetMessagesByConversations(int id,
            [ValueProvider(typeof(HeaderValueProviderFactory<String>))] String sessionKey)
        {
            var user = usersRepository.GetBySessionKey(sessionKey);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid session key");
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, 
                messagesRepository.GetByConversation(id));
        }
    }
}
