using JWT.Algorithms;
using JWT.Builder;
using UnityEngine;

public class TokenDecoded
{
    public string userTag;
    public string iat;
    public string exp;

    public TokenDecoded(string token, bool verify)
    {
        if (verify)
        {
            var json = JwtBuilder.Create()
                     .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                     .WithSecret(GlobalVariables.TokenDecodeString)
                     .MustVerifySignature()
                     .Decode(token);

            TokenDecoded aux = JsonUtility.FromJson<TokenDecoded>(json);
            this.userTag = aux.userTag;
            this.iat = aux.iat;
            this.exp = aux.exp;
        }
        else
        {
            var json = JwtBuilder.Create()
                     .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                     .WithSecret(GlobalVariables.TokenDecodeString)
                     .Decode(token);

            TokenDecoded aux = JsonUtility.FromJson<TokenDecoded>(json);
            this.userTag = aux.userTag;
            this.iat = aux.iat;
            this.exp = aux.exp;
        }
    }
}
