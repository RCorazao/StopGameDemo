namespace StopGame.Domain.ValueObjects;

public class Answer
{
    public string Word { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    
    public Answer() { }
    
    public Answer(string word, string topicName)
    {
        Word = word?.Trim() ?? string.Empty;
        TopicName = topicName?.Trim() ?? string.Empty;
    }
    
    public bool IsValid(char requiredLetter)
    {
        if (string.IsNullOrWhiteSpace(Word))
            return false;
            
        return char.ToUpperInvariant(Word[0]) == char.ToUpperInvariant(requiredLetter);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Answer other)
            return false;
            
        return Word.Equals(other.Word, StringComparison.OrdinalIgnoreCase) &&
               TopicName.Equals(other.TopicName, StringComparison.OrdinalIgnoreCase);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Word.ToLowerInvariant(), 
            TopicName.ToLowerInvariant());
    }
}