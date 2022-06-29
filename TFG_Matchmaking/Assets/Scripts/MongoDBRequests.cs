using System.Text;
using System.Collections;
using UnityEngine.Networking;
using Mirror;
using MessagesTFG;

public class MongoDBRequests
{
    private static string databaseIP = "http://localhost:3000";
    public static IEnumerator GetPlayerRequest(LoginMessage msg, NetworkConnection connection, System.Action<string, PlayerData, NetworkConnection> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(databaseIP + "/players/" + msg.Username))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.Log(request.error);
                if (callback != null)
                {
                    callback.Invoke("", null, connection);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke(msg.Password, PlayerData.Parse(request.downloadHandler.text), connection);
                }
            }
        }
    }

    public static IEnumerator GetPlayerRequest(string token, NetworkConnection connection, System.Action<PlayerData, string, NetworkConnection> callback = null)
    {
        TokenDecoded aux = new TokenDecoded(token, false);

        using (UnityWebRequest request = UnityWebRequest.Get(databaseIP + "/players/" + aux.userTag))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.Log(request.error);
                if (callback != null)
                {
                    callback.Invoke(null, token, connection);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke(PlayerData.Parse(request.downloadHandler.text), token, connection);
                }
            }
        }
    }

    public static IEnumerator GetPlayerRequest_EndMatch(string userTag, int puesto, NetworkConnection connection, System.Action<PlayerData, int, NetworkConnection> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(databaseIP + "/players/" + userTag))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.Log(request.error);
                if (callback != null)
                {
                    callback.Invoke(null, puesto, connection);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke(PlayerData.Parse(request.downloadHandler.text), puesto, connection);
                }
            }
        }
    }

    public static IEnumerator GetPlayerTokenRequest(PlayerData player, NetworkConnection connection, System.Action<PlayerData, string, NetworkConnection> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(databaseIP + "/players/token/" + player.player_tag))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.Log(request.error);
                if (callback != null)
                {
                    callback.Invoke(player, null, connection);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke(player, request.downloadHandler.text, connection);
                }
            }
        }
    }

    public static IEnumerator PostPlayerRequest(string profile, NetworkConnection connection, System.Action<bool, NetworkConnection> callback = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(databaseIP + "/players", "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(profile);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.Log(request.error);
                if (callback != null)
                {
                    callback.Invoke(false, connection);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke(request.downloadHandler.text != "{}", connection);
                }
            }
        }
    }

    public static IEnumerator PutPlayerRequest(PlayerData player, NetworkConnection connection, System.Action<bool, NetworkConnection> callback = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(databaseIP + "/players/" + player.player_tag, "PUT"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(player.Stringify());
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.Log(request.error);
                if (callback != null)
                {
                    callback.Invoke(false, connection);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke(request.downloadHandler.text != "{}", connection);
                }
            }
        }
    }

}



