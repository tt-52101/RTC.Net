﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using unirest_net.request;

namespace unirest_net.http
{
    public class HttpClientHelper
    {
        private const string USER_AGENT = "unirest-net/1.0";

        public static HttpResponse<T> Request<T>(HttpRequest request)
        {
            var responseTask = RequestHelper(request);
            HttpResponseMessage response;
            try
            {
                Task.WaitAll(responseTask);
                response = responseTask.Result;
            }
            catch (Exception ex)
            {
                StringBuilder errmsg = new StringBuilder();
                Exception exnext = ex.InnerException;
                errmsg.Append(ex.Message);
                while (exnext != null)
                {
                    errmsg.Append("->" + exnext.Message);
                    exnext = exnext.InnerException;
                }
                response = new HttpResponseMessage()
                {
                    ReasonPhrase = errmsg.ToString(),
                    StatusCode = HttpStatusCode.RequestTimeout,
                    RequestMessage = new HttpRequestMessage()
                    {
                        Method = request.HttpMethod,
                        RequestUri = request.URL
                    }
                };
            }

            return new HttpResponse<T>(response);
        }

        public static Task<HttpResponse<T>> RequestAsync<T>(HttpRequest request)
        {
            var responseTask = RequestHelper(request);
            return Task<HttpResponse<T>>.Factory.StartNew(() =>
            {
                Task.WaitAll(responseTask);
                return new HttpResponse<T>(responseTask.Result);
            });
        }

        private static Task<HttpResponseMessage> RequestHelper(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("user-agent"))
            {
                request.Headers.Add("user-agent", USER_AGENT);
            }

            var client = new HttpClient();
            client.Timeout = request.TimeOut;
            var msg = new HttpRequestMessage(request.HttpMethod, request.URL);

            foreach (var header in request.Headers)
            {
                msg.Headers.Add(header.Key, header.Value);
            }

            if (request.Body.Any())
            {
                msg.Content = request.Body;
            }

            return client.SendAsync(msg);
        }
    }
}
