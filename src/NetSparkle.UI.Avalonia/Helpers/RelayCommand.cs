using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetSparkleUpdater.UI.Avalonia.Helpers
{
    /// <summary>
    /// Command (<see cref="Action"/>) that can be executed based on an optional
    /// function that says whether or not the command can be run. This command
    /// can send a parameter to the <see cref="Action"/> to execute and the 
    /// <see cref="Predicate{T}"/> that checks whether or not the <see cref="Action"/>
    /// should execute in the first place.
    /// <para>
    /// https://gist.github.com/schuster-rainer/2648922 with some modifications
    /// </para>
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T?> _execute;
        readonly Predicate<T?>? _canExecute;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a <see cref="RelayCommand"/> with a given <see cref="Action"/>
        /// and no optional function for whether or not the <see cref="RelayCommand"/>
        /// can execute
        /// </summary>
        /// <param name="execute"><see cref="Action"/> to call when this <see cref="RelayCommand"/>
        /// is executed</param>
        public RelayCommand(Action<T?> execute)
        : this(execute, null)
        {
        }

        /// <summary>
        /// Create a <see cref="RelayCommand"/> with a given <see cref="Action"/>
        /// and optional function for whether or not the <see cref="RelayCommand"/>
        /// can execute
        /// </summary>
        /// <param name="execute"><see cref="Action"/> to call when this <see cref="RelayCommand"/>
        /// is executed</param>
        /// <param name="canExecute"><see cref="Predicate{T}"/> function that determines whether or not the given <see cref="Action"/>
        /// should be executed or not</param>
        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("Execute parameter cannot be null");
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        /// <summary>
        /// Determine whether or not this command shoudl execute
        /// </summary>
        /// <param name="parameter">Nullable object that will be sent to this
        /// command's function <see cref="Predicate{T}"/></param>
        /// <returns>True if the command should run, false if the command should not be run</returns>
        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null ? true : _canExecute((T?)parameter);
        }

        /// <summary>
        /// Event that is called when the command may have had its ability to be executed changed
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add {}//{ CommandManager.RequerySuggested += value; }
            remove {}//{ CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Execute this command
        /// </summary>
        /// <param name="parameter">the nullable object parameter to send to the execution <see cref="Action"/></param>
        public void Execute(object? parameter) => _execute((T?)parameter);

        #endregion
    }

    /// <summary>
    /// Command (<see cref="Action"/>) that can be executed based on an optional
    /// function that says whether or not the command can be run
    /// </summary>
    public class RelayCommand : RelayCommand<object>
    {
        /// <summary>
        /// Create a <see cref="RelayCommand"/> with a given <see cref="Action"/>
        /// and no optional function for whether or not the <see cref="RelayCommand"/>
        /// can execute
        /// </summary>
        /// <param name="execute"><see cref="Action"/> to call when this <see cref="RelayCommand"/>
        /// is executed</param>
        public RelayCommand(Action execute) : this(execute, null) { }

        /// <summary>
        /// Create a <see cref="RelayCommand"/> with a given <see cref="Action"/>
        /// and optional function for whether or not the <see cref="RelayCommand"/>
        /// can execute
        /// </summary>
        /// <param name="execute"><see cref="Action"/> to call when this <see cref="RelayCommand"/>
        /// is executed</param>
        /// <param name="canExecute">Function that determines whether or not the given <see cref="Action"/>
        /// should be executed or not</param>
        public RelayCommand(Action execute, Func<bool>? canExecute)
            : base(param => execute?.Invoke(),
                   param => (canExecute?.Invoke()) ?? true) { }
    }
}
