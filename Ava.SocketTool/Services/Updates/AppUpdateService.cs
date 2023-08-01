﻿using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Ava.SocketTool.Services.Updates.Model;

namespace Ava.SocketTool.Services.Updates;

public sealed class AppUpdateService
{
    public AppUpdateService()
    {
        CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
    }

    public Version CurrentVersion { get; }

    public bool IsUpdateAvailable { get; private set; }

    public Version? LatestVersion { get; private set; }

    public void EnableUpdateChecks()
    {
        Task.Run(DoUpdateChecks);
    }

    async Task DoUpdateChecks()
    {
        using (var httpClient = new HttpClient())
        {
            while (true)
            {
                try
                {
                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri("https://api.github.com/repos/wmchuang/Ava.SocketTool/releases")
                    };

                    request.Headers.UserAgent.ParseAdd("Ava.SocketTool");

                    var response = await httpClient.SendAsync(request).ConfigureAwait(false);
                    var releasesJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var releases = JsonSerializer.Deserialize<Releases>(releasesJson);
                    var latestReleaseName = releases?.First().Name ?? string.Empty;

                    // Remove the "v" from the version from GitHub. Then it can be safely parsed.
                    LatestVersion = Version.Parse(latestReleaseName.TrimStart('v'));

                    IsUpdateAvailable = LatestVersion > CurrentVersion;
                }
                catch
                {
                    // Do nothing for now.
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }
        }
    }
}