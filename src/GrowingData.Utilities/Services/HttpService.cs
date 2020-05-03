using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GrowingData.Utilities {

	/// <summary>
	/// Transient Service for managing HttpRequests
	/// </summary>
	public class HttpService {
		private IHttpClientFactory _factory;
		private Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();
		private List<Action<HttpRequestMessage>> _preflight;

		public HttpService AddHeader(string header, string value) {
			_defaultHeaders[header] = value;
			return this;
		}

		public HttpService(IHttpClientFactory factory) {
			_factory = factory;
			_preflight = new List<Action<HttpRequestMessage>>();
		}

		public HttpService WithPreflight(Action<HttpRequestMessage> preflight) {
			_preflight.Add(preflight);
			return this;
		}

		public async Task Get(Uri url, Stream destination) {
			await Send(HttpMethod.Get, url, null, _defaultHeaders, destination);
		}

		public async Task<T> GetJson<T>(Uri url) {


			var json = await GetString(url);
			if (json.Trim().StartsWith("<")) {
				throw new Exception($"Unknown response from: {url} (probably not authorized?)");
			}

			return JsonConvert.DeserializeObject<T>(json);

		}
		public async Task<string> GetString(Uri url) {
			using (var memoryStream = new MemoryStream()) {
				await Send(HttpMethod.Get, url, null, _defaultHeaders, memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);
				using (var streamReader = new StreamReader(memoryStream)) {
					var json = streamReader.ReadToEnd();
					return json;
				}
			}

		}

		public async Task Post(Uri url, HttpContent content, Stream destination) {
			await Send(HttpMethod.Post, url, content, _defaultHeaders, destination);
		}

		public async Task<T> PostJson<T>(Uri url, HttpContent content) {
			var responseString = await PostString(url, content);
			return JsonConvert.DeserializeObject<T>(responseString);

		}
		public async Task<string> PostString(Uri url, HttpContent content) {
			using (var memoryStream = new MemoryStream()) {
				await Send(HttpMethod.Post, url, content, _defaultHeaders, memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);
				using (var streamReader = new StreamReader(memoryStream)) {
					var responseString = streamReader.ReadToEnd();
					return responseString;
				}
			}

		}

		public async Task Send(HttpMethod method, Uri url, HttpContent content, Dictionary<string, string> headers, Stream destination) {
			var client = _factory.CreateClient();

			// Build up a sweet request
			var message = new HttpRequestMessage(method, url);
			foreach (var h in headers) {
				message.Headers.Add(h.Key, h.Value);
			}

			// Execute our pre-flight actions so that people can change headers, add Authorization etc.
			foreach (var pre in _preflight) {
				pre(message);
			}

			message.Content = content;

			using (var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead)) {

				if (!response.IsSuccessStatusCode) {
					throw new Exception($"HttpRequest Failed: {method}: {url} {response.ReasonPhrase} ({response.StatusCode})");
				}
				using (var streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
					await streamToReadFrom.CopyToAsync(destination);
				}

			}

		}
	}
}
