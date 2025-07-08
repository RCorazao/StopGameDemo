using StopGame.Domain.ValueObjects;

namespace StopGame.Domain.Entities;

public class Round
{
    public Guid Id { get; set; }
    public char Letter { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<Submission> Submissions { get; set; } = new();
    public List<Vote> Votes { get; set; } = new();
    public bool IsActive { get; set; } = true;
    
    public Round()
    {
        Id = Guid.NewGuid();
        Letter = GenerateRandomLetter();
        StartedAt = DateTime.UtcNow;
    }
    
    private static char GenerateRandomLetter()
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();
        return letters[random.Next(letters.Length)];
    }
    
    public void EndRound()
    {
        IsActive = false;
        EndedAt = DateTime.UtcNow;
    }
    
    public void AddSubmission(Submission submission)
    {
        if (IsActive)
        {
            // Remove existing submission from same player for same topic
            var existingSubmission = Submissions.FirstOrDefault(s => 
                s.PlayerId == submission.PlayerId && s.TopicName == submission.TopicName);
            
            if (existingSubmission != null)
            {
                Submissions.Remove(existingSubmission);
            }
            
            Submissions.Add(submission);
        }
    }
    
    public void AddVote(Vote vote)
    {
        // Remove existing vote from same voter for same answer
        var existingVote = Votes.FirstOrDefault(v => 
            v.VoterId == vote.VoterId && 
            v.AnswerOwnerId == vote.AnswerOwnerId && 
            v.TopicName == vote.TopicName);
        
        if (existingVote != null)
        {
            Votes.Remove(existingVote);
        }
        
        Votes.Add(vote);
    }
    
    public List<Submission> GetSubmissionsForPlayer(Guid playerId)
    {
        return Submissions.Where(s => s.PlayerId == playerId).ToList();
    }
    
    public List<Submission> GetSubmissionsForTopic(string topicName)
    {
        return Submissions.Where(s => s.TopicName == topicName).ToList();
    }
    
    public bool HasPlayerSubmitted(Guid playerId)
    {
        return Submissions.Any(s => s.PlayerId == playerId);
    }
    
    public Dictionary<Guid, int> CalculateScores(List<string> topicNames)
    {
        var scores = new Dictionary<Guid, int>();
        
        foreach (var topicName in topicNames)
        {
            var topicSubmissions = GetSubmissionsForTopic(topicName);
            var validSubmissions = topicSubmissions.Where(s => IsAnswerValid(s, topicName)).ToList();
            
            foreach (var submission in validSubmissions)
            {
                if (!scores.ContainsKey(submission.PlayerId))
                    scores[submission.PlayerId] = 0;
                
                // Base points for valid answer
                int points = 10;
                
                // Bonus points for unique answers
                var sameAnswers = validSubmissions.Count(s => 
                    s.Answer.Word.Equals(submission.Answer.Word, StringComparison.OrdinalIgnoreCase));
                
                if (sameAnswers == 1)
                {
                    points += 5; // Unique answer bonus
                }
                
                scores[submission.PlayerId] += points;
            }
        }
        
        return scores;
    }
    
    private bool IsAnswerValid(Submission submission, string topicName)
    {
        var topicVotes = Votes.Where(v => 
            v.AnswerOwnerId == submission.PlayerId && 
            v.TopicName == topicName).ToList();
        
        if (!topicVotes.Any())
            return true; // No votes means valid by default
        
        var validVotes = topicVotes.Count(v => v.IsValid);
        var invalidVotes = topicVotes.Count(v => !v.IsValid);
        
        return validVotes >= invalidVotes;
    }
}