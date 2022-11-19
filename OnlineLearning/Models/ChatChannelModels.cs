using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class ChatChannel
    {
        public Guid Id { get; set; }
        public string ChatUserOne { get; set; }
        public string ChatUserTwo { get; set; }
        public bool UserOneClearedHistory { get; set; }
        public DateTime? UserOneClearedHistoryTime { get; set; }
        public bool UserTwoClearedHistory { get; set; }
        public DateTime? UserTwoClearedHistoryTime { get; set; }
        public bool UserOneDeletedChat { get; set; }
        public DateTime? UserOneDeletedChatTime { get; set; }
        public bool UserTwoDeletedChat { get; set; }
        public DateTime? UserTwoDeletedChatTime { get; set; }
    }
    public class ChatChannelViewModels
    {
        public Guid Id { get; set; }
        public string ChatUserOne { get; set; }
        public string CurrentUserName { get; set; }
        public string ChatUserTwo { get; set; }
        public string ChatReceiverId { get; set; }
        public string ChatReceiverName { get; set; }
        public string ChatReceiverProfilePic { get; set; }
        public DateTime? SentTime { get; set; }
        public bool UserOneDeletedChat { get; set; }
        public bool UserTwoDeletedChat { get; set; }
        public DateTime? UserOneDeletedChatTime { get; set; }
        public DateTime? UserTwoDeletedChatTime { get; set; }

    }
}