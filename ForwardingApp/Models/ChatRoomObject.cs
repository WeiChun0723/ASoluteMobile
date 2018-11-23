using System;
using System.Collections.Generic;

namespace ASolute_Mobile.Models
{

        public class Headings
        {
            public string en { get; set; }
        }

        public class Contents
        {
            public string en { get; set; }
        }

        public class ChatRoomObject
        {
            public string app_id { get; set; }
            public List<string> include_player_ids { get; set; }
            public Headings headings { get; set; }
            public Contents contents { get; set; }
        }

}
