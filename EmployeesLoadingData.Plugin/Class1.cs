using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using PhoneApp.Domain;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;

namespace EmployeesLoaderPlugin
{
    [Author(Name = "Yulia")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static HttpClient httpClient = new HttpClient();
        private List<DataTransferObject> employeesList = new List<DataTransferObject>();

        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Загрузка сотрудников из API");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var response = httpClient.GetAsync("https://dummyjson.com/users").Result;
            logger.Info("Данные успешно загружены");

            var jsonString = response.Content.ReadAsStringAsync().Result;

            var userResponse = JsonConvert.DeserializeObject<UserResponse>(jsonString);
            if (userResponse?.Users != null)
            {
                foreach (var user in userResponse.Users)
                {
                    var employee = new EmployeesDTO
                    {
                        Name = user.FirstName
                    };
                    employee.AddPhone(user.Phone);

                    employeesList.Add(employee);
                }


                return employeesList.Cast<DataTransferObject>();
            }
            else
            {
                logger.Warn("Пользователи не найдены в ответе.");
                return Enumerable.Empty<DataTransferObject>();
            }
        }

        public class UserResponse
        {
            public List<User> Users { get; set; }
        }

        public class User
        {
            public string FirstName { get; set; }
            public string Phone { get; set; }

        }
    }
}
