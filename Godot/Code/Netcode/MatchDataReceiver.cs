using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public partial class MatchDataReceiver : HttpRequest
    {
        public async override void _Ready()
        {
            //await GetAsync( new System.Net.Http.HttpClient());
            return;
            RequestCompleted += onRequestCompleted;
            var error = Request("http://localhost:8080/users");

            if (error != Error.Ok)
            {
                throw new Exception("Error requesting data.");
            }
        }

        void onRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
        {
            //GD.Print(result);
            App.Log(responseCode);
            //GD.Print(headers);
            //GD.Print(body);

            var json = new Json();

            json.Parse(body.GetStringFromUtf8());

            var response = json.Data as dynamic;

            //GD.Print(json);
            App.Log(response);

            foreach (var item in response) 
            {
                App.Log(item.name);
            }
        }

        //static async Task GetAsync(System.Net.Http.HttpClient httpClient)
        //{
        //    var users = await httpClient.GetFromJsonAsync<List<User>>("http://localhost:8080/users");

        //    foreach(var user in users) 
        //    {
        //        App.Log($"{}"user.Name, " is a ", user.Sex);
        //    }
        //}
    }
}

public record class User(
    string? Id    = null,
    string? Email = null,
    string? Name  = null,
    char?   Sex   = null
);