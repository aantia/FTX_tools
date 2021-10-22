﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Position_Fetcher { 

    class Endpoint
    {
        public static readonly string BALANCE = "wallet/balances";
        public static readonly string POSITION = "positions";
    }

    class Program
    {
        static string YOUR_API_SECRET = "";
        static string YOUR_API_KEY = "";
        readonly static string FTX = "https://ftx.com";
        readonly static HttpMethod METHOD = HttpMethod.Get;

        static void Main(string[] args)
        {

            InitialiseConfig();

            using (HttpClient client = new())
            {
                client.BaseAddress = new(FTX);

                while (true)
                {
                    FetchData(client, Endpoint.POSITION);
                    FetchData(client, Endpoint.BALANCE);

                    Thread.Sleep(10 * 1000);
                }
            }

        }

        static void FetchData(HttpClient client, string objective)
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
        }

        private static void InitialiseConfig()
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

        public static async void ParseAndWritePositionsToCsv(HttpResponseMessage response)
        {
            var positions = JsonSerializer.Deserialize<RootobjectPosition>(await response.Content.ReadAsStringAsync());

            Console.WriteLine("\n####\n");
            using (StreamWriter writer = new("positions.csv"))
            {
                writer.WriteLine($"Name,Size,Side,Net Size,Long Order Size,Short Order Size,Cost,Entry Price,Unrealised PNL,Realised PNL,Initial Margin Requirement,Maintenance Margin Requirement,Open Size,Collateral Used,Estimated Liquidation Price");
                foreach (var item in positions.result)
                {
                    if (item.size > 0)
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

        public static async void ParseAndWriteBalancesToCsv(HttpResponseMessage response)
        {
            var balances = JsonSerializer.Deserialize<RootobjectBalance>(await response.Content.ReadAsStringAsync());

            Console.WriteLine("\n####\n");
            using (StreamWriter writer = new("balances.csv"))
            {
                writer.WriteLine($"Name,Free,Spot Borrow,Total,USD Value,Available Without Borrow");
                foreach (var item in balances.result)
                {
                    if (item.total > 0)
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
    }
}