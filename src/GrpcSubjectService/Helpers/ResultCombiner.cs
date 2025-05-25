namespace NeptunKiller.SubjectService.Helpers;

public class ResultCombiner<TResult, TError> where TError : Exception
{
    private readonly List<(Func<Task<Result<TResult, TError>>>, Func<TResult, Task>)> _toExecute;

    public ResultCombiner()
    {
        _toExecute = [];
    }
    
    public ResultCombiner<TResult, TError> AddRun(Func<Task<Result<TResult, TError>>> func, Func<TResult, Task> onSuccess)
    {
        _toExecute.Add((func, onSuccess));
        return this;
    }

    public async Task<TError> ExecuteAsync()
    {
        foreach (var (func, onSuccess) in _toExecute)
        {
            var result = await func();

            if (result.TryGetError(out var error))
            {
                return error;
            }
            
            if (result.TryGetResult(out var value))
            {
                await onSuccess(value);
            }
        }

        return null;
    }
}