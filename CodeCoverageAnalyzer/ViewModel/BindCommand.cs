using System.Windows.Input;

namespace CoverageAnalyzer;

/// <summary>
/// A command class that implements the ICommand interface, used for binding commands to UI elements in MVVM.
/// </summary>
public class BindCommand : ICommand {
   #region Members
   /// <summary>
   /// The action to execute when the command is invoked with a parameter.
   /// </summary>
   private readonly Action<object>? mExecuteWithParameter;

   /// <summary>
   /// The action to execute when the command is invoked without a parameter.
   /// </summary>
   private readonly Action? mExecuteWithoutParameter;

   /// <summary>
   /// A predicate to determine if the command can execute.
   /// </summary>
   private readonly Predicate<object>? mCanExecute;
   #endregion

   #region Constructor(s)
   /// <summary>
   /// Initializes a new instance of the <see cref="BindCommand"/> class.
   /// </summary>
   /// <param name="execute">The action to execute when the command is invoked with a parameter.</param>
   /// <param name="canExecute">A predicate to determine if the command can execute. Optional.</param>
   /// <exception cref="ArgumentNullException">Thrown if the execute action is null.</exception>
   public BindCommand (Action<object> execute, Predicate<object>? canExecute = null) {
      mExecuteWithParameter = execute ?? throw new ArgumentNullException (nameof (execute));
      mCanExecute = canExecute;
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="BindCommand"/> class.
   /// </summary>
   /// <param name="execute">The action to execute when the command is invoked without a parameter.</param>
   /// <param name="canExecute">A predicate to determine if the command can execute. Optional.</param>
   /// <exception cref="ArgumentNullException">Thrown if the execute action is null.</exception>
   public BindCommand (Action execute, Predicate<object>? canExecute = null) {
      mExecuteWithoutParameter = execute ?? throw new ArgumentNullException (nameof (execute));
      mCanExecute = canExecute;
   }
   #endregion

   #region ICommand Implementation
   /// <summary>
   /// Determines whether the command can execute in its current state.
   /// </summary>
   /// <param name="parameter">Data used by the command. If the command does not require data, this object can be set to null.</param>
   /// <returns>true if this command can be executed; otherwise, false.</returns>
   public bool CanExecute (object? parameter) {
      if (mCanExecute == null) return true;
      return parameter != null && mCanExecute (parameter);
   }

   /// <summary>
   /// Executes the command.
   /// </summary>
   /// <param name="parameter">Data used by the command. If the command does not require data, this object can be set to null.</param>
   public void Execute (object? parameter) {
      if (mExecuteWithParameter != null && parameter != null) mExecuteWithParameter (parameter);
      else mExecuteWithoutParameter?.Invoke ();
   }

   /// <summary>
   /// Occurs when changes occur that affect whether or not the command should execute.
   /// </summary>
   public event EventHandler? CanExecuteChanged {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
   }
   #endregion
}

/// <summary>
/// A generic command class that implements the ICommand interface, used for binding commands to UI elements in MVVM.
/// </summary>
/// <typeparam name="T">The type of the parameter passed to the command.</typeparam>
public class BindCommand<T> (Action<T> execute, Predicate<T>? canExecute = null) : ICommand {
   #region Members
   /// <summary>
   /// The action to execute when the command is invoked.
   /// </summary>
   private readonly Action<T> mExecute = execute ?? throw new ArgumentNullException (nameof (execute));

   /// <summary>
   /// A predicate to determine if the command can execute.
   /// </summary>
   private readonly Predicate<T>? mCanExecute = canExecute;
   #endregion

   #region ICommand Implementation
   /// <summary>
   /// Determines whether the command can execute in its current state.
   /// </summary>
   /// <param name="parameter">Data used by the command. If the command does not require data, this object can be set to null.</param>
   /// <returns>true if this command can be executed; otherwise, false.</returns>
   public bool CanExecute (object? parameter) {
      if (mCanExecute == null) return true;
      return parameter is T typedParameter && mCanExecute (typedParameter);
   }

   /// <summary>
   /// Executes the command.
   /// </summary>
   /// <param name="parameter">Data used by the command. If the command does not require data, this object can be set to null.</param>
   public void Execute (object? parameter) {
      if (parameter is T typedParameter) mExecute (typedParameter);
   }

   /// <summary>
   /// Occurs when changes occur that affect whether or not the command should execute.
   /// </summary>
   public event EventHandler? CanExecuteChanged {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
   }
   #endregion
}