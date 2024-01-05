namespace FHTW.Swen1.Swamp
{
    public static class TokenHelper
    {
        public static string ExtractUsernameFromToken(string token)
        {
            // Token im Format "Bearer <username>-mtcgToken"
            const string tokenPrefix = "Bearer ";
            const string tokenSuffix = "-mtcgToken";

            if (string.IsNullOrWhiteSpace(token) || !token.StartsWith(tokenPrefix) || !token.EndsWith(tokenSuffix))
            {
                return null;
            }

            var startIndex = tokenPrefix.Length;
            var endIndex = token.Length - tokenSuffix.Length;

            return token.Substring(startIndex, endIndex - startIndex);
        }
    }

 }