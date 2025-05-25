namespace NeptunKiller.SubjectService.Helpers;

public class Result<TResult, TError> where TError : Exception
{
    private readonly TResult _result;
    private readonly TError _error;
    private readonly bool _success;

    private Result(TResult result)
    {
        _result = result;
        _error = null;
        _success = true;
    }

    private Result(TError error)
    {
        _result = default;
        _error = error;
        _success = false;
    }

    public static implicit operator Result<TResult, TError>(TResult result) => new(result);

    public static implicit operator Result<TResult, TError>(TError error) => new(error);
    
    public bool TryGetResult(out TResult result)
    {
        if (_success)
        {
            result = _result;
            return true;
        }

        result = default;
        return false;
    }
    
    public bool TryGetError(out TError error)
    {
        if (!_success)
        {
            error = _error;
            return true;
        }

        error = null;
        return false;
    }

    public void Match(Action<TResult> onSuccess, Action<TError> onFailure)
    {
        if (_success)
        {
            onSuccess(_result);
            return;
        }
        
        onFailure(_error);
    }

    public T Match<T>(Func<TResult, T> onSuccess, Func<TError, T> onFailure)
    {
        if (_success)
        {
            return onSuccess(_result);
        }
        
        return onFailure(_error);
    }
    
    public Task MatchAsync(Func<TResult, Task> onSuccess, Func<TError, Task> onFailure)
    {
        if (_success)
        {
            return onSuccess(_result);
        }
        
        return onFailure(_error);
    }
    
    public Task<T> MatchAsync<T>(Func<TResult, Task<T>> onSuccess, Func<TError, Task<T>> onFailure)
    {
        if (_success)
        {
            return onSuccess(_result);
        }
        
        return onFailure(_error);
    }
}