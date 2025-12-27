using System;

namespace MainApplication.ViewModels.Service
{
    public interface IDateTimeEditorService
    {
        DateTime? EditDateTime(DateTime? initial, Func<DateTime?, bool> validate);
    }
}
