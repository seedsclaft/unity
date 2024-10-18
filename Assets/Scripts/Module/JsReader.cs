using System.Runtime.InteropServices;

public class JsReader
{
    [DllImport("__Internal")]
    public static extern void InjectionJs(string url);

    [DllImport("__Internal")]
    public static extern void InjectionCSS(string url);


    public static void Initialize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        {
            var url = "https://www.gstatic.com/firebasejs/10.7.0/firebase-app-compat.js";
            InjectionJs(url);
            url = "https://www.gstatic.com/firebasejs/10.7.0/firebase-firestore-compat.js";
            InjectionJs(url);
        }
#endif
    }

}