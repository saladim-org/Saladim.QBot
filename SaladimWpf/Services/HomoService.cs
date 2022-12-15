using System.Dynamic;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows;

namespace SaladimWpf.Services;

public class HomoService
{
    protected V8ScriptEngine jsEngine;
    protected HttpService httpService;

    public HomoService(HttpService httpService)
    {
        this.httpService = httpService;
        jsEngine = new();
        if (!Directory.Exists("js_src"))
            Directory.CreateDirectory("js_src");
        if (!File.Exists("js_src/homo.js"))
        {
            var r = httpService.HttpClient.GetAsync("https://cdn.jsdelivr.net/gh/itorr/homo@master/homo.js");
            using FileStream fs = new("js_src/homo.js", FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new(fs);
            using StreamReader sr = new(r.Result.Content.ReadAsStream());
            sw.Write(sr.ReadToEnd());
        }
        string js = File.ReadAllText("js_src/homo.js");
        jsEngine.Execute("function get_homo_method() {" + js + "\treturn homo;\r\n}");
    }

    public string Homo(double d)
    {
        string result = (jsEngine.Script.get_homo_method())(d);
        return result;
    }
}
