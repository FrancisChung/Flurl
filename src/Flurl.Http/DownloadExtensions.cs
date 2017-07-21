﻿using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

#if !NETSTANDARD1_1
namespace Flurl.Http
{
	/// <summary>
	/// Download extensions for the Flurl Client.
	/// </summary>
	public static class DownloadExtensions
	{
        /// <summary>
        /// Asynchronously downloads a file at the specified URL.
        /// </summary>
        /// <param name="client">The flurl client.</param>
        /// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
        /// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
        /// <returns>A Task whose result is the local path of the downloaded file.</returns>
        public static async Task<string> DownloadFileAsync(this IFlurlClient client, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			if (localFileName == null)
				localFileName = client.Url.Path.Split('/').Last();

			// need to temporarily disable autodispose if set, otherwise reading from stream will fail
			var autoDispose = client.Settings.AutoDispose;
			client.Settings.AutoDispose = false;

			try {
				var response = await client.SendAsync(HttpMethod.Get, completionOption: HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

				// http://codereview.stackexchange.com/a/18679
				using (var httpStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
				using (var filestream = await FileUtil.OpenWriteAsync(localFolderPath, localFileName, bufferSize).ConfigureAwait(false)) {
					await httpStream.CopyToAsync(filestream, bufferSize).ConfigureAwait(false);
				}

				return FileUtil.CombinePath(localFolderPath, localFileName);
			}
			finally {
				client.Settings.AutoDispose = autoDispose;
				if (client.Settings.AutoDispose)
					client.Dispose();
			}
		}

        /// <summary>
        /// Asynchronously downloads a file at the specified URL.
        /// </summary>
        /// <param name="url">The Url.</param>
        /// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
        /// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
        /// <returns>A Task whose result is the local path of the downloaded file.</returns>
        public static Task<string> DownloadFileAsync(this string url, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			return new FlurlClient(url, true).DownloadFileAsync(localFolderPath, localFileName, bufferSize);
		}

		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="url">The Url.</param>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static Task<string> DownloadFileAsync(this Url url, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			return new FlurlClient(url, true).DownloadFileAsync(localFolderPath, localFileName, bufferSize);
		}
	}
}
#endif