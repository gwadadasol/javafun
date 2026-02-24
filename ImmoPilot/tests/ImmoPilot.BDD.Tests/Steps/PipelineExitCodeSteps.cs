using ImmoPilot.Application.Models;
using Reqnroll;
using Shouldly;

namespace ImmoPilot.BDD.Tests.Steps;

[Binding]
public class PipelineExitCodeSteps
{
    private PipelineResult _pipelineResult;
    private int _exitCode;

    [Given("the pipeline result is {word}")]
    public void GivenPipelineResult(string result)
    {
        _pipelineResult = Enum.Parse<PipelineResult>(result);
    }

    [When("the exit code is determined")]
    public void WhenExitCodeDetermined()
    {
        _exitCode = (int)_pipelineResult;
    }

    [Then("the exit code is {int}")]
    public void ThenExitCodeIs(int expected)
    {
        _exitCode.ShouldBe(expected);
    }
}
