﻿using dockerapi.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace dockerapi.Services
{
    public class DockerApiService
    {
        private HttpClientHandler handler;
        private HttpClient client;

        public DockerApiService()
        {
            handler = new HttpClientHandler();
            handler.Proxy = null;
            handler.UseProxy = false;
            handler.UseCookies = false;

            client = new HttpClient(handler);
        }

        public async Task<bool> TestCredentials(string registry, string authString)
        {
            try
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get,
                    new Uri(new Uri("https://" + registry), "/v2/"));

                message.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);

                var resp = await client.SendAsync(message);
                return resp.StatusCode != HttpStatusCode.Unauthorized;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to read the tags for a repository with the given credentials.
        /// If authentication failed, returns null.
        /// If any other error occured, throws an exception.
        /// </summary>
        public async Task<Tuple<string, HttpStatusCode, string>> ListTags(RegistryCredential cred, string repoName, string queryString)
        {
            try
            {
                string endpoint = $"/v2/{repoName}/tags/list{queryString}";
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get,
                    new Uri(new Uri("https://" + cred.Registry), endpoint));

                message.Headers.Authorization = new AuthenticationHeaderValue("Basic", cred.BasicAuth);

                var resp = await client.SendAsync(message);

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return null;
                }

                return Tuple.Create(await resp.Content.ReadAsStringAsync(), resp.StatusCode,
                    resp.Headers.Contains("Link") ? resp.Headers.GetValues("Link").First() : null);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>
        /// Tries to read the registry catalog with the given credentials.
        /// If authentication failed, returns null.
        /// If any other error occured, throws an exception.
        /// </summary>
        /// <returns>A tuple containing the result, and the HTTP Link header.</returns>
        public async Task<Tuple<string, HttpStatusCode, string>> Catalog(RegistryCredential cred, string queryString)
        {
            try
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get,
                    new Uri(new Uri("https://" + cred.Registry), "/v2/_catalog" + queryString));

                message.Headers.Authorization = new AuthenticationHeaderValue("Basic", cred.BasicAuth);

                var resp = await client.SendAsync(message);

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return null;
                }

                return Tuple.Create(await resp.Content.ReadAsStringAsync(), resp.StatusCode,
                    resp.Headers.Contains("Link") ? resp.Headers.GetValues("Link").First() : null);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a manifest.
        /// </summary>
        public async Task<Tuple<string, HttpStatusCode>> Manifest(RegistryCredential cred, string repo, string tag)
        {
            try
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get,
                    new Uri(new Uri("https://" + cred.Registry), $"/v2/{repo}/manifests/{tag}"));

                message.Headers.Authorization = new AuthenticationHeaderValue("Basic", cred.BasicAuth);
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(
                    "application/vnd.docker.distribution.manifest.v1+json", 0.5));
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(
                    "application/vnd.docker.distribution.manifest.v2+json", 0.6));

                var resp = await client.SendAsync(message);

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return null;
                }

                return Tuple.Create(await resp.Content.ReadAsStringAsync(), resp.StatusCode);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
