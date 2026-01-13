namespace Walacor_SDK.W_Client.Constants
{
    internal static class ErrorMessageFactory
    {
        public static string FileNotReady(string? status)
            => $"File is not ready to download. Status: {status ?? FileConstants.Unknown}";
    }
}
