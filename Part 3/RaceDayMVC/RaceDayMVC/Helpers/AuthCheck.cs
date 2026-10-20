namespace RaceDayMVC.Helpers
{
    public static class AuthCheck
    {
        public static bool IsLoggedIn(ISession session)
            => !string.IsNullOrEmpty(session.GetString("JwtToken"));

        public static bool IsOrganiser(ISession session)
            => session.GetString("IsOrganiser") == "True";

        public static bool IsParticipant(ISession session)
            => session.GetString("IsParticipant") == "True";

        public static int GetUserId(ISession session)
            => session.GetInt32("UserID") ?? 0;

        public static string GetFullName(ISession session)
            => session.GetString("FullName") ?? "";

        public static string GetEmail(ISession session)
            => session.GetString("Email") ?? "";
    }
}