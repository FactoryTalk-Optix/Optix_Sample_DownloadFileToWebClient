#region Using directives
using System;
using System.IO;
using System.Net;
using System.Text;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.WebUI;
#endregion

/// <summary>
/// Class responsible for listing files in a directory and serving them over HTTP.
/// </summary>
public class FilesLister : BaseNetLogic
{
    private HttpListener httpListener;
    private string directoryPath;

    /// <summary>
    /// Initializes and starts the HTTP listener.
    /// </summary>
    public override void Start()
    {
        // Get the base folder 
        try
        {
            directoryPath = new ResourceUri(LogicObject.GetVariable("BaseDirectory").Value).Uri;
            Log.Info("FilesList.Start", "Base directory: " + directoryPath);
        }
        catch (Exception ex)
        {
            Log.Warning("FilesList.Start", "Failed to get the base directory, falling back to ProjectDir. Exception: " + ex.Message);
            directoryPath = Project.Current.ProjectDirectory;
        }
        // Check if the directory exists
        if (!Directory.Exists(directoryPath))
        {
            Log.Error("FilesList.Start", "Directory does not exist: " + directoryPath);
            return;
        }

        // Read the server address from the configuration
        var serverAddress = (string) LogicObject.GetVariable("ServerAddress").Value.Value;
        // Validate the server address
        if (string.IsNullOrEmpty(serverAddress))
        {
            Log.Error("FilesList.Start", "Server address is not set");
            return;
        }
        // Check if the server address is valid using regex
        if (!System.Text.RegularExpressions.Regex.IsMatch(serverAddress, @"^([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$"))
        {
            Log.Error("FilesList.Start", "Invalid server address: " + serverAddress);
            return;
        }

        // Read the server port from the configuration
        var serverPort = LogicObject.GetVariable("ServerPort").Value;
        // Validate the server port
        if (string.IsNullOrEmpty(serverPort) || !int.TryParse(serverPort, out var port))
        {
            Log.Error("FilesList.Start", "Invalid server port: " + serverPort);
            return;
        }

        try
        {
            // Initialize and start the HTTP listener
            httpListener = new HttpListener();
            var fullAddress = $"http://{serverAddress}:{port}/";
            Log.Info("FilesList.Start", "Starting HTTP listener at " + fullAddress);
            httpListener.Prefixes.Add(fullAddress); // Change this to the desired IP and port
            httpListener.Start();
            httpListener.BeginGetContext(OnRequest, null);
            Log.Info("FilesList.Start", $"HTTP listener started at {fullAddress}");
        }
        catch (Exception ex)
        {
            Log.Error("FilesList.Start", "Failed to start the HTTP listener. Exception: " + ex.Message);
        }
    }

    /// <summary>
    /// Stops the HTTP listener.
    /// </summary>
    public override void Stop()
    {
        httpListener.Stop();
    }

    /// <summary>
    /// Handles incoming HTTP requests.
    /// </summary>
    /// <param name="result">The result of the asynchronous operation.</param>
    private void OnRequest(IAsyncResult result)
    {
        // Check if the listener is still active
        if (!httpListener.IsListening)
            return;

        // Get the context of the incoming request
        var context = httpListener.EndGetContext(result);
        httpListener.BeginGetContext(OnRequest, null);

        var request = context.Request;
        var response = context.Response;

        // Handle the root URL by listing all files
        if (request.Url.AbsolutePath == "/")
        {
            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            var sb = new StringBuilder();
            sb.Append("<html><body><h1>Files List</h1><ul>");

            // Create a list of files with download links
            foreach (var file in files)
            {
                var relativePath = file.Substring(directoryPath.Length + 1).Replace("\\", "/");
                sb.AppendFormat("<li><a href=\"/download/{0}\">{1}</a></li>", relativePath, relativePath);
            }

            sb.Append("</ul></body></html>");
            var buffer = Encoding.UTF8.GetBytes(sb.ToString());
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        // Handle file download requests
        else if (request.Url.AbsolutePath.StartsWith("/download/"))
        {
            var filePath = Path.Combine(directoryPath, request.Url.AbsolutePath.Substring(10).Replace("/", "\\"));
            if (File.Exists(filePath))
            {
                response.ContentType = "application/octet-stream";
                response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(filePath));
                var buffer = File.ReadAllBytes(filePath);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = (int) HttpStatusCode.NotFound;
            }
        }
        // Handle unknown URLs
        else
        {
            response.StatusCode = (int) HttpStatusCode.NotFound;
        }

        // Close the response stream
        response.OutputStream.Close();
    }
}
