using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class Chat
    {
		[Key]
		public Guid Id { get; set; }
		public Guid? ChannelId { get; set; }
		public string SenderId { get; set; }
		public string Message { get; set; }
		public DateTime? SentTime { get; set; }
    }

	public class ChatViewModels
	{
		public Guid? Id { get; set; }
		public Guid? ChannelId { get; set; }
		public string CurrentUserId { get; set; }
		public string SenderId { get; set; }
		public string SenderName { get; set; }
		public string ReceiverId { get; set; }
		public string ReceiverName { get; set; }
		public string Message { get; set; }
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:ss tt}", ApplyFormatInEditMode = true)]
		public DateTime? SentTime { get; set; }
		public List<ChatViewModels> ChatWithReceiver { get; set; }
		public List<ChatChannelViewModels> ChatChannelList { get; set; }
	}
}

