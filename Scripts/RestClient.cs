﻿using LiteNetLibManager;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityRestClient
{
    public class RestClient : MonoBehaviour
    {
        public static bool DoNotCountNextRequest { get; set; }
        public static int PostLoadCount { get; private set; }
        public static int PatchLoadCount { get; private set; }
        public static int PutLoadCount { get; private set; }
        public static int DeleteLoadCount { get; private set; }
        public static int GetLoadCount { get; private set; }
        public static int LoadCount { get { return PostLoadCount + PatchLoadCount + PutLoadCount + DeleteLoadCount + GetLoadCount; } }

        public static string GetQueryString(KeyValuePair<string, object>[] queries)
        {
            StringBuilder queryStringBuilder = new StringBuilder();
            for (int i = 0; i < queries.Length; ++i)
            {
                if (string.IsNullOrEmpty(queries[i].Key) || queries[i].Value == null)
                    continue;
                if (i == 0)
                    queryStringBuilder.Append('?');
                else
                    queryStringBuilder.Append('&');
                if (queries[i].Value.GetType().IsArray ||
                    queries[i].Value.GetType().IsAssignableFrom(typeof(IList)))
                {
                    int j = 0;
                    foreach (object value in queries[i].Value as IEnumerable)
                    {
                        if (j > 0)
                            queryStringBuilder.Append('&');
                        queryStringBuilder.Append(queries[i].Key);
                        queryStringBuilder.Append("=");
                        queryStringBuilder.Append(value);
                        ++j;
                    }
                }
                else
                {
                    queryStringBuilder.Append(queries[i].Key);
                    queryStringBuilder.Append('=');
                    queryStringBuilder.Append(queries[i].Value);
                }
            }
            return queryStringBuilder.ToString();
        }

        public static async Task<Result<TResponse>> Get<TResponse>(string url, params KeyValuePair<string, object>[] queries)
        {
            return await Get<TResponse>(url, string.Empty, queries);
        }

        public static async Task<Result<TResponse>> Get<TResponse>(string url, string authorizationToken, params KeyValuePair<string, object>[] queries)
        {
            Result result = await Get(url + GetQueryString(queries), authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Delete<TResponse>(string url, params KeyValuePair<string, object>[] queries)
        {
            return await Delete<TResponse>(url, string.Empty, queries);
        }

        public static async Task<Result<TResponse>> Delete<TResponse>(string url, string authorizationToken, params KeyValuePair<string, object>[] queries)
        {
            Result result = await Delete(url + GetQueryString(queries), authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Post<TForm, TResponse>(string url, TForm data)
        {
            return await Post<TForm, TResponse>(url, data, string.Empty);
        }

        public static async Task<Result<TResponse>> Post<TForm, TResponse>(string url, TForm data, string authorizationToken)
        {
            Result result = await Post(url, data, authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Post<TResponse>(string url, string authorizationToken)
        {
            Result result = await Post(url, "{}", authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result> Post(string url, string authorizationToken)
        {
            return await Post(url, "{}", authorizationToken);
        }

        public static async Task<Result<TResponse>> Patch<TForm, TResponse>(string url, TForm data)
        {
            return await Patch<TForm, TResponse>(url, data, string.Empty);
        }

        public static async Task<Result<TResponse>> Patch<TForm, TResponse>(string url, TForm data, string authorizationToken)
        {
            Result result = await Patch(url, data, authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Patch<TResponse>(string url, string authorizationToken)
        {
            Result result = await Patch(url, "{}", authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result> Patch(string url, string authorizationToken)
        {
            return await Patch(url, "{}", authorizationToken);
        }

        public static async Task<Result<TResponse>> Put<TForm, TResponse>(string url, TForm data)
        {
            return await Put<TForm, TResponse>(url, data, string.Empty);
        }

        public static async Task<Result<TResponse>> Put<TForm, TResponse>(string url, TForm data, string authorizationToken)
        {
            Result result = await Put(url, data, authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result<TResponse>> Put<TResponse>(string url, string authorizationToken)
        {
            Result result = await Put(url, "{}", authorizationToken);
            return new Result<TResponse>(result.ResponseCode, result.IsHttpError, result.IsNetworkError, result.StringContent, result.Error);
        }

        public static async Task<Result> Put(string url, string authorizationToken)
        {
            return await Put(url, "{}", authorizationToken);
        }

        public static async Task<Result> Get(string url, string authorizationToken)
        {
#if DEBUG_REST_CLIENT
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.Log($"Get request {id} {url}");
#endif
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = true;
            bool isNetworkError = true;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                GetLoadCount++;
            using (UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                webRequest.certificateHandler = new SimpleWebRequestCert();
                if (!string.IsNullOrEmpty(authorizationToken))
                {
#if DEBUG_REST_CLIENT
                    Debug.Log($"Get {id} with authorization token {authorizationToken}");
#endif
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    webRequest.SetRequestHeader("Authorization", "Bearer " + authorizationToken);
                }
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                try
                {
                    UnityWebRequestAsyncOperation ayncOp = webRequest.SendWebRequest();
                    while (!ayncOp.isDone)
                    {
                        await Task.Yield();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG_REST_CLIENT
                    Debug.LogError($"Get error {id} catched {ex}");
                    errorLogged = true;
#else
                    Debug.LogException(ex);
#endif
                }
                responseCode = webRequest.responseCode;
#if UNITY_2020_2_OR_NEWER
                isHttpError = (webRequest.result == UnityWebRequest.Result.ProtocolError);
                isNetworkError = (webRequest.result == UnityWebRequest.Result.ConnectionError);
#else
                isHttpError = webRequest.isHttpError;
                isNetworkError = webRequest.isNetworkError;
#endif
                if (!isNetworkError)
                    stringContent = webRequest.downloadHandler.text;
                else
                    error = webRequest.error;
#if DEBUG_REST_CLIENT
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.LogError($"Get error {id} {stringContent}");
                else
                    Debug.Log($"Get success {id} {webRequest.responseCode} {stringContent}");
#endif
            }
            if (!doNotCountNextRequest)
                GetLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static async Task<Result> Delete(string url, string authorizationToken)
        {
#if DEBUG_REST_CLIENT
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.Log($"Delete request {id} {url}");
#endif
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = true;
            bool isNetworkError = true;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                DeleteLoadCount++;
            using (UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbDELETE))
            {
                webRequest.certificateHandler = new SimpleWebRequestCert();
                if (!string.IsNullOrEmpty(authorizationToken))
                {
#if DEBUG_REST_CLIENT
                    Debug.Log($"Delete {id} with authorization token {authorizationToken}");
#endif
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    webRequest.SetRequestHeader("Authorization", "Bearer " + authorizationToken);
                }
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                try
                {
                    UnityWebRequestAsyncOperation ayncOp = webRequest.SendWebRequest();
                    while (!ayncOp.isDone)
                    {
                        await Task.Yield();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG_REST_CLIENT
                    Debug.LogError($"Delete error {id} catched {ex}");
                    errorLogged = true;
#else
                    Debug.LogException(ex);
#endif
                }
                responseCode = webRequest.responseCode;
#if UNITY_2020_2_OR_NEWER
                isHttpError = (webRequest.result == UnityWebRequest.Result.ProtocolError);
                isNetworkError = (webRequest.result == UnityWebRequest.Result.ConnectionError);
#else
                isHttpError = webRequest.isHttpError;
                isNetworkError = webRequest.isNetworkError;
#endif
                if (!isNetworkError)
                    stringContent = webRequest.downloadHandler.text;
                else
                    error = webRequest.error;
#if DEBUG_REST_CLIENT
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.LogError($"Delete error {id} {stringContent}");
                else
                    Debug.Log($"Delete success {id} {webRequest.responseCode} {stringContent}");
#endif
            }
            if (!doNotCountNextRequest)
                DeleteLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static async Task<Result> Post<TForm>(string url, TForm data)
        {
            return await Post(url, JsonConvert.SerializeObject(data), null);
        }

        public static async Task<Result> Post<TForm>(string url, TForm data, string authorizationToken)
        {
            return await Post(url, JsonConvert.SerializeObject(data), authorizationToken);
        }

        public static async Task<Result> Post(string url, string data, string authorizationToken)
        {
#if DEBUG_REST_CLIENT
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.Log($"Post request {id} {url} {data}");
#endif
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = true;
            bool isNetworkError = true;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                PostLoadCount++;
            using (UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                webRequest.certificateHandler = new SimpleWebRequestCert();
                if (!string.IsNullOrEmpty(authorizationToken))
                {
#if DEBUG_REST_CLIENT
                    Debug.Log($"Post {id} with authorization token {authorizationToken}");
#endif
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    webRequest.SetRequestHeader("Authorization", "Bearer " + authorizationToken);
                }
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data.ToCharArray()));
                webRequest.uploadHandler.contentType = "application/json";
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                try
                {
                    UnityWebRequestAsyncOperation ayncOp = webRequest.SendWebRequest();
                    while (!ayncOp.isDone)
                    {
                        await Task.Yield();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG_REST_CLIENT
                    Debug.LogError($"Post error {id} catched {ex}");
                    errorLogged = true;
#else
                    Debug.LogException(ex);
#endif
                }
                responseCode = webRequest.responseCode;
#if UNITY_2020_2_OR_NEWER
                isHttpError = (webRequest.result == UnityWebRequest.Result.ProtocolError);
                isNetworkError = (webRequest.result == UnityWebRequest.Result.ConnectionError);
#else
                isHttpError = webRequest.isHttpError;
                isNetworkError = webRequest.isNetworkError;
#endif
                if (!isNetworkError)
                    stringContent = webRequest.downloadHandler.text;
                else
                    error = webRequest.error;
#if DEBUG_REST_CLIENT
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.LogError($"Post error {id} {stringContent}");
                else
                    Debug.Log($"Post success {id} {webRequest.responseCode} {stringContent}");
#endif
            }
            if (!doNotCountNextRequest)
                PostLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static async Task<Result> Patch<TForm>(string url, TForm data)
        {
            return await Patch(url, JsonConvert.SerializeObject(data), null);
        }

        public static async Task<Result> Patch<TForm>(string url, TForm data, string authorizationToken)
        {
            return await Patch(url, JsonConvert.SerializeObject(data), authorizationToken);
        }

        public static async Task<Result> Patch(string url, string data, string authorizationToken)
        {
#if DEBUG_REST_CLIENT
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.Log($"Patch request {id} {url} {data}");
#endif
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = true;
            bool isNetworkError = true;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                PatchLoadCount++;
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "PATCH"))
            {
                webRequest.certificateHandler = new SimpleWebRequestCert();
                if (!string.IsNullOrEmpty(authorizationToken))
                {
#if DEBUG_REST_CLIENT
                    Debug.Log($"Patch {id} with authorization token {authorizationToken}");
#endif
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    webRequest.SetRequestHeader("Authorization", "Bearer " + authorizationToken);
                }
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data.ToCharArray()));
                webRequest.uploadHandler.contentType = "application/json";
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                try
                {
                    UnityWebRequestAsyncOperation ayncOp = webRequest.SendWebRequest();
                    while (!ayncOp.isDone)
                    {
                        await Task.Yield();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG_REST_CLIENT
                    Debug.LogError($"Patch error {id} catched {ex}");
                    errorLogged = true;
#else
                    Debug.LogException(ex);
#endif
                }
                responseCode = webRequest.responseCode;
#if UNITY_2020_2_OR_NEWER
                isHttpError = (webRequest.result == UnityWebRequest.Result.ProtocolError);
                isNetworkError = (webRequest.result == UnityWebRequest.Result.ConnectionError);
#else
                isHttpError = webRequest.isHttpError;
                isNetworkError = webRequest.isNetworkError;
#endif
                if (!isNetworkError)
                    stringContent = webRequest.downloadHandler.text;
                else
                    error = webRequest.error;
#if DEBUG_REST_CLIENT
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.LogError($"Patch error {id} {stringContent}");
                else
                    Debug.Log($"Patch success {id} {webRequest.responseCode} {stringContent}");
#endif
            }
            if (!doNotCountNextRequest)
                PatchLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static async Task<Result> Put<TForm>(string url, TForm data)
        {
            return await Put(url, JsonConvert.SerializeObject(data), null);
        }

        public static async Task<Result> Put<TForm>(string url, TForm data, string authorizationToken)
        {
            return await Put(url, JsonConvert.SerializeObject(data), authorizationToken);
        }

        public static async Task<Result> Put(string url, string data, string authorizationToken)
        {
#if DEBUG_REST_CLIENT
            Guid id = Guid.NewGuid();
            bool errorLogged = false;
            Debug.Log($"Put request {id} {url} {data}");
#endif
            bool doNotCountNextRequest = DoNotCountNextRequest;
            long responseCode = -1;
            bool isHttpError = true;
            bool isNetworkError = true;
            string stringContent = string.Empty;
            string error = string.Empty;
            if (!doNotCountNextRequest)
                PutLoadCount++;
            using (UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT))
            {
                webRequest.certificateHandler = new SimpleWebRequestCert();
                if (!string.IsNullOrEmpty(authorizationToken))
                {
#if DEBUG_REST_CLIENT
                    Debug.Log($"Put {id} with authorization token {authorizationToken}");
#endif
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    webRequest.SetRequestHeader("Authorization", "Bearer " + authorizationToken);
                }
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data.ToCharArray()));
                webRequest.uploadHandler.contentType = "application/json";
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                try
                {
                    UnityWebRequestAsyncOperation ayncOp = webRequest.SendWebRequest();
                    while (!ayncOp.isDone)
                    {
                        await Task.Yield();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG_REST_CLIENT
                    Debug.LogError($"Put error {id} catched {ex}");
                    errorLogged = true;
#else
                    Debug.LogException(ex);
#endif
                }
                responseCode = webRequest.responseCode;
#if UNITY_2020_2_OR_NEWER
                isHttpError = (webRequest.result == UnityWebRequest.Result.ProtocolError);
                isNetworkError = (webRequest.result == UnityWebRequest.Result.ConnectionError);
#else
                isHttpError = webRequest.isHttpError;
                isNetworkError = webRequest.isNetworkError;
#endif
                if (!isNetworkError)
                    stringContent = webRequest.downloadHandler.text;
                else
                    error = webRequest.error;
#if DEBUG_REST_CLIENT
                if ((isHttpError || isNetworkError) && !errorLogged)
                    Debug.LogError($"Put error {id} {stringContent}");
                else
                    Debug.Log($"Put success {id} {webRequest.responseCode} {stringContent}");
#endif
            }
            if (!doNotCountNextRequest)
                PutLoadCount--;
            DoNotCountNextRequest = false;
            return new Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }

        public static string GetQueryString(params KeyValuePair<string, string>[] parameters)
        {
            string queryString = string.Empty;
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (string.IsNullOrEmpty(parameters[i].Key) ||
                    string.IsNullOrEmpty(parameters[i].Value))
                    continue;
                if (!string.IsNullOrEmpty(queryString))
                    queryString += "&";
                else
                    queryString += "?";
                queryString += $"{parameters[i].Key}={parameters[i].Value}";
            }
            return queryString;
        }

        public static string GetUrl(string apiUrl, string action)
        {
            if (apiUrl.EndsWith("/"))
                apiUrl = apiUrl.Substring(0, apiUrl.Length - 1);
            if (action.StartsWith("/"))
                action = action.Substring(1);
            return $"{apiUrl}/{action}";
        }

        public interface IResult
        {
            long ResponseCode { get; }
            bool IsHttpError { get; }
            bool IsNetworkError { get; }
            string StringContent { get; }
            string Error { get; }
        }

        public struct Result : IResult
        {
            public long ResponseCode { get; private set; }
            public bool IsHttpError { get; private set; }
            public bool IsNetworkError { get; private set; }
            public string StringContent { get; private set; }
            public string Error { get; private set; }

            public Result(long responseCode, bool isHttpError, bool isNetworkError, string stringContent, string error)
            {
                ResponseCode = responseCode;
                IsHttpError = isHttpError;
                IsNetworkError = isNetworkError;
                StringContent = stringContent;
                Error = error;
            }
        }

        public struct Result<T> : IResult
        {
            public long ResponseCode { get; private set; }
            public bool IsHttpError { get; private set; }
            public bool IsNetworkError { get; private set; }
            public string StringContent { get; private set; }
            public string Error { get; private set; }
            public T Content { get; private set; }

            public Result(long responseCode, bool isHttpError, bool isNetworkError, string stringContent, string error)
            {
                ResponseCode = responseCode;
                StringContent = stringContent;
                IsHttpError = isHttpError;
                IsNetworkError = isNetworkError;
                Error = error;
                Content = default;
                if (!IsHttpError && !IsNetworkError)
                {
                    try
                    {
                        Content = JsonConvert.DeserializeObject<T>(stringContent);
                    }
                    catch (Exception ex)
                    {
                        // It may not able to deserialize
                        Debug.LogError($"Can't deserialize content: {ex}");
                    }
                }
            }
        }
    }
}
