using MainApplication.Views;
using System;
using System.Linq;
using System.Windows;

namespace MainApplication.ViewModels.Service
{
    public class DateTimeEditorService : IDateTimeEditorService
    {
        public DateTime? EditDateTime(DateTime? initial, Func<DateTime?, bool> validate)
        {
            var window = new DateTimeEditorWindow(initial, validate)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            var result = window.ShowDialog();

            if (result == true && window.DataContext is DateTimeEditorViewModel vm)
            {
                /* OK → Result = Composed */
                /* クリア → Result = null */
                return vm.Result;
            }

            /* キャンセル → initial */
            return initial;
        }
    }
}
