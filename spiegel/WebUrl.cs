using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spiegel
{
    class WebUrl
    {
        public struct Query
        {
            public string parameter;
            public string value;

            public Query(string parameter, string value)
            {
                this.parameter = parameter;
                this.value = value;
            }
        }

        private string protocol;
        private string host;
        private int port;
        private string[] path;
        private Query[] query;


        public WebUrl(string protocol, string host, int port)
        {
            this.protocol = protocol;
            this.host = host;
            this.port = port;
        }

        public void addPath(string[] path)
        {
            this.path = path;
        }

        public void addQuery(Query[] query)
        {
            this.query = query;
        }

        public Uri compose()
        {
            string url = protocol + "://" + host + ":" + port.ToString();
            foreach (string pathSection in path)
            {
                url += '/' + pathSection;
            }

            if (query != null && query.Length != 0)
            {
                url += '?';
                foreach (Query q in query)
                {
                    url += q.parameter;
                    if (q.value != null)
                    {
                        url += '=' + q.value;
                    }

                    if (!q.Equals(query.Last()))
                    {
                        url += '&';
                    }
                }
            }

            return new Uri(url);
        }
    }
}
