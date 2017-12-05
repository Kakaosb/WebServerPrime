using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace WebServerPrimes
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = args.Length > 0 ? args[0] : "localhost";
            int port = args.Length > 1 ? Convert.ToInt32(args[1]) : 8080;
            string route = args.Length > 2 ? args[2] : "app/primes";

            var httpServer = new HttpServer(host, port, route);

            httpServer.Start();
        }
    }

    public class HttpServer
    {
        private HttpListener _listener; 
   
        public HttpServer(string host, int port, string route )
        {
           
            _listener = new HttpListener();


            string url = string.Format("http://{0}:{1}/{2}/", host, port, route);
            _listener.Prefixes.Add(url);
        }

        public void Start()
        {
            _listener.Start();


            while (true)
            {
                HttpListenerContext context = _listener.GetContext();
                HttpListenerResponse response = context.Response;

                var min = Convert.ToInt32(context.Request.QueryString["min"]);
                var max = Convert.ToInt32(context.Request.QueryString["max"]);

                string msg = CalculateHtmlPage(min,max);

                byte[] buffer = Encoding.UTF8.GetBytes(msg);

                response.ContentLength64 = buffer.Length;
                Stream st = response.OutputStream;
                st.Write(buffer, 0, buffer.Length);

                context.Response.Close();
            }
        }

        public static bool IsPrime(int number)
        {
            if (number == 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0) return false;
            }

            return true;
        }

        private string CalculateHtmlPage(int min, int max)
        {
            int primeDistance = 0;
            int index = 1;

            StringBuilder pageContent = new StringBuilder();

            pageContent.AppendLine("<html><body><table border=1 ><tr><td>#</td><td>Prime</td><td>Distance</td></tr>");

            HtmlTableRow prevPrime = null;

            List<HtmlTableRow> htmlTableRows = new List<HtmlTableRow>();

            for (int currentNumber = min; currentNumber <= max; currentNumber++)
            {
                primeDistance++;

                if (IsPrime(currentNumber))
                {
                    if (prevPrime != null)
                    {
                        prevPrime.Dist = primeDistance;
                    }

                    var row = new HtmlTableRow();

                    row.Index = index;
                    row.Prime = currentNumber;

                    prevPrime = row;

                    htmlTableRows.Add(row);

                    primeDistance = 0;

                    index++;
                }
            }

            int prevPrimeDistance = 0;
            int prevPrimeCurrentNumber = prevPrime.Prime;

            while (true)
            {
                prevPrimeCurrentNumber++;
                prevPrimeDistance++;

                if (IsPrime(prevPrimeCurrentNumber))
                {
                    prevPrime.Dist = prevPrimeDistance;

                    break;
                }
            }

            foreach (var tableRow in htmlTableRows)
            {
                pageContent.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>"
                    , tableRow.Index, tableRow.Prime, tableRow.Dist));
            }
            pageContent.AppendLine("</table></body></html>");

            return pageContent.ToString();
        }


        ~HttpServer()
        {
    
            if (_listener != null)
            {
               
                _listener.Stop();
            }
        }
    }

    public class HtmlTableRow
    {
        public int Index { get; set; }
        public int Prime { get; set; }
        public int Dist { get; set; }

    }
}
