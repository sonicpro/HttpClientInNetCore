using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
	public class StreamService : IIntegrationService
	{

		// Added a handler for automatic content decompression
		private static HttpClient _httpClient = new HttpClient(new HttpClientHandler()
		{
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
		});

		public StreamService()
		{
			_httpClient.BaseAddress = new Uri("http://localhost:5000");
			_httpClient.Timeout = new TimeSpan(0, 0, 30);
			_httpClient.DefaultRequestHeaders.Clear();
		}

		public async Task Run()
		{
			//await TestGetPosterWithoutStream();
			//await TestGetPosterWithStream();
			//await TestGetPosterWithStreamAndCompletionMode();
			await GetPosterWithGZipCompression();
		}

		private async Task GetPoster()
		{
			// Get posters for "The Big Lebowski" movie.
			var postersUrl = $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}";
			var request = new HttpRequestMessage(HttpMethod.Get, postersUrl);
			request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			// Use HttpCompletionOption to start reading the content stream as soon as possible.
			var response = await _httpClient.SendAsync(request);
			var content = await response.Content.ReadAsStringAsync();
			var poster = JsonConvert.DeserializeObject<Poster>(content);
		}

		private async Task GetPosterWithStream()
		{
			// Get posters for "The Big Lebowski" movie.
			var postersUrl = $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}";
			var request = new HttpRequestMessage(HttpMethod.Get, postersUrl);
			request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			// Use HttpCompletionOption to start reading the content stream as soon as possible.
			using (var response = await _httpClient.SendAsync(request))
			{
				response.EnsureSuccessStatusCode();
				var stream = await response.Content.ReadAsStreamAsync();
				using (var streamReader = new StreamReader(stream))
				using (var jsonTextReader = new JsonTextReader(streamReader))
				{
					var jsonSerializer = new JsonSerializer();
					var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
				}
			}
		}

		private async Task GetPosterWithStreamAndOptions()
		{
			// Get posters for "The Big Lebowski" movie.
			var postersUrl = $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}";
			var request = new HttpRequestMessage(HttpMethod.Get, postersUrl);
			request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			// Use HttpCompletionOption to start reading the content stream as soon as possible.
			using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
			{
				response.EnsureSuccessStatusCode();
				var stream = await response.Content.ReadAsStreamAsync();
				using (var streamReader = new StreamReader(stream))
				using (var jsonTextReader = new JsonTextReader(streamReader))
				{
					var jsonSerializer = new JsonSerializer();
					var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
				}
			}
		}

		private async Task GetPosterWithGZipCompression()
		{
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			// Add AcceptEncoding header to get gzipped content.
			request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

			using (var response = await _httpClient.SendAsync(request))
			{
				response.EnsureSuccessStatusCode();

				var stream = await response.Content.ReadAsStreamAsync();
				using (var streamReader = new StreamReader(stream))
				using (var jsonTextReader = new JsonTextReader(streamReader))
				{
					var jsonSerializer = new JsonSerializer();
					var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
				}
			}
		}

		private async Task TestGetPosterWithoutStream()
		{
			// warmup
			await GetPoster();

			// start stopwatch 
			var stopWatch = System.Diagnostics.Stopwatch.StartNew();

			// run requests
			for (int i = 0; i < 200; i++)
			{
				await GetPoster();
			}

			// stop stopwatch
			stopWatch.Stop();
			System.Diagnostics.Debug.WriteLine($"Elapsed milliseconds without stream: " +
				$"{stopWatch.ElapsedMilliseconds}, " +
				$"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
		}

		private async Task TestGetPosterWithStream()
		{
			// warmup
			await GetPosterWithStream();

			// start stopwatch 
			var stopWatch = System.Diagnostics.Stopwatch.StartNew();

			// run requests
			for (int i = 0; i < 200; i++)
			{
				await GetPosterWithStream();
			}

			// stop stopwatch
			stopWatch.Stop();
			System.Diagnostics.Debug.WriteLine($"Elapsed milliseconds with stream: " +
				$"{stopWatch.ElapsedMilliseconds}, " +
				$"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
		}


		private async Task TestGetPosterWithStreamAndCompletionMode()
		{
			// warmup
			await GetPosterWithStreamAndOptions();

			// start stopwatch 
			var stopWatch = System.Diagnostics.Stopwatch.StartNew();

			// run requests
			for (int i = 0; i < 200; i++)
			{
				await GetPosterWithStreamAndOptions();
			}

			// stop stopwatch
			stopWatch.Stop();
			System.Diagnostics.Debug.WriteLine($"Elapsed milliseconds with stream and completionmode: " +
				$"{stopWatch.ElapsedMilliseconds}, " +
				$"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
		}
	}
}
