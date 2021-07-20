using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TalkToApi.V1.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("FromId")]
        public ApplicationUser From { get; set; }
        [ForeignKey("ToId")]
        public ApplicationUser To { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
