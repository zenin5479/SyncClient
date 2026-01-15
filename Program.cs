using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace SyncClient
{
   class Program
   {
      // Базовый URL сервера (укажите свой, если сервер на другом порту/хосте)
      private const string BaseUrl = "http://localhost:8080/";

      static void Main()
      {
         Console.WriteLine("Тестирование HTTP-сервера...\n");
         try
         {
            // 1. GET — получить все записи
            Console.WriteLine("1. GET / (все записи):");
            string allData = SendGet("/");
            Console.WriteLine(allData);
            Console.WriteLine();

            // 2. POST — создать запись
            Console.WriteLine("2. POST /user1 (создание):");
            string postResponse = SendPost("/user1", new { name = "Bob", age = 25 });
            Console.WriteLine(postResponse);
            Console.WriteLine();

            // 3. GET — получить конкретную запись
            Console.WriteLine("3. GET /user1 (проверка создания):");
            string user1 = SendGet("/user1");
            Console.WriteLine(user1);
            Console.WriteLine();

            // 4. PUT — обновить запись
            Console.WriteLine("4. PUT /user1 (обновление):");
            string putResponse = SendPut("/user1", new { name = "Bob Updated", age = 26 });
            Console.WriteLine(putResponse);
            Console.WriteLine();

            // 5. GET — проверить обновление
            Console.WriteLine("5. GET /user1 (после обновления):");
            string updatedUser1 = SendGet("/user1");
            Console.WriteLine(updatedUser1);
            Console.WriteLine();

            // 6. DELETE — удалить запись
            Console.WriteLine("6. DELETE /user1 (удаление):");
            string deleteResponse = SendDelete("/user1");
            Console.WriteLine(deleteResponse);
            Console.WriteLine();

            // 7. GET — проверить удаление
            Console.WriteLine("7. GET /user1 (после удаления):");
            string deletedUser1 = SendGet("/user1");
            Console.WriteLine(deletedUser1);
            Console.WriteLine();

            // 8. GET — снова все записи (должно быть пусто)
            Console.WriteLine("8. GET / (после удаления):");
            string finalData = SendGet("/");
            Console.WriteLine(finalData);
         }
         catch (Exception ex)
         {
            Console.WriteLine("Ошибка: {0}", ex.Message);
         }

         Console.WriteLine("\nТестирование завершено");
      }

      private static string SendGet(string path)
      {
         using (WebClient client = new WebClient())
         {
            try
            {
               var response = client.DownloadString(BaseUrl + path);
               return response;
            }
            catch (WebException webEx)
            {
               using (var errorResponse = webEx.Response as HttpWebResponse)
               {
                  if (errorResponse != null)
                  {
                     return $"Ошибка {errorResponse.StatusCode}: {webEx.Message}";
                  }
               }
               return $"Ошибка: {webEx.Message}";
            }
         }
      }

      private static string SendPost(string path, object data)
      {
         using (var client = new WebClient())
         {
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            var json = JsonConvert.SerializeObject(data);
            try
            {
               var response = client.UploadString(BaseUrl + path, "POST", json);
               return response;
            }
            catch (WebException webEx)
            {
               using (var errorResponse = webEx.Response as HttpWebResponse)
               {
                  if (errorResponse != null)
                  {
                     return $"Ошибка {errorResponse.StatusCode}: {webEx.Message}";
                  }
               }
               return $"Ошибка: {webEx.Message}";
            }
         }
      }

      private static string SendPut(string path, object data)
      {
         using (var client = new WebClient())
         {
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            var json = JsonConvert.SerializeObject(data);
            try
            {
               // WebClient не поддерживает PUT напрямую, используем UploadString с методом
               var response = client.UploadString(BaseUrl + path, "PUT", json);
               return response;
            }
            catch (WebException webEx)
            {
               using (var errorResponse = webEx.Response as HttpWebResponse)
               {
                  if (errorResponse != null)
                  {
                     return $"Ошибка {errorResponse.StatusCode}: {webEx.Message}";
                  }
               }
               return $"Ошибка: {webEx.Message}";
            }
         }
      }

      private static string SendDelete(string path)
      {
         try
         {
            var request = (HttpWebRequest)WebRequest.Create(BaseUrl + path);
            request.Method = "DELETE"; // Прямое задание метода

            using (var response = (HttpWebResponse)request.GetResponse())
            {
               if (response.StatusCode == HttpStatusCode.NoContent ||
                   response.StatusCode == HttpStatusCode.OK)
               {
                  // Если ответ пустой (204) или успешный (200)
                  return "{\"success\": true}";
               }

               // Читаем тело ответа, если оно есть
               using (var stream = response.GetResponseStream())
               using (var reader = new StreamReader(stream))
               {
                  return reader.ReadToEnd();
               }
            }
         }
         catch (WebException webEx)
         {
            using (var errorResponse = webEx.Response as HttpWebResponse)
            {
               if (errorResponse != null)
               {
                  return $"Ошибка {errorResponse.StatusCode}: {webEx.Message}";
               }
            }
            return $"Ошибка: {webEx.Message}";
         }
      }


   }
}
