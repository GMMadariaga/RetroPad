using System.Windows.Input;

namespace RetroPad.UI.Commands;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    private RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public static RelayCommand Create(Action execute) => new(_ => execute());

    public static RelayCommand Create(Action execute, Func<bool> canExecute)
        => new(_ => execute(), _ => canExecute());

    public static RelayCommand Create<T>(Action<T?> execute)
        => new(p => execute(p is T typed ? typed : default));

    public static RelayCommand CreateAsync(Func<Task> execute)
        => new(async _ => await execute());

    public static RelayCommand CreateAsync(Func<Task> execute, Func<bool> canExecute)
        => new(async _ => await execute(), _ => canExecute());
}
