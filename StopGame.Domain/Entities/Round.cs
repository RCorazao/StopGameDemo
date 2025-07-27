using StopGame.Domain.ValueObjects;

namespace StopGame.Domain.Entities;

public class Round
{
    public Guid Id { get; set; }
    public char Letter { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<Answer> Answers { get; set; } = new();
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
    
    public void AddAnswer(Answer answer)
    {
        if (IsActive)
        {
            // Remove existing answer from same player for same topic
            var existingAnswer = Answers.FirstOrDefault(a => 
                a.PlayerId == answer.PlayerId && a.TopicId == answer.TopicId);
            
            if (existingAnswer != null)
            {
                Answers.Remove(existingAnswer);
            }
            
            Answers.Add(answer);
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
    
    public List<Answer> GetAnswersForPlayer(Guid playerId)
    {
        return Answers.Where(a => a.PlayerId == playerId).ToList();
    }
    
    public List<Answer> GetAnswersForTopic(Guid topicId)
    {
        return Answers.Where(a => a.TopicId == topicId).ToList();
    }
    
    public bool HasPlayerAnswered(Guid playerId, Guid TopicId)
    {
        return Answers.Any(a => a.PlayerId == playerId && a.TopicId == TopicId);
    }
    
    public Dictionary<Guid, int> CalculateScores(List<string> topicNames)
    {
        var scores = new Dictionary<Guid, int>();
        
        //foreach (var topicName in topicNames)
        //{
        //    var topicSubmissions = GetSubmissionsForTopic(topicName);
        //    var validSubmissions = topicSubmissions.Where(s => IsAnswerValid(s, topicName)).ToList();
            
        //    foreach (var submission in validSubmissions)
        //    {
        //        if (!scores.ContainsKey(submission.PlayerId))
        //            scores[submission.PlayerId] = 0;
                
        //        // Base points for valid answer
        //        int points = 10;
                
        //        // Bonus points for unique answers
        //        var sameAnswers = validSubmissions.Count(s => 
        //            s.Answer.Word.Equals(submission.Answer.Word, StringComparison.OrdinalIgnoreCase));
                
        //        if (sameAnswers == 1)
        //        {
        //            points += 5; // Unique answer bonus
        //        }
                
        //        scores[submission.PlayerId] += points;
        //    }
        //}
        
        return scores;
    }
    
    //private bool IsAnswerValid(Answer answer, Guid topicId)
    //{
    //    var topicVotes = Votes.Where(v => 
    //        v.AnswerOwnerId == submission.PlayerId && 
    //        v.TopicName == topicName).ToList();
        
    //    if (!topicVotes.Any())
    //        return true; // No votes means valid by default
        
    //    var validVotes = topicVotes.Count(v => v.IsValid);
    //    var invalidVotes = topicVotes.Count(v => !v.IsValid);
        
    //    return validVotes >= invalidVotes;
    //}
}