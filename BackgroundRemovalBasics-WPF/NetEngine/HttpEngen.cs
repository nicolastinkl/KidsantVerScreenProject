using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.DirectoryServices.Protocols;
using System.ServiceModel.Security;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
 
/*   
 * 原文出处：http://blog.csdn.net/zhoufoxcn 或http://zhoufoxcn.blog.51cto.com 
 * */
namespace HttpEngen
{
    
    /// <summary>
    /// 有关HTTP请求的辅助类
    /// </summary>
    /// 
    public class HttpWebResponseUtility
    {
        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        /// <summary>
        /// 创建GET方式的HTTP请求
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="timeout">请求的超时时间</param>
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>
        /// <returns></returns>
        public static HttpWebResponse CreateGetHttpResponse(string url, int? timeout, string userAgent, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            return request.GetResponse() as HttpWebResponse;
        }
        /// <summary>
        /// 创建POST方式的HTTP请求
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="parameters">随同请求POST的参数名称及参数值字典</param>
        /// <param name="timeout">请求的超时时间</param>
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>
        /// <param name="requestEncoding">发送HTTP请求时所用的编码</param>
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>
        /// <returns></returns>
        public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int? timeout, string userAgent, Encoding requestEncoding, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (requestEncoding == null)
            {
                throw new ArgumentNullException("requestEncoding");
            }
            HttpWebRequest request = null;
            //如果是发送HTTPS请求
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentType = "application/json";


            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //如果需要POST数据
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                    }
                    i++;
                }
                byte[] data = requestEncoding.GetBytes(buffer.ToString());
                try
                {
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    return request.GetResponse() as HttpWebResponse;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("ex" + ex.ToString());
                    return null;
                }
                

            }
            return null;
            
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受
        }

        public static Dictionary<string, string> jsonParse(string rawjson)
        {
            Dictionary<string, string> outdict = new Dictionary<string, string>();
            StringBuilder keybufferbuilder = new StringBuilder();
            StringBuilder valuebufferbuilder = new StringBuilder();
            StringReader bufferreader = new StringReader(rawjson);

            int s = 0;
            bool reading = false;
            bool inside_string = false;
            bool reading_value = false;
            //break at end (returns -1)
            while (s >= 0)
            {
                s = bufferreader.Read();
                //opening of json
                if (!reading)
                {
                    if ((char)s == '{' && !inside_string && !reading) reading = true;
                    continue;
                }
                else
                {
                    //if we find a quote and we are not yet inside a string, advance and get inside
                    if (!inside_string)
                    {
                        //read past the quote
                        if ((char)s == '\"') inside_string = true;
                        continue;
                    }
                    if (inside_string)
                    {
                        //if we reached the end of the string
                        if ((char)s == '\"')
                        {
                            inside_string = false;
                            s = bufferreader.Read(); //advance pointer
                            if ((char)s == ':')
                            {
                                reading_value = true;
                                continue;
                            }
                            if (reading_value && (char)s == ',')
                            {
                                //we know we just ended the line, so put itin our dictionary
                                if (!outdict.ContainsKey(keybufferbuilder.ToString())) outdict.Add(keybufferbuilder.ToString(), valuebufferbuilder.ToString());
                                //and clear the buffers
                                keybufferbuilder.Clear();
                                valuebufferbuilder.Clear();
                                reading_value = false;
                            }
                            if (reading_value && (char)s == '}')
                            {
                                //we know we just ended the line, so put itin our dictionary
                                if (!outdict.ContainsKey(keybufferbuilder.ToString())) outdict.Add(keybufferbuilder.ToString(), valuebufferbuilder.ToString());
                                //and clear the buffers
                                keybufferbuilder.Clear();
                                valuebufferbuilder.Clear();
                                reading_value = false;
                                reading = false;
                                break;
                            }
                        }
                        else
                        {
                            if (reading_value)
                            {
                                valuebufferbuilder.Append((char)s);
                                continue;
                            }
                            else
                            {
                                keybufferbuilder.Append((char)s);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        switch ((char)s)
                        {
                            case ':':
                                reading_value = true;
                                break;
                            default:
                                if (reading_value)
                                {
                                    valuebufferbuilder.Append((char)s);
                                }
                                else
                                {
                                    keybufferbuilder.Append((char)s);
                                }
                                break;
                        }
                    }
                }
            }
            return outdict;
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        public static string GetModel(string strUrl)
        {
            string strRet = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strUrl);
                request.Timeout = 2000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                System.IO.Stream resStream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.Default;
                StreamReader readStream = new StreamReader(resStream, encode);
                Char[] read = new Char[256];
                int count = readStream.Read(read, 0, 256);
                while (count > 0)
                {
                    String str = new String(read, 0, count);
                    strRet = strRet + str;
                    count = readStream.Read(read, 0, 256);
                }
                resStream.Close();
            }
            catch (Exception e)
            {
                strRet = "";
            }
            return strRet;
        }

        public static string PostModel(string strUrl, string strParm)
        {
            Encoding encode = System.Text.Encoding.Default;

            byte[] arrB = encode.GetBytes(strParm);
            string strBaseUrl = null;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(strUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = arrB.Length;
            Stream outStream = myReq.GetRequestStream();
            outStream.Write(arrB, 0, arrB.Length);
            outStream.Close();
            WebResponse myResp = null;
            try
            {
                //接收HTTP做出的响应
                myResp = myReq.GetResponse();
            }
            catch (Exception e)
            {
                int ii = 0;
            }
            Stream ReceiveStream = myResp.GetResponseStream();
            StreamReader readStream = new StreamReader(ReceiveStream, encode);
            Char[] read = new Char[256];
            int count = readStream.Read(read, 0, 256);
            string str = null;
            while (count > 0)
            {
                str += new String(read, 0, count);
                count = readStream.Read(read, 0, 256);
            }
            readStream.Close();
            myResp.Close();
            return str;
        }
    }
}