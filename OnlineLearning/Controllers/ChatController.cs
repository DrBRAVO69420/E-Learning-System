using Microsoft.AspNet.Identity;
using OnlineLearning.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineLearning.Controllers
{
    public class ChatController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();
        GeneralFunctionController generalFunction = new GeneralFunctionController();

        // GET: Chat
        public ActionResult Index()
        {
            return View();
        }

        //Id = receiver id
        public ActionResult Message(string Id)
        {
            string currentUserId = User.Identity.GetUserId();
            ChatViewModels models = new ChatViewModels();
            models.ChatChannelList = new List<ChatChannelViewModels>();   
            models.ChatChannelList = (from t1 in db.ChatChannels
                                      where t1.ChatUserOne == currentUserId || t1.ChatUserTwo == currentUserId
                                      select new ChatChannelViewModels
                                      {
                                          Id = t1.Id,
                                          ChatUserOne = t1.ChatUserOne,
                                          ChatUserTwo = t1.ChatUserTwo,
                                          UserOneDeletedChat = t1.UserOneDeletedChat,
                                          UserTwoDeletedChat = t1.UserTwoDeletedChat,
                                          UserOneDeletedChatTime = t1.UserOneDeletedChatTime,
                                          UserTwoDeletedChatTime = t1.UserTwoDeletedChatTime,
                                          ChatReceiverId = t1.ChatUserOne != currentUserId ? t1.ChatUserOne : t1.ChatUserTwo,
                                          CurrentUserName = t1.ChatUserOne == currentUserId ? db.AspNetUsers.Where(a => a.Id == t1.ChatUserOne).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault() :
                                            db.AspNetUsers.Where(a => a.Id == t1.ChatUserTwo).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                          ChatReceiverName = t1.ChatUserOne != currentUserId ? db.AspNetUsers.Where(a => a.Id == t1.ChatUserOne).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault() :
                                            db.AspNetUsers.Where(a => a.Id == t1.ChatUserTwo).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                          SentTime = db.Chats.Where(a => a.ChannelId == t1.Id).Select(a => a.SentTime).OrderByDescending(o => o.Value).FirstOrDefault(),
                                          ChatReceiverProfilePic = t1.ChatUserOne != currentUserId ? db.AspNetUsers.Where(a => a.Id == t1.ChatUserOne).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault() :
                                            db.AspNetUsers.Where(a => a.Id == t1.ChatUserTwo).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault()
                                      }).OrderByDescending(o => o.SentTime).ToList();
            models.ChatChannelList.RemoveAll(a => a.ChatUserOne == currentUserId && a.UserOneDeletedChat == true && a.SentTime < a.UserOneDeletedChatTime);
            models.ChatChannelList.RemoveAll(a => a.ChatUserTwo == currentUserId && a.UserTwoDeletedChat == true && a.SentTime < a.UserTwoDeletedChatTime);

            models.CurrentUserId = currentUserId;
            models.SenderId = currentUserId;
            models.SenderName = db.AspNetUsers.Where(a => a.Id == currentUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
            //user click on chat menu on the top navigation bar, means didn't specify any receiver, so just return the first(newest) chat channel
            if (Id == null || Id == Guid.Empty.ToString())
            {
                models.ReceiverId = models.ChatChannelList.Select(a => a.ChatReceiverId).FirstOrDefault();
                Id = models.ReceiverId;
                models.ReceiverName = models.ChatChannelList.Select(a => a.ChatReceiverName).FirstOrDefault();
                models.ChannelId = models.ChatChannelList.Select(a => a.Id).FirstOrDefault();
            }
            //user click on search user > send message button, means it specify to chat with a specific user OR
            //in the chat list, user click on a receiver name on the left bar, means it specify a chat channel, load the messages in the chat channel
            else
            {
                Guid channelwithreceiver = models.ChatChannelList.Where(a => a.ChatReceiverId == Id).Select(a => a.Id).FirstOrDefault(); //get the chat channel id
                string receiverName = db.AspNetUsers.Where(a => a.Id == Id).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
                //if the channel with receiver is null, means that user click on send message button on search user list AND never started to chat with the user before 
                if (channelwithreceiver  == Guid.Empty || channelwithreceiver == null)
                {
                    //create new chat channel model
                    ChatChannelViewModels channelViewModels = new ChatChannelViewModels();
                    channelViewModels.ChatReceiverId = Id;
                    channelViewModels.ChatReceiverName = receiverName;
                    channelViewModels.ChatReceiverProfilePic = db.AspNetUsers.Where(a => a.Id == Id).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault();
                    models.ChatChannelList.Add(channelViewModels);
                }
                else
                {
                    //assign the channel id to models.ChannelId
                    models.ChannelId = channelwithreceiver;
                }
                models.ReceiverId = Id;
                models.ReceiverName = receiverName;
            }
            //read all messages with the selected receiver
            models.ChatWithReceiver = new List<ChatViewModels>();
            string channelUserOne = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.ChatUserOne).FirstOrDefault();
            string channelUserTwo = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.ChatUserTwo).FirstOrDefault();
            bool userClearedHistory = false;
            DateTime? clearHistoryTime = generalFunction.GetSystemTimeZoneDateTimeNow();
            bool userDeletedChat = false;
            DateTime? userDeletedChatTime = generalFunction.GetSystemTimeZoneDateTimeNow();
            //check whether current user has clear the chat history or deleted chat before
            if (channelUserOne == currentUserId)
            {
                userClearedHistory = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.UserOneClearedHistory).FirstOrDefault();
                clearHistoryTime = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.UserOneClearedHistoryTime).FirstOrDefault();
                userDeletedChat = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.UserOneDeletedChat).FirstOrDefault();
                userDeletedChatTime = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.UserOneDeletedChatTime).FirstOrDefault();
            }
            else if (channelUserTwo == currentUserId)
            {
                userClearedHistory = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.UserTwoClearedHistory).FirstOrDefault();
                clearHistoryTime = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.UserTwoClearedHistoryTime).FirstOrDefault();
                userDeletedChat = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.UserTwoDeletedChat).FirstOrDefault();
                userDeletedChatTime = db.ChatChannels.Where(a => a.Id == models.ChannelId).Select(a => a.UserTwoDeletedChatTime).FirstOrDefault();
            }
            if (userClearedHistory == false && userDeletedChat == false)
            {
                models.ChatWithReceiver = (from t1 in db.ChatChannels
                                           join t2 in db.Chats on t1.Id equals t2.ChannelId
                                           where t1.Id == models.ChannelId
                                           //where ( t1.ChatUserOne == Id && t1.ChatUserTwo == currentUserId) 
                                           //||  (t1.ChatUserOne == currentUserId && t1.ChatUserTwo == Id)
                                           select new ChatViewModels
                                           {
                                               Id = t2.Id,
                                               ChannelId = t1.Id,
                                               Message = t2.Message,
                                               SenderId = t2.SenderId,
                                               SenderName = db.AspNetUsers.Where(a => a.Id == t2.SenderId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                               SentTime = t2.SentTime
                                           }).OrderBy(o => o.SentTime).ToList();
            }
            else if (userClearedHistory == true && userDeletedChat == false)
            {
                models.ChatWithReceiver = (from t1 in db.ChatChannels
                                           join t2 in db.Chats on t1.Id equals t2.ChannelId
                                           where t1.Id == models.ChannelId && t2.SentTime > clearHistoryTime
                                           //where ( t1.ChatUserOne == Id && t1.ChatUserTwo == currentUserId) 
                                           //||  (t1.ChatUserOne == currentUserId && t1.ChatUserTwo == Id)
                                           select new ChatViewModels
                                           {
                                               Id = t2.Id,
                                               ChannelId = t1.Id,
                                               Message = t2.Message,
                                               SenderId = t2.SenderId,
                                               SenderName = db.AspNetUsers.Where(a => a.Id == t2.SenderId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                               SentTime = t2.SentTime
                                           }).OrderBy(o => o.SentTime).ToList();
            }
            else if (userClearedHistory == false && userDeletedChat == true)
            {
                models.ChatWithReceiver = (from t1 in db.ChatChannels
                                           join t2 in db.Chats on t1.Id equals t2.ChannelId
                                           where t1.Id == models.ChannelId && t2.SentTime > userDeletedChatTime
                                           //where ( t1.ChatUserOne == Id && t1.ChatUserTwo == currentUserId) 
                                           //||  (t1.ChatUserOne == currentUserId && t1.ChatUserTwo == Id)
                                           select new ChatViewModels
                                           {
                                               Id = t2.Id,
                                               ChannelId = t1.Id,
                                               Message = t2.Message,
                                               SenderId = t2.SenderId,
                                               SenderName = db.AspNetUsers.Where(a => a.Id == t2.SenderId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                               SentTime = t2.SentTime
                                           }).OrderBy(o => o.SentTime).ToList();
            }
            else
            {
                models.ChatWithReceiver = (from t1 in db.ChatChannels
                                           join t2 in db.Chats on t1.Id equals t2.ChannelId
                                           where t1.Id == models.ChannelId && t2.SentTime > clearHistoryTime
                                           && t2.SentTime > userDeletedChatTime
                                           select new ChatViewModels
                                           {
                                               Id = t2.Id,
                                               ChannelId = t1.Id,
                                               Message = t2.Message,
                                               SenderId = t2.SenderId,
                                               SenderName = db.AspNetUsers.Where(a => a.Id == t2.SenderId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                               SentTime = t2.SentTime
                                           }).OrderBy(o => o.SentTime).ToList();
            }
            return View(models);
        }

        public string SaveChat(string senderid, string receiverid, string message)
        {
            string error = "";
            try
            {
                ChatChannel chatChannel = new ChatChannel();
                chatChannel = db.ChatChannels.Where(a => (a.ChatUserOne == senderid && a.ChatUserTwo == receiverid) || (a.ChatUserOne == receiverid && a.ChatUserTwo == senderid)).FirstOrDefault();
                if (chatChannel == null)
                {
                    chatChannel = new ChatChannel();
                    chatChannel.Id = Guid.NewGuid();
                    chatChannel.ChatUserOne = senderid;
                    chatChannel.ChatUserTwo = receiverid;
                    db.ChatChannels.Add(chatChannel);
                    db.SaveChanges();
                }
                Chat chat = new Chat();
                chat.ChannelId = chatChannel.Id;
                chat.Id = Guid.NewGuid();
                chat.SenderId = senderid;
                chat.SentTime = generalFunction.GetSystemTimeZoneDateTimeNow();
                chat.Message = message;
                db.Chats.Add(chat);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                error = ex.InnerException.Message.ToString();
            }
            return error;
        }

        public ActionResult DeleteChat(Guid? Id)
        {
            string currentUserId = User.Identity.GetUserId();
            try
            {
                ChatChannel chatChannel = db.ChatChannels.Where(a => a.Id == Id).FirstOrDefault();
                if (chatChannel.ChatUserOne == currentUserId)
                {
                    chatChannel.UserOneDeletedChat = true;
                    chatChannel.UserOneDeletedChatTime = generalFunction.GetSystemTimeZoneDateTimeNow();
                }
                else if (chatChannel.ChatUserTwo == currentUserId)
                {
                    chatChannel.UserTwoDeletedChat = true;
                    chatChannel.UserTwoDeletedChatTime = generalFunction.GetSystemTimeZoneDateTimeNow();
                }
                db.Entry(chatChannel).State = EntityState.Modified;
                if (chatChannel.UserOneClearedHistoryTime != null && chatChannel.UserTwoClearedHistoryTime != null && chatChannel.UserOneDeletedChatTime != null && chatChannel.UserTwoDeletedChatTime != null)
                {
                    //if the chat message is cleared or deleted by both users(sender and receiver) in the chat, then the message can be deleted from the database to free up the space in database, because both users already no need to see the messages
                    List<Chat> chats = db.Chats.Where(a => a.ChannelId == Id && a.SentTime < chatChannel.UserOneClearedHistoryTime && a.SentTime < chatChannel.UserTwoClearedHistoryTime && a.SentTime < chatChannel.UserOneDeletedChatTime && a.SentTime < chatChannel.UserTwoDeletedChatTime).ToList();
                    foreach (var chat in chats)
                    {
                        db.Chats.Remove(chat);
                    }
                }
                db.SaveChanges();
                TempData["DeleteChatSuccess"] = "Deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["DeleteChatFailed"] = ex.InnerException.Message.ToString();
            }
            return RedirectToAction("message");
        }

        public ActionResult ClearChatHistory(Guid? Id)
        {
            string error = "";
            string currentUserId = User.Identity.GetUserId();
            try
            {
                ChatChannel chatChannel = db.ChatChannels.Where(a => a.Id == Id).FirstOrDefault();
                if (chatChannel.ChatUserOne == currentUserId)
                {
                    chatChannel.UserOneClearedHistory = true;
                    chatChannel.UserOneClearedHistoryTime = generalFunction.GetSystemTimeZoneDateTimeNow();
                }
                else if (chatChannel.ChatUserTwo == currentUserId)
                {
                    chatChannel.UserTwoClearedHistory = true;
                    chatChannel.UserTwoClearedHistoryTime = generalFunction.GetSystemTimeZoneDateTimeNow();
                }
                db.Entry(chatChannel).State = EntityState.Modified;
                if (chatChannel.UserOneClearedHistory && chatChannel.UserTwoClearedHistory)
                {                  
                    //if the chat message is cleared by both users(sender and receiver) in the chat, then the message can be deleted from the database to free up the space in database, because both users already no need to see the messages
                    List<Chat> chathistory = db.Chats.Where(a => a.ChannelId == Id && a.SentTime < chatChannel.UserOneClearedHistoryTime && a.SentTime < chatChannel.UserTwoClearedHistoryTime).ToList();
                    foreach (var chat in chathistory)
                    {
                        db.Chats.Remove(chat);
                    }
                }
                db.SaveChanges();
                TempData["DeleteChatHistorySuccess"] = "Chat history is cleared";
            }
            catch (Exception ex)
            {
                error = ex.InnerException.Message.ToString();
            }
            return RedirectToAction("message");
        }
    }
}