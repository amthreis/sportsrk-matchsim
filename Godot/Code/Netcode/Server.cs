using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Newtonsoft.Json.Serialization;
using System.IO;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using StackExchange.Redis;
using System.Runtime.CompilerServices;

namespace Futzim
{
    public partial class Server : Node
    {
        const int OwnPort = 8585;
        const int MainPort = 5757;
        const int RedisPort = 6379;

        private static string RedisConnectionString => $"{redisHost}:{ RedisPort }";
        private static ConnectionMultiplexer connection;
        private const string Channel = "queue";

        HttpListener listener = new HttpListener();
        System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

        MatchResolver resolver = new MatchResolver();

        static string redisHost = "localhost";
        static string prefixHost = "localhost";
        static string mainHost = "localhost";

        public override void _Ready()
        {
            GD.Print("DOCKER = ", OS.HasEnvironment("DOCKER"));
            Console.WriteLine($"<ln> DOCKER = {OS.HasEnvironment("DOCKER")}");

            if (OS.HasEnvironment("DOCKER"))
            {
                redisHost = "db-redis";
                prefixHost = "*";
                mainHost = "main";
            }

            resolver.Done += OnDoneResolving;

            listener = new HttpListener();

            listener.Prefixes.Add($"http://{prefixHost}:" + OwnPort.ToString() + "/");
            listener.Start();

            //var pubsub = connection.GetSubscriber();
            //pubsub.Subscribe(new RedisChannel(Channel, RedisChannel.PatternMode.Auto), (channel, message) => GD.Print($"Message received from { channel } : " + message));
            

            App.Log($"Listening on port { OwnPort }...");
            Receive();

        }

        async Task<bool> SendResolve(string jsonObj)
        {
            var httpClient = new System.Net.Http.HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, $"http://{mainHost}:5757/common/football/matchsim-resolve");
            //req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            req.Content = new StringContent(jsonObj, Encoding.UTF8, "application/json"); ;

            //GD.Print("Prepare to send");
            var res = await httpClient.SendAsync(req);

            App.Log($"Sent resolved match... Code from server: {(int)res.StatusCode} ({res.StatusCode})");
            //GD.Print(res.StatusCode);

            var content = await res.Content.ReadAsStringAsync();
            App.Log(content);

            return res.StatusCode == HttpStatusCode.OK;
        }

        void OnDoneResolving(List<Match> matches)
        {
            App.Log($"------------- All the { matches.Count } matches are over!");

            foreach(var m in matches)
            {
                App.Log($"({m.Id}) Black {m.Home.Stats.Goals}x{m.Away.Stats.Goals} White");
            }

            var matchesOTD = new MatchesOTD(matches);


            var json = Serialized(matchesOTD.Matches);

            App.Log("=================================================");
            App.Log(json);
            //DisplayServer.ClipboardSet(json);

            SendResolve(json);
        }

        public void Stop()
        {
            listener.Stop();
        }

        void Receive()
        {
            listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
            //GD.Print("Received");
        }

        public static T Deserialized<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            Converters = { new StringEnumConverter() },
                            TypeNameHandling = TypeNameHandling.Auto,
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                        });
        }

        public static string Serialized(object o, Action callback = null)
        {
            var data = JsonConvert.SerializeObject(o, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Converters = { new StringEnumConverter() },
                    TypeNameHandling = TypeNameHandling.Auto,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            callback?.Invoke();

            return data;
        }

        List<Match> matchesToResolve;

        void WorkOLD()
        {
            App.Log($"MatchesToResolve: { matchesToResolve.Count}");
            resolver.Play(matchesToResolve);
        }

        bool isWorking = false;

        void Work()
        {
            if (connection == null)
            {
                connection = ConnectionMultiplexer.Connect(RedisConnectionString);
            }

            isWorking = true;

            Task.Run(TrySimAMatch);
        }

        async void TrySimAMatch()
        {
            App.Log("Try to sim a match...");
            var match = await connection.GetDatabase().ListRightPopAsync("football:matches:tbp");

            if (!match.HasValue)
            {
                App.Log("No more matches to sim.");
                isWorking = false;
                return;
            }

            var dsr = Deserialized<MatchDTO>(match);
            var m = Match.FromDTO(dsr);

            GD.Print($"Match({ dsr.Id })'s avg MMR: ", dsr.Players.Average(p => p.MMR));
            var matchOTD = await Task.Run(() => resolver.PlaySingle(m));


            GD.Print($"Incr of Pl0 = {matchOTD.Players[0].MMRIncr}");

            //var matchesOTD = new MatchesOTD(matches);


            var json = Serialized(matchOTD);

            //GD.Print("=================================================");
            //GD.Print(json);
            //DisplayServer.ClipboardSet(json);

            //GD.Print("Send Resolve");
            var canProceed = await SendResolve(json);

            if (canProceed)
            {
                TrySimAMatch();
            }
            else
            {
                App.Log("Server said it's not OK to start simulating matches. Stop.");
                isWorking = false;
                return;
            }
        }

        void HandleOLD(HttpListenerContext ctx, HttpListenerRequest req)
        {
            var objs = GetRequestBody<MatchDTO[]>(req);

            App.Log($"Formatting MatchDTOs...");

            //App.Log("------- req");
            App.Log(objs);
            //App.Log("-------");

            var matches = new List<Match>();

            foreach (var dto in objs)
            {
                matches.Add(Match.FromDTO(dto));
            }

            matchesToResolve = matches;

            Respond(ctx, HttpStatusCode.OK, new { message = "ok" });

            CallDeferred(nameof(Work));
        }

        void Handle(HttpListenerContext ctx, HttpListenerRequest req)
        {
            var body = GetRequestBody<StartRequest>(req);

            
            Respond(ctx, HttpStatusCode.OK, new { message = "OK" });

            //GD.Print("Message: ", body.Message);

            if (body.Message == "OK")
            {
                App.Log("Just got a message from the main server to start simulating games.");
                CallDeferred(nameof(Work));
            }
            else
            {
                App.Log($"Message: { body.Message}");
            }

            //CallDeferred(nameof(Work));
        }

        void ListenerCallback(IAsyncResult result)
        {
            //GD.Print("Listening: ", listener.IsListening);
            if (listener.IsListening)
            {
                var context = listener.EndGetContext(result);
                var request = context.Request;

                if (request.ContentType != "application/json")
                {
                    App.Log("Request didn't send a json object.");
                }

                App.Log($"got a { request.HttpMethod}");
                if (request.HttpMethod != HttpMethod.Post.ToString())
                {
                    return;
                }

                try
                {
                    Handle(context, request);
                    //GD.Print("re");

                }
                catch (Exception ex)
                {
                    //GD.Print("exc");
                    Respond(context, HttpStatusCode.BadRequest, new { error = ex.Message });
                }

                Receive();
            }

        }

        T GetRequestBody<T>(HttpListenerRequest req)
        {
            string text;
            using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
            {
                text = reader.ReadToEnd();
            }

            App.Log("------ text");
            App.Log(text);
            App.Log("------ ");
            
            return Deserialized<T>(text);
        }

        void Respond(HttpListenerContext ctx, HttpStatusCode code, object? body = null)
        {
            var response = ctx.Response;
            response.StatusCode = (int)code;

            if (body != null)
            {
                string responseString = Serialized(body);

                var buffer = new byte[] { };

                response.ContentType = "application/json";

                buffer = Encoding.ASCII.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;

                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
