using System;
using System.Collections.Specialized;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            VkApiClient vkApiClient = new VkApiClient(
                "4070029",
                "groups",
                "5.5",
                "+79164556832",
                "SslDemo:)");

            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("gid", "30621569");
            parameters.Add("tid", "27884279");
            parameters.Add("extended", "1");
            parameters.Add("offset", "0");
            parameters.Add("count", "100");

            string jsonBoardMessages = vkApiClient.ExecuteMethod("board.getComments", parameters);

            Console.WriteLine(jsonBoardMessages);
        }
    }
}
