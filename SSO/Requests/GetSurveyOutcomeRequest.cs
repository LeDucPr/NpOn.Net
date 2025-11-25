using System.ComponentModel.DataAnnotations;

namespace SSO.Requests;

public class GetSurveyOutcomeRequest
{
    [Required]
    public string? SurveyId { get; set; }
}
