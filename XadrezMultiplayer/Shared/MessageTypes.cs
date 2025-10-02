namespace Shared;

public static class MessageTypes
{
        public const string Login = "login";
        public const string LoginResponse = "login_response";
        public const string Register = "register";
        public const string RegisterResponse = "register_response";
        public const string Invite = "invite";
        public const string InviteResponse = "invite_response";
        public const string Move = "move";
        public const string MoveResponse = "move_response";
        public const string DrawOffer = "draw_offer";
        public const string DrawResponse = "draw_response";
        public const string Resign = "resign";
        public const string Error = "error";

        public const string Ping = "ping";

        public const string PingResponse = "ping_response";

        public const string GetOlineUsersRequest = "get_online_users";
        public const string GetOnlineUsersResponse = "get_online_users_response";
}