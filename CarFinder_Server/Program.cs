using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using shared_data;

namespace CarFinder_Server
{
    internal class Program
    {
        private static readonly int port = 8080;

        private static readonly string address = "127.0.0.1";

        private static readonly string url = "https://baza-gai.com.ua";

        private static readonly string key = "0d441e4b96bcb7ac4271120b69df6a96";

        static void Main(string[] args)
        {
            IPEndPoint ipPoint = new(IPAddress.Parse(address), port);
            TcpListener listener = new(ipPoint);
            listener.Start(10);
           
            try
            {
                while (true)
                {
                    Console.WriteLine("Car finder server started! Waiting for connection...");
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine($"Client {client.Client.RemoteEndPoint} connected");
                    Task.Run(() => { ListenClient(client); });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                listener.Stop();
            }
            
        }

        static async void ListenClient(TcpClient client)
        {
            StreamReader? reader = null;
            StreamWriter? writer = null;
            try
            {
                reader = new(client.GetStream());
                string? json = await reader.ReadLineAsync();
                ServerData? sdata =  JsonSerializer.Deserialize<ServerData>(json);
               
                Console.WriteLine($"Resived to find : \"{sdata?.CarFindString}\"  from {client.Client.RemoteEndPoint}");
                ClientData data = new();
                string selector = sdata.IsCarNumber ? @"/nomer/" : @"/vin/";
                using HttpRequestMessage request = new(HttpMethod.Get, url + selector + sdata?.CarFindString);
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("X-Api-Key", key);
                using HttpClient httpClient = new();
                using HttpResponseMessage response = await httpClient.SendAsync(request);
                data.Status = response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    data.Content = await response.Content.ReadAsStringAsync();
                    data.RequestCount = response.Headers.FirstOrDefault(x => x.Key == "X-RateLimit-Remaining").Value.ElementAt(0);
                }
                json = JsonSerializer.Serialize(data);
                writer = new(client.GetStream());
                await writer.WriteLineAsync(json);
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine($"Client {client.Client.RemoteEndPoint} disconnected");
                writer?.Close();
                reader?.Close();
                client.Close();
                writer?.Dispose();
                reader?.Dispose();
                client.Dispose();
            }
        }
    }
}