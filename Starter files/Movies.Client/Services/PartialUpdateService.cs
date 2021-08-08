using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Movies.Client.Models;
using Newtonsoft.Json;

namespace Movies.Client.Services
{
	public class PartialUpdateService : IIntegrationService
	{
		private static HttpClient _httpClient = new HttpClient();
		private readonly CRUDService _crudService;

		public PartialUpdateService(CRUDService crudService)
		{
			_httpClient.BaseAddress = new Uri("http://localhost:5000");
			_httpClient.Timeout = new TimeSpan(0, 0, 30);
			_httpClient.DefaultRequestHeaders.Clear();
			_crudService = crudService;
		}

		public async Task Run()
		{
			await PatchResource();
		}

		public async Task PatchResource()
		{
			var patchDoc = new JsonPatchDocument<MovieForUpdate>();
			// Update description for "The Usual Suspects" movie.
			patchDoc.Replace(m => m.Description, "The Usual Suspects wiped description");
			var movies = await _crudService.GetResource();
			var movie = movies.SingleOrDefault(m => m.Title == "The Usual Suspects");
			if (movie != null && movie.Id != null)
			{
				var request = new HttpRequestMessage(HttpMethod.Patch, $"api/movies/{movie.Id}");
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				request.Content = new StringContent(JsonConvert.SerializeObject(patchDoc));
				request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json-path+json");
				var response = await _httpClient.SendAsync(request);
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				var updatedMovie = JsonConvert.DeserializeObject<Movie>(content);
			}
		}
	}
}
