using System.Net;

namespace Level.IO
{

    public interface IVirtualFS
    {
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
        string GetTextContent(string path);
        string SaveTextContent(string path, string data);
    }

    public class FTPLoader
    {
        FTPConnectionSettings _connectionSettings;
        // public async Task<string[]> GetDirectories(string path)
        // {
        //     var request = (FtpWebRequest)FtpWebRequest.Create(path);
        //     if (!string.IsNullOrWhiteSpace(_connectionSettings.user)) {
        //         request.Credentials = new NetworkCredential(_connectionSettings.user, _connectionSettings.password);
        //     }
        //     request.Method = WebRequestMethods.Ftp.ListDirectory;
        //     request.KeepAlive = false;
            
        //     using (var response = (FtpWebResponse)await request.GetResponseAsync()) {
        //         using (var stream = response.GetResponseStream()) {
        //             // stream.
        //         }
        //     }
        // }
    }

    public struct FTPConnectionSettings
    {
        public string user;
        public string password;
    }
}