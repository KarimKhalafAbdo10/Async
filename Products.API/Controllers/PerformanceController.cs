using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Employees;

namespace Products.API.Controllers;

/// <summary>
/// Compares running two independent I/O-bound operations sequentially vs in parallel.
/// Each parallel task gets its own DI scope because a DbContext is not thread-safe.
/// </summary>
[ApiController]
[Route("api/performance")]
public class PerformanceController(IServiceScopeFactory scopes, ILogger<PerformanceController> logger) : ControllerBase
{
    public record StepResult(string Name, long Ms);
    public record RunResult(string Mode, long TotalMs, List<StepResult> Steps);
    public record ComparisonResult(int DelayAMs, int DelayBMs, RunResult Sequential, RunResult Parallel,
        long SavedMs, double Speedup, bool ParallelFaster, string Report);

    [HttpGet("compare")]
    public async Task<ActionResult<ComparisonResult>> Compare(int delayA = 400, int delayB = 600, CancellationToken ct = default)
    {
        delayA = Math.Clamp(delayA, 0, 5000);
        delayB = Math.Clamp(delayB, 0, 5000);

        // Operation A: load employees + simulated call to an external payroll service.
        // Operation B: build department statistics + simulated call to a reporting service.
        async Task<StepResult> OpA(CancellationToken token)
        {
            var sw = Stopwatch.StartNew();
            await using var scope = scopes.CreateAsyncScope();
            var svc = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
            var all = await svc.GetAllAsync(token);
            await Task.Delay(delayA, token);
            return new StepResult($"A: تحميل {all.Count} موظف + خدمة الرواتب", sw.ElapsedMilliseconds);
        }

        async Task<StepResult> OpB(CancellationToken token)
        {
            var sw = Stopwatch.StartNew();
            await using var scope = scopes.CreateAsyncScope();
            var svc = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
            var stats = (await svc.GetAllAsync(token)).GroupBy(e => e.Department).Count();
            await Task.Delay(delayB, token);
            return new StepResult($"B: إحصاءات {stats} أقسام + خدمة التقارير", sw.ElapsedMilliseconds);
        }

        var seqWatch = Stopwatch.StartNew();
        var seqSteps = new List<StepResult> { await OpA(ct), await OpB(ct) };
        seqWatch.Stop();

        var parWatch = Stopwatch.StartNew();
        var parSteps = await Task.WhenAll(OpA(ct), OpB(ct));
        parWatch.Stop();

        var seq = new RunResult("Sequential", seqWatch.ElapsedMilliseconds, seqSteps);
        var par = new RunResult("Parallel", parWatch.ElapsedMilliseconds, parSteps.ToList());
        var saved = seq.TotalMs - par.TotalMs;
        var speedup = par.TotalMs == 0 ? 0 : Math.Round((double)seq.TotalMs / par.TotalMs, 2);
        var faster = saved > 0;

        var report = faster
            ? $"التنفيذ بالتتابع استغرق {seq.TotalMs}ms (مجموع الزمنين تقريباً)، بينما التنفيذ بالتوازي بـ Task.WhenAll استغرق {par.TotalMs}ms (أطول عملية فقط). " +
              $"وفّر التوازي {saved}ms أي تسريع {speedup}x، لأن العمليتين مستقلتان وتنتظران I/O فيمكن تداخل انتظارهما. " +
              "يُشترط أن تكون العمليات مستقلة وأن يكون لكل عملية DbContext خاص بها."
            : $"لم يتحسن الأداء بشكل ملحوظ (تتابع {seq.TotalMs}ms مقابل توازي {par.TotalMs}ms). يحدث ذلك عندما تكون العمليات سريعة جداً أو غير I/O-bound.";

        logger.LogInformation("Performance comparison: sequential {SeqMs} ms vs parallel {ParMs} ms ({Speedup}x)",
            seq.TotalMs, par.TotalMs, speedup);

        return new ComparisonResult(delayA, delayB, seq, par, saved, speedup, faster, report);
    }
}
