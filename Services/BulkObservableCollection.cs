using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace TubaWinUi3.Services;

public sealed class BulkObservableCollection<T> : ObservableCollection<T>
{
    private bool _suppressNotification;

    public BulkObservableCollection() { }

    public BulkObservableCollection(IEnumerable<T> collection) : base(collection) { }

    public void AddRange(IEnumerable<T> items)
    {
        if (items is null) return;
        _suppressNotification = true;
        foreach (var item in items)
            Items.Add(item);
        _suppressNotification = false;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressNotification)
            base.OnCollectionChanged(e);
    }
}
