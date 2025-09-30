namespace Shared.Messages;
public class DrawOffer : MessageBase
{
    public DrawOffer() => Type = "draw_offer";
}

    public class DrawResponse : MessageBase
    {
        public DrawResponse() => Type = "draw_response";
        public bool Accept { get; set; }
    }