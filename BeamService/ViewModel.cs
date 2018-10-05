using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BeamService
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        /// <summary>Событие возникает, когда модель меняет значение своего свойства</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Генерация события изменения значения свойства модели</summary>
        /// <param name="propertyName">Имя изменившегося свойства (оставить = null для автоматического выбора)</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>Установка значения поля данных свойства</summary>
        /// <typeparam name="T">Тип значения свойства</typeparam>
        /// <param name="field">Ссылка на поле данных свойства</param>
        /// <param name="value">новое значение свойства</param>
        /// <param name="property">Имя изменяемого свойства (оставить = null для автоматического выбора)</param>
        /// <returns>Истина, если значение свойства было изменено</returns>
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string property = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(property);
            return true;
        }
    }
}