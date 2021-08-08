using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Movies.Client.Models;
using System.Xml.Serialization;
using System.IO;

namespace Movies.Client.Services
{
    public class CRUDService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();

		public CRUDService()
		{
			_httpClient.BaseAddress = new Uri("http://localhost:5000");
			_httpClient.Timeout = new TimeSpan(0, 0, 30);
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 0.4));
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.6));
		}
		public async Task Run()
        {
            //await DeleteResource(Guid.Parse("5b1c2b4d-48c7-402a-80c3-cc796ad49c6b"));

		}

        public async Task<IEnumerable<Movie>> GetResource()
		{
			//var response = await _httpClient.GetAsync("api/movies");
			var response = await GetResourseThroughHttpRequestMessage();
			response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
			var movies = new List<Movie>();
			if (response.Content.Headers.ContentType.MediaType == "application/xml")
			{
				var serializer = new XmlSerializer(typeof(List<Movie>));
				return (List<Movie>)serializer.Deserialize(new StringReader(content));
			}
			else
				return JsonConvert.DeserializeObject<List<Movie>>(content);
		}

		public async Task CreateResource()
		{
			var movie = new MovieForCreation
			{
				Title = "K-19",
				DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
				ReleaseDate = new DateTimeOffset(new DateTime(1999, 2, 12)),
				Description = "About soviet submarine accident",
				Genre = "Drama"
			};
			var response = await PostResourceThroughHttpRequestMessage(movie);
		}

		public async Task UpdateResource()
		{
			var movies = await GetResource();
			var movie = movies.SingleOrDefault(m => m.Title == "K-19");
			if (movie != null && movie.Id != null)
			{
				// Change director to James Cameron
				var movieForUpdate = new MovieForUpdate
				{
					Title = movie.Title,
					DirectorId = Guid.Parse("7A2FBC72-BB33-49DE-BD23-C78FCEB367FC"),
					ReleaseDate = movie.ReleaseDate.GetValueOrDefault(),
					Description = movie.Description,
					Genre = movie.Genre
				};
				var response = await PutResourceThroughHttpRequestMessage(movie.Id.Value, movieForUpdate);
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				var getTheSubmittedModelBack = JsonConvert.DeserializeObject<Movie>(content);
			}
		}

		public async Task DeleteResource(Guid id)
		{
			var content = await DeleteResourseThroughHttpRequestMessage(id);
		}

		private async Task<HttpResponseMessage> GetResourseThroughHttpRequestMessage()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
			// DefaultRequestHeaders set on httpClient will be ignored.
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			return await _httpClient.SendAsync(request);
		}

		private async Task<HttpResponseMessage> PostResourceThroughHttpRequestMessage(MovieForCreation movie)
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "api/movies");
			return await SendRequestWithStringContent(request, movie);
		}

		private async Task<HttpResponseMessage> PutResourceThroughHttpRequestMessage(Guid id, MovieForUpdate movie)
		{
			var request = new HttpRequestMessage(HttpMethod.Put, $"api/movies/{id}");
			return await SendRequestWithStringContent(request, movie);
		}

		private async Task<HttpResponseMessage> SendRequestWithStringContent(HttpRequestMessage request, object model)
		{
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Content = new StringContent(JsonConvert.SerializeObject(model));
			request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

			return await _httpClient.SendAsync(request);
		}

		private async Task<string> DeleteResourseThroughHttpRequestMessage(Guid id)
		{
			var request = new HttpRequestMessage(HttpMethod.Delete, $"api/movies/{id}");
			// DefaultRequestHeaders set on httpClient will be ignored.
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();
			var content = await response.Content.ReadAsStringAsync();
			if (response.StatusCode != HttpStatusCode.NoContent)
				return content;
			else
				return string.Empty;
		}
	}
}
