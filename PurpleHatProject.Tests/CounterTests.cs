using Bunit;
using PurpleHatProject.Components.Pages;

namespace PurpleHatProject.Tests;

public class CounterTests : BunitContext
{
    [Fact]
    public void CounterStartsAtZero()
    {
        var cut = Render<Counter>();

        cut.Find("p[role='status']").MarkupMatches("<p role=\"status\">Current count: 0</p>");
    }

    [Fact]
    public void ClickingButtonIncrementsCount()
    {
        var cut = Render<Counter>();

        cut.Find("button").Click();

        cut.Find("p[role='status']").MarkupMatches("<p role=\"status\">Current count: 1</p>");
    }

    [Fact]
    public void ClickingButtonMultipleTimesIncrementsCorrectly()
    {
        var cut = Render<Counter>();

        cut.Find("button").Click();
        cut.Find("button").Click();
        cut.Find("button").Click();

        cut.Find("p[role='status']").MarkupMatches("<p role=\"status\">Current count: 3</p>");
    }
}
