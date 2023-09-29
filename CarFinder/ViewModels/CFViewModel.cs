using shared_data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;

namespace CarFinder.ViewModels
{
    internal class CFViewModel : INotifyPropertyChanged
    {
        private readonly int port = 8080;

        private readonly string address = "127.0.0.1";

        private string photoUrl = string.Empty;

        private string requestCount = string.Empty;

        private readonly IPEndPoint ipPoint;
       
        private List<Comment>? carComments;
       
        private async void find()
        {
            //string number = "CB9868AX";
           
            try
            {
                ClientData? response = null;
                using (TcpClient client = new())
                {
                    client.Connect(ipPoint);
                    using StreamWriter writer = new (client.GetStream());
                    string? json =  JsonSerializer.Serialize( new ServerData() { CarFindString = FindString, IsCarNumber = !IsVIN });
                    await writer.WriteLineAsync(json);
                    await writer.FlushAsync();
                    using StreamReader reader = new(client.GetStream());
                    json = await reader.ReadLineAsync();
                    response =  JsonSerializer.Deserialize<ClientData>(json);
                }
                if (response != null)
                {
                    if (response.Status == HttpStatusCode.OK)
                    {
                        CarInfo info = JsonSerializer.Deserialize<CarInfo>(response.Content) ?? new();
                        Elements["Виробник"].Value = info.vendor;
                        Elements["Область"].Value = info.region.name_ua;
                        Elements["Номер"].Value = info.digits;
                        Elements["VIN"].Value = info.vin ?? "VIN доступний для авто з регістрацією після 2021";
                        Elements["Зареестрована на компанію"].Value = info.operations[^1].is_registered_to_company ? "Tak" : "Hi";
                        Elements["Колір"].Value = info.operations[^1].color.ua;
                        Elements["Модель"].Value = info.model;
                        Elements["Рік моделі"].Value = info.model_year.ToString();
                        Elements["Новий код"].Value = info.region.new_code;
                        Elements["Старий код"].Value = info.region.old_code;
                        Elements["У розшуку"].Value = info.is_stolen ? "Tak" : "Hi";
                        Elements["Тип"].Value = info.operations[^1].kind.ua;
                        Elements["Адресca"].Value = info.operations[^1].address;
                        Elements["Відділ"].Value = info.operations[^1].department;
                        Elements["Реєстрація"].Value = info.operations[^1].registered_at;
                        Elements["Остання операція"].Value = info.operations[^1].operation.ua;
                        Elements["Група операції"].Value = info.operations[^1].operation_group.ua;
                        PhotoUrl = info.photo_url;
                        RequestCount = response.RequestCount;
                        carComments = info.comments;
                        OnPropertyChanged(nameof(CarComments));
                    }
                    else
                    {
                        string message = response?.Status switch
                        {
                            HttpStatusCode.NotFound =>"За вишим запитом інформації не знайдено",
                            HttpStatusCode.Unauthorized => " Сервер не авторизований...Не вірний ключ ...",
                            _ => $" Повідомлення сервера \"{response?.Status}\""
                        };
                        MessageBox.Show(message);
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        public string PhotoUrl
        {
            get => photoUrl;
            set
            {
                photoUrl = value;
                OnPropertyChanged();
            }
        }

        public string RequestCount 
        {
            get => requestCount;
            set
            {
                requestCount = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Comment>? CarComments => carComments;

        public Dictionary<string,InfoViewElement> Elements { get; set; }

        public bool IsVIN { get; set; }

        public string FindString { get; set; } = string.Empty;

        public CFViewModel()
        {
            Elements = new()
            {
                { "Номер", new() },
                { "VIN", new() },
                { "Виробник", new() },
                { "Модель", new() },
                { "Рік моделі", new() },
                { "Колір", new() },
                { "Тип", new() },
                { "Область", new() },
                { "Старий код", new() },
                { "Новий код", new() },
                { "У розшуку", new() },
                { "Зареестрована на компанію", new() },
                { "Адресca", new() },
                { "Відділ", new() },
                { "Реєстрація", new() },
                { "Остання операція", new() },
                { "Група операції", new() },
            };

            ipPoint = new(IPAddress.Parse(address), port);
            
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RelayCommand Exit => new((o) => Environment.Exit(0));

        public RelayCommand Find => new((o) => find(), (o) => (IsVIN && FindString.Trim().Length == 17) || ( !IsVIN && FindString.Trim().Length == 8));

        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
