namespace Shared.Messages

{

    public class GetOnlineUsersRequest : MessageBase
    {
        public GetOnlineUsersRequest() => Type = "get_online_users";
    }


    public class GetOnlineUsersResponse : MessageBase
    {
        public GetOnlineUsersResponse() => Type = "get_online_users_response";
        public List<string> Users { get; set; } = new List<string>();
    }
}