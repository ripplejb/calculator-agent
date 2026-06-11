public class Logprob
{
    public string? token { get; set; }
    public int logprob { get; set; }
    public List<int>? bytes { get; set; }
    public List<TopLogprob>? top_logprobs { get; set; }
}

public class PromptResponse
{
    public string? model { get; set; }
    public string? created_at { get; set; }
    public string? response { get; set; }
    public string? thinking { get; set; }
    public bool done { get; set; }
    public string? done_reason { get; set; }
    public int total_duration { get; set; }
    public int load_duration { get; set; }
    public int prompt_eval_count { get; set; }
    public int prompt_eval_duration { get; set; }
    public int eval_count { get; set; }
    public int eval_duration { get; set; }
    public List<Logprob>? logprobs { get; set; }
}

public class TopLogprob
{
    public string? token { get; set; }
    public int logprob { get; set; }
    public List<int>? bytes { get; set; }
}
