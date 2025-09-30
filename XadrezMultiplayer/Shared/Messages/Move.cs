namespace Shared.Messages;

 public class MoveRequest : MessageBase
    {
        public MoveRequest() => Type = "move";
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public string? Promotion { get; set; }
    }

    public class MoveResponse : MessageBase
    {
        public MoveResponse() => Type = "move_response";
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public bool IsDraw { get; set; }
        public string? BoardState { get; set; }
    }
