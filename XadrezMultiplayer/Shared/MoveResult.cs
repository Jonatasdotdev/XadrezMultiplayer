public class MoveResult
{
    public bool IsValid { get; set; }
    public bool IsCheck { get; set; }
    public bool IsCheckmate { get; set; }
    public bool IsDraw { get; set; }
    public string GameState { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    
}