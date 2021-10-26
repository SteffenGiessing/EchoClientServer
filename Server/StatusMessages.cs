namespace Server
{
    public static class ErrorFormatter
    {
        public static string FormatMissingMessage(string element) => $"{(int) Status.BadReq} Missing {element} ";
        public static string FormatIllegalMessage(string element) => $"{(int) Status.BadReq} Illegal {element} ";
        
        public static string FormatGenericMessage(Status status, string element) => $"{(int) status} {element} ";
    }

    public static class StatusMessages
    {
        public const string BadRequest = "Bad Request";
    }

    public enum Status
    {
        Ok = 1,
        Created = 2,
        Updated = 3,
        BadReq = 4,
        NotFound = 5,
        Error = 6
    }
}