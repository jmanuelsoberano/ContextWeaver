using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ContextWeaver.Cli.Commands.Wizard;
using Xunit;

namespace ContextWeaver.Cli.Tests;

public class WizardOrchestratorTests
{
    [Fact]
    public async Task ExecuteAsync_AllNext_ShouldCompleteSuccessfully()
    {
        // Arrange
        var step1 = new MockStep { Result = StepResult.Next };
        var step2 = new MockStep { Result = StepResult.Next };
        var orchestrator = new WizardOrchestrator(new[] { step1, step2 });

        // Act
        var result = await orchestrator.ExecuteAsync(CreateContext());

        // Assert
        Assert.Equal(0, result);
        Assert.Equal(1, step1.ExecuteCount);
        Assert.Equal(1, step2.ExecuteCount);
    }

    [Fact]
    public async Task ExecuteAsync_SkipStep_ShouldNotExecuteSkippedStep()
    {
        // Arrange
        var step1 = new MockStep { Result = StepResult.Next };
        var step2 = new MockStep { ShouldEx = false };
        var step3 = new MockStep { Result = StepResult.Next };
        var orchestrator = new WizardOrchestrator(new[] { step1, step2, step3 });

        // Act
        var result = await orchestrator.ExecuteAsync(CreateContext());

        // Assert
        Assert.Equal(0, result);
        Assert.Equal(1, step1.ExecuteCount);
        Assert.Equal(0, step2.ExecuteCount);
        Assert.Equal(1, step3.ExecuteCount);
    }

    [Fact]
    public async Task ExecuteAsync_GoBackFromStep2_ShouldReExecuteStep1()
    {
        // Arrange
        var step1 = new MockStep { Result = StepResult.Next };
        var step2 = new MockStep { Result = StepResult.Previous }; // Will cause back navigation first time
        var step3 = new MockStep { Result = StepResult.Next };
        var orchestrator = new WizardOrchestrator(new[] { step1, step2, step3 });

        // Simulate a back button that gets pressed once, then the user hits Next.
        bool step2FirstRun = true;
        var dynamicStep2 = new MockStep();
        dynamicStep2.ExecuteCount = 0;
        var dynamicExecuteAsync = async (WizardContext ctx) =>
        {
            dynamicStep2.ExecuteCount++;
            if (step2FirstRun)
            {
                step2FirstRun = false;
                return StepResult.Previous;
            }

            return StepResult.Next;
        };

        // Redefine step2 using dynamic mock
        var customStep2 = new CustomMockStep { OnExecute = dynamicExecuteAsync };

        orchestrator = new WizardOrchestrator(new IWizardStep[] { step1, customStep2, step3 });

        // Act
        var result = await orchestrator.ExecuteAsync(CreateContext());

        // Assert
        Assert.Equal(0, result);
        Assert.Equal(2, step1.ExecuteCount); // Executed once, then executed again after going back
        Assert.Equal(2, customStep2.ExecuteCount); // Executed first time (returns previous), executed second time (returns next)
        Assert.Equal(1, step3.ExecuteCount); // Executed at the end
    }

    [Fact]
    public async Task ExecuteAsync_Cancel_ShouldReturnEarlyWithErrorCode()
    {
        // Arrange
        var step1 = new MockStep { Result = StepResult.Next };
        var step2 = new MockStep { Result = StepResult.Cancel };
        var step3 = new MockStep { Result = StepResult.Next };
        var orchestrator = new WizardOrchestrator(new[] { step1, step2, step3 });

        // Act
        var result = await orchestrator.ExecuteAsync(CreateContext());

        // Assert
        Assert.Equal(1, result); // 1 typically means cancelled or error in CLI commands
        Assert.Equal(1, step1.ExecuteCount);
        Assert.Equal(1, step2.ExecuteCount);
        Assert.Equal(0, step3.ExecuteCount); // Never reached
    }

    private WizardContext CreateContext()
    {
        var settings = new Commands.WizardSettings();
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        return new WizardContext(settings, dir);
    }

    private class MockStep : IWizardStep
    {
        public bool ShouldEx { get; set; } = true;

        public StepResult Result { get; set; } = StepResult.Next;

        public int ExecuteCount { get; set; } = 0;

        public bool ShouldExecute(WizardContext context) => ShouldEx;

        public Task<StepResult> ExecuteAsync(WizardContext context)
        {
            ExecuteCount++;
            return Task.FromResult(Result);
        }
    }

    private class CustomMockStep : IWizardStep
    {
        public bool ShouldEx { get; set; } = true;

        public System.Func<WizardContext, Task<StepResult>> OnExecute { get; set; } = null!;

        public int ExecuteCount { get; set; } = 0;

        public bool ShouldExecute(WizardContext context) => ShouldEx;

        public async Task<StepResult> ExecuteAsync(WizardContext context)
        {
            ExecuteCount++;
            return await OnExecute(context);
        }
    }
}
