using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SistemaDocente.Presentation;

public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Any(e => e.Value.Count > 0);

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName) || !_errors.TryGetValue(propertyName, out var errors))
        {
            return Enumerable.Empty<string>();
        }

        return errors;
    }

    protected void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out var errors))
        {
            errors = new List<string>();
            _errors[propertyName] = errors;
        }

        if (!errors.Contains(error))
        {
            errors.Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    protected void RemoveError(string propertyName, string error)
    {
        if (_errors.TryGetValue(propertyName, out var errors) && errors.Remove(error))
        {
            OnErrorsChanged(propertyName);
        }
    }

    protected void ClearErrors(string propertyName)
    {
        if (_errors.TryGetValue(propertyName, out var errors) && errors.Count > 0)
        {
            errors.Clear();
            OnErrorsChanged(propertyName);
        }
    }

    protected void ClearAllErrors()
    {
        foreach (var propertyName in _errors.Keys.ToList())
        {
            if (_errors[propertyName].Count > 0)
            {
                _errors[propertyName].Clear();
                OnErrorsChanged(propertyName);
            }
        }
    }

    protected virtual void OnErrorsChanged([CallerMemberName] string? propertyName = null) =>
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName))
        {
            return false;
        }

        onChanged();
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}