using CodeRoom.Web.Models;

namespace CodeRoom.Web.ViewModels.Learning;

public class ChallengeListViewModel
{
    public Course Course { get; set; } = null!;
    public IReadOnlyList<Challenge> Challenges { get; set; } = [];
}
