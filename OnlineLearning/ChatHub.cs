using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using OnlineLearning.Controllers;

namespace OnlineLearning
{
    public class ChatHub : Hub
    {
        GeneralFunctionController generalFunction = new GeneralFunctionController();
        public void Send(string senderid, string receiverid, string name, string message)
        {
            DateTime? timenow = generalFunction.GetSystemTimeZoneDateTimeNow();
            Clients.User(receiverid).send(message);
            Clients.User(senderid).addNewMessageToPage(senderid, name, message, timenow.Value.ToShortTimeString());
            Clients.User(receiverid).addNewMessageToPage(senderid, name, message, timenow.Value.ToShortTimeString());
        }

        public void Notify(string senderid, string receiverid, string sendername)
        {
            Clients.User(receiverid).send("New message from " + sendername);
            Clients.User(receiverid).addNotificationMessage("New message from " + sendername);
        }
    }
}