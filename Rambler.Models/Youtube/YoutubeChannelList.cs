using System;
using System.Collections.Generic;
using System.Text;

namespace Rambler.Models.Youtube
{
    public class YoutubeChannelList
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public string prevPageToken { get; set; }
        public PageInfo PageInfo { get; set; }
        public IEnumerable<YoutubeChannel> items { get; set; }
    }
}
