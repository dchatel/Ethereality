using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Ethereality
{
    public class Dialog : INotifyPropertyChanged
    {
        readonly CancellationTokenSource cts;
        bool? dialogResult;

        public object Content { get; }

        public Dialog(object content)
        {
            cts = new();
            dialogResult = false;

            Content = content;
        }

        public bool IsCurrent => Dialogs.Last() == this;

        public static ICommand OkCommand { get; } = new RelayCommand(() => CloseDialog(true));
        public static ICommand CancelCommand { get; } = new RelayCommand(() => CloseDialog(false));
        public static void CloseDialog(bool result)
        {
            var dialog = Dialogs.Last();
            Dialogs.Remove(dialog);
            if (Dialogs.Any() == false)
                OnStaticPropertyChanged(nameof(HasOpenDialog));
            foreach (var d in Dialogs)
                d.OnPropertyChanged(nameof(IsCurrent));
            dialog.dialogResult = result;
            dialog.cts.Cancel();
        }

        public static async Task<bool?> Show(object content)
        {
            var dialog = new Dialog(content);
            Dialogs.Add(dialog);
            OnStaticPropertyChanged(nameof(HasOpenDialog));
            foreach (var d in Dialogs)
                d.OnPropertyChanged(nameof(IsCurrent));
            try
            {
                await Task.Delay(-1, dialog.cts.Token);
            }
            catch (TaskCanceledException) { }
            return dialog.dialogResult;
        }

        public static ObservableCollection<Dialog> Dialogs { get; } = new();

        public static bool HasOpenDialog => Dialogs.Any();

        public static event EventHandler<PropertyChangedEventArgs>? StaticPropertyChanged;
        private static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null!)
            => StaticPropertyChanged?.Invoke(null, new(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new(propertyName));
    }
}
