using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Position_Fetcher
{

    class Endpoint
    {
        public static readonly string BALANCE = "wallet/balances";
        public static readonly string POSITION = "positions";
        public static readonly string LEVERAGED_BALANCES = "lt/balances";
        public static readonly string ALL_BALANCES = "wallet/all_balances";
    }

    class Program
    {
        static string YOUR_API_SECRET = "";
        static string YOUR_API_KEY = "";
        readonly static string FTX = "https://ftx.com";
        readonly static HttpMethod METHOD = HttpMethod.Get;

        static void Main(string[] args)
        {
            try
            {

                InitialiseConfig();

                using (HttpClient client = new())
                {
                    client.BaseAddress = new(FTX);

                    while (true)
                    {
                        FetchData(client, Endpoint.POSITION);
                        FetchData(client, Endpoint.BALANCE);
                        //FetchData(client, Endpoint.LEVERAGED_BALANCES);
                        //FetchData(client, Endpoint.ALL_BALANCES);

                        Thread.Sleep(10 * 1000);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Threw exception " + ex.StackTrace);
                throw;
            }
        }

        static void FetchData(HttpClient client, string objective)
        {
            try
            {
                string endpoint = "/api/" + objective;
                var request = new HttpRequestMessage(METHOD, endpoint);
                HttpResponseMessage response = client.Send(Authenticate(request, endpoint));

                Console.WriteLine(response);
                if (objective == Endpoint.POSITION)
                {
                    ParseAndWritePositionsToCsv(response);
                }
                else if (objective == Endpoint.BALANCE)
                {
                    ParseAndWriteBalancesToCsv(response);
                }
                else if (objective == Endpoint.ALL_BALANCES)
                {
                    ParseAndWriteAllBalancesToCsv(response);
                }
                else if (objective == Endpoint.LEVERAGED_BALANCES)
                {
                    ParseAndWriteLeveragedBalancesToCsv(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Threw exception when fetching data: " + ex.StackTrace);
            }
        }

        private static void InitialiseConfig()
        {
            try
            {
                using (StreamReader reader = new("config.txt"))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        string input = reader.ReadLine();
                        if (i == 0)
                        {
                            YOUR_API_KEY = input.Split(' ')[1];
                        }
                        else if (i == 1)
                        {
                            YOUR_API_SECRET = input.Split(' ')[1];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Threw exception in InitialiseConfig: " + ex);
                throw;
            }
        }

        public static async void ParseAndWritePositionsToCsv(HttpResponseMessage response)
        {
            try
            {
                var positions = JsonSerializer.Deserialize<RootobjectPosition>(await response.Content.ReadAsStringAsync());

                Console.WriteLine("\n####\n");
                using (StreamWriter writer = new("positions.csv"))
                {
                    writer.WriteLine($"Name,Size,Side,Net Size,Long Order Size,Short Order Size,Cost,Entry Price,Unrealised PNL,Realised PNL,Initial Margin Requirement,Maintenance Margin Requirement,Open Size,Collateral Used,Estimated Liquidation Price,Time");
                    if (positions.result is null)
                    {
                        Console.WriteLine("No positions to write.");
                        return;
                    }
                    foreach (var item in positions.result)
                    {
                        if (item.size != 0)
                        {
                            string output = "";
                            foreach (var itemB in DictionaryFromType(item))
                            {
                                output += itemB.Value + ",";
                            }
                            output += DateTime.Now.ToString();
                            writer.WriteLine(output);

                            Console.WriteLine(output);
                        }
                    }
                }
                Console.WriteLine("\n####\n" + await response.Content.ReadAsStringAsync());

            }
            catch (Exception ex)
            {
                Console.WriteLine("Threw exception in ParseAndWritePositionsToCsv: " + ex.StackTrace);
            }
        }

        public static async void ParseAndWriteBalancesToCsv(HttpResponseMessage response)
        {
            try
            {

                var balances = JsonSerializer.Deserialize<RootobjectBalance>(await response.Content.ReadAsStringAsync());

                Console.WriteLine("\n####\n");
                using (StreamWriter writer = new("balances.csv"))
                {
                    writer.WriteLine($"Name,Free,Spot Borrow,Total,USD Value,Available Without Borrow");
                    if (balances.result is null)
                    {
                        Console.WriteLine("No balances to write.");
                        return;
                    }
                    foreach (var item in balances.result)
                    {
                        if (item.total != 0)
                        {
                            string output = "";
                            foreach (var itemB in DictionaryFromType(item))
                            {
                                output += itemB.Value + ",";
                            }
                            writer.WriteLine(output);

                            Console.WriteLine(output);
                        }
                    }
                }
                Console.WriteLine("\n####\n" + await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Threw exception in ParseAndWriteBalancesToCsv: " + ex.StackTrace);
            }

        }

        public static async void ParseAndWriteAllBalancesToCsv(HttpResponseMessage response)
        {
            try
            {

                var balances = JsonSerializer.Deserialize<RootobjectAllBalances>(await response.Content.ReadAsStringAsync());

                Console.WriteLine("\n####\n");
                using (StreamWriter writer = new("allBalances.csv"))
                {
                    writer.WriteLine($"Name,Free,Spot Borrow,Total,USD Value,Available Without Borrow");
                    if (balances.result is null)
                    {
                        Console.WriteLine("No AllBalances to write.");
                        return;
                    }
                    foreach (var item in balances.result.main)
                    {
                        if (item.total != 0)
                        {
                            string output = "";
                            foreach (var itemB in DictionaryFromType(item))
                            {
                                output += itemB.Value + ",";
                            }
                            writer.WriteLine(output);

                            Console.WriteLine(output);
                        }
                    }
                }
                Console.WriteLine("\n####\n" + await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Threw exception in ParseAndWriteBalancesToCsv: " + ex.StackTrace);
            }

        }


        public static async void ParseAndWriteLeveragedBalancesToCsv(HttpResponseMessage response)
        {
            try
            {

                var balances = JsonSerializer.Deserialize<RootobjectLeveragedBalance>(await response.Content.ReadAsStringAsync());

                Console.WriteLine("\n####\n");
                using (StreamWriter writer = new("LeveragedBalances.csv"))
                {
                    writer.WriteLine($"Token,Balance");
                    if (balances.result is null)
                    {
                        Console.WriteLine("No leveraged balances to write.");
                        return;
                    }
                    foreach (var item in balances.result)
                    {
                        if (item.balance != 0)
                        {
                            string output = "";
                            foreach (var itemB in DictionaryFromType(item))
                            {
                                output += itemB.Value + ",";
                            }
                            writer.WriteLine(output);

                            Console.WriteLine(output);
                        }
                    }
                }
                Console.WriteLine("\n####\n" + await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Threw exception in ParseAndWriteLeveragedBalancesToCsv: " + ex.StackTrace);
            }

        }


        public static Dictionary<string, object> DictionaryFromType(object atype)
        {
            if (atype == null) return new Dictionary<string, object>();
            Type t = atype.GetType();
            PropertyInfo[] props = t.GetProperties();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (PropertyInfo prp in props)
            {
                object value = prp.GetValue(atype, new object[] { });
                dict.Add(prp.Name, value);
            }
            return dict;
        }

        static HttpRequestMessage Authenticate(HttpRequestMessage request, string endpoint)
        {
            try
            {
                var _nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var hashMaker = new HMACSHA256(Encoding.UTF8.GetBytes($"{YOUR_API_SECRET}"));
                var signaturePayload = $"{_nonce}{METHOD.ToString().ToUpper()}{endpoint}";
                var hash = hashMaker.ComputeHash(Encoding.UTF8.GetBytes(signaturePayload));
                var hashString = BitConverter.ToString(hash).Replace("-", string.Empty);
                var signature = hashString.ToLower();

                request.Headers.Add("FTX-KEY", YOUR_API_KEY);
                request.Headers.Add("FTX-SIGN", signature);
                request.Headers.Add("FTX-TS", _nonce.ToString());

                return request;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Threw exception in Authenticate: " + ex);
                throw;
            }
        }
    }
}
