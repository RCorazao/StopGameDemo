using StopGame.Domain.ValueObjects;

namespace StopGame.Domain.Entities;

public class Round
{
    public Guid Id { get; set; }
    public char Letter { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<Answer> Answers { get; set; } = new();
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

    public Answer? GetAnswerById(Guid answerId)
    {
        return Answers.FirstOrDefault(a => a.Id == answerId);
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
}