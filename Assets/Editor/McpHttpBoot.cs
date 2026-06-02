using UnityEditor;
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Services.Transport;

public static class McpHttpBoot
{
    public static void StartHttp()
    {
        // Use literal strings to bypass internal protection level of EditorPrefKeys
        EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);
        EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);

        // Access MCPServiceLocator using its correct namespace (MCPForUnity.Editor.Services)
        var task = MCPServiceLocator.TransportManager.StartAsync(TransportMode.Http);
        
        UnityEngine.Debug.Log("MCP HTTP Transport start requested via McpHttpBoot.StartHttp()");
    }
}
