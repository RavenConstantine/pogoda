using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace pogoda
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Введи название города: ");
            string gorod = Console.ReadLine();
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://api.openweathermap.org/data/2.5/weather?q="+gorod+"&appid=" + "c3c398126843eb80375dd3c0dc29408f" + "&lang=ru&units=metric");
            HttpResponseMessage response2 = await client.GetAsync("https://api.openweathermap.org/data/2.5/forecast?q="+gorod+"&appid=" + "c3c398126843eb80375dd3c0dc29408f" + "&lang=ru&units=metric");
            if (response.IsSuccessStatusCode && response2.IsSuccessStatusCode)
            {
                Dictionary<string, string> dict = JsonParser(await response.Content.ReadAsStringAsync());
                Console.WriteLine("Прогноз погоды на " + DateTime.Now + " для города " + dict["name"] + ":");
                Console.WriteLine("Текущая температура " + dict["main.temp"] + "°, " + dict["weather.description"] + ", ощущается как " + dict["main.feels_like"] + "°");
                Console.WriteLine("Скорость ветра " + dict["wind.speed"] + " м/с, " + WindDeg(int.Parse(dict["wind.deg"]))+ ", влажность " + dict["main.humidity"] + "%, давление " + dict["main.pressure"] + " мм рт. ст.\n");

                Dictionary<string, string> dict2 = JsonParser(await response2.Content.ReadAsStringAsync());
                Console.WriteLine($"Прогноз погоды на промежуток времени с {DateTime.Now.ToString("d")} до {DateTime.Now.AddDays(3).ToString("d")} включительно для города {dict2["city.name"]}:");
                for (int i = 0; i <= 24; i+=8)
                Console.WriteLine($"{DateTime.Parse(dict2["list[" + i + "].dt_txt"]).ToString("D")}: минимальная температура {dict2["list[" + i + "].main.temp_min"]}°, максимальная температура {dict2["list[" + i + "].main.temp_max"]}°, {dict2["list[" + i + "].weather.description"]}");
            }
            else
                Console.WriteLine("Неправильный город");
            Console.ReadLine();
        }
        static string WindDeg(int deg)
        {
            if (deg > 345 || deg <= 15)
                return "С";
            else if (deg > 15 && deg <= 75)
                return "СВ";
            else if(deg > 75 && deg <= 105)
                return "В";
            else if(deg > 105 && deg <= 165)
                return "ЮВ";
            else if(deg > 165 && deg <= 195)
                return "Ю";
            else if(deg > 195 && deg <= 255)
                return "ЮЗ";
            else if(deg > 255 && deg <= 285)
                return "З";
            else
                return "СЗ";
        }
        static Dictionary<string, string> JsonParser(string json)
        {
            if (json[0] != '[')
                json = "[\n" + json + "\n]";
            var objects = JArray.Parse(json);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (JObject obj in objects)
            {
                foreach (KeyValuePair<String, JToken> pair in obj)
                {
                    if (pair.Value.Count() > 0)
                    {
                        Dictionary<string, string> miniopenWith = JsonParser(pair.Value.ToString());
                        foreach (string s in miniopenWith.Keys)
                        {
                            if (objects.Count > 1)
                                dict.Add(obj.Path + "." + pair.Key + "." + s, miniopenWith[s]);
                            else if (s[0] == '[')
                                dict.Add(pair.Key + s, miniopenWith[s]);
                            else
                                dict.Add(pair.Key + "." + s, miniopenWith[s]);
                        }
                    }
                    else
                    {
                        if (objects.Count > 1)
                            dict.Add(obj.Path + "." + pair.Key, pair.Value.ToString());
                        else
                            dict.Add(pair.Key, pair.Value.ToString());
                    }
                }
            }
            return dict;
        }
    }
}
