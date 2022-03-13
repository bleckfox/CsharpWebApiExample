using Microsoft.AspNetCore.Mvc;
using System.Net;
using Newtonsoft.Json;
using Tele2Task.Models;

namespace Tele2Task.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CitizensController : ControllerBase
    {
        private readonly ILogger<CitizensController> _logger;

        public CitizensController(ILogger<CitizensController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetCitizens")]
        public static IEnumerable<Citizens> Get()
        {
            string mainUrl = "http://testlodtask20172.azurewebsites.net/task";
            string citizenUrl = "http://testlodtask20172.azurewebsites.net/task/";
            List<Citizens> citizensList = new List<Citizens>();

            // Получаем данные в формате json
            var citizensJson = GetJson(mainUrl);
            // Десереализация json в объект
            var citizensData = JsonConvert.DeserializeObject<IList<Citizens>>(citizensJson);
            if (citizensData != null)
            {
                // Заполняем список горожан
                foreach (var citizens in citizensData)
                {
                    // Получаем данные о возрасте
                    var citizenDataJson = GetJson(citizenUrl + citizens.id);
                    var citizen = JsonConvert.DeserializeObject<Citizen>(citizenDataJson);

                    citizensList.Add(new() { id = citizens.id, name = citizens.name, sex = citizens.sex, age = citizen.age });
                }
            }
            return citizensList;
        }

        // Функция получения данных json
        static string GetJson(string url)
        {
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();
            return data;
        }
    }
}
