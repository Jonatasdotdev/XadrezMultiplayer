namespace Shared.Messages
{
    public class InviteRequest : MessageBase
    {
        public InviteRequest() => Type = "invite";
        public string TargetUsername { get; set; } = null!;
    }

    public class InviteResponse : MessageBase
    {
        public InviteResponse() => Type = "invite_response";
        public string InviteId { get; set; } = null!;
        public string FromUsername { get; set; } = null!;
        public bool Accept { get; set; }
    }
}