using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class Post
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? ThreadId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? LastUpdate { get; set; }
        public Guid? PostId { get; set; }
    }

    public class PostViewModels
    {
        public Guid? Id { get; set; }
        public Guid? ThreadId { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string ReceiverName { get; set; }
        public string ProfilePic { get; set; }
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime? LastUpdate { get; set; }
        public Guid? PostId { get; set; }
        public string ReplyToContent { get; set; }
        public List<Guid> PostReplies { get; set; }

    }
}