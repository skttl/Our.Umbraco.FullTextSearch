using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.FullTextSearch.Models
{
    public class CacheExpiryRule
    {
        public CacheExpiryRule(int expires, string contentTypeAlias = null, string xPath = null)
        {
            Expires = expires;
            ContentTypeAlias = contentTypeAlias;
            XPath = xPath;
        }
        public string ContentTypeAlias { get; set; }
        public string XPath { get; set; }
        public int Expires { get; set; }
    }
}
