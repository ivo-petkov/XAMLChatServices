using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace Chat.Services
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = 
                new CamelCasePropertyNamesContractResolver();

            config.Routes.MapHttpRoute(
                   name: "ContactsApi",
                   routeTemplate: "api/contacts/{action}/{id}",
                   defaults: new { controller = "contacts", id = RouteParameter.Optional }
               );

            config.Routes.MapHttpRoute(
                   name: "UserApi",
                   routeTemplate: "api/users/{action}/{id}",
                   defaults: new { controller = "users", id=RouteParameter.Optional }
               );

            config.Routes.MapHttpRoute(
                   name: "MessagesApi",
                   routeTemplate: "api/messages/{action}/{id}",
                   defaults: new { controller = "messages", id = RouteParameter.Optional }
               );

            config.Routes.MapHttpRoute(
                   name: "ConversationsApi",
                   routeTemplate: "api/conversations/{action}/{id}",
                   defaults: new { controller = "conversations", id = RouteParameter.Optional }
               );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
