using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace SaladimWpf.Services;

public class HttpService
{
    protected HttpListener httpListener;
    protected HttpClient httpClient;

    public HttpClient HttpClient => httpClient;

    public HttpService()
    {
        bool error = false;
        httpListener = new();
        try
        {
            httpListener.Prefixes.Add("http://127.0.0.1:5702/");
            httpListener.Start();
        }
        catch { error = true; httpListener.Close(); }
        if (!error)
            Task.Run(ListenerLoop);
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Saladim.QBot external value fetcher");
    }

    private void ListenerLoop()
    {
        while (true)
        {
            var context = httpListener.GetContext(); //监听http请求
            var imgName = context.Request.QueryString["img_name"]; //获取query string
            var fileName = $"tempImages\\{imgName}";
            if (File.Exists(fileName))
            {
                using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
                var res = context.Response;
                res.ContentType = "image/bmp";
                res.StatusCode = 200;//状态码
                CopyStream(fs, context.Response.OutputStream);//写入返回流
                res.Close();//完成回应
                continue;
            }
            else
            {
                //图片
                var res = context.Response;
                res.ContentType = "text/plain";
                res.StatusCode = 404;
                using StreamWriter sw = new(context.Response.OutputStream, Encoding.UTF8);
                sw.WriteLine("图片未找到");
                sw.Close();
                context.Response.Close();
                continue;
            }
        }
    }

    public static void CopyStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[3 * SaladimQBot.Shared.Size.KiB];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }
}
