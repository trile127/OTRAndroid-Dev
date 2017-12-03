using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkWithXamarin.Core
{


    public class ChatUserDetail
    {
        [Key]
        public int ID { get; set; }
        public string ConnectionID { get; set; }
        public string UserName { get; set; }
        public string EmailID { get; set; }
    }

    public class ChatPrivateMessageDetail
    {
        [Key]
        public int ID { get; set; }
        public string MasterEmailID { get; set; }
        public string ChattoEmailID { get; set; }
        public string Message { get; set; }
    }

    public class ChatPrivateMessage
    {
        [Key]
        public int ID { get; set; }
        public string UserName { get; set; }
        public string EmailID { get; set; }
    }

    public class ChatMessageDetail
    {
        [Key]
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string EmailID { get; set; }
    }

}
