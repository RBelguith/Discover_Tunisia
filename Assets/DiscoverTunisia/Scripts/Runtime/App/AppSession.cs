using DiscoverTunisia.Content;

namespace DiscoverTunisia.App
{
    public enum SelectionSource { Manual, Scan }

    /// <summary>The governorate the visitor picked, kept across scene loads. Nothing is stored on disk.</summary>
    public static class AppSession
    {
        public static GovernorateData Selected { get; private set; }
        public static SelectionSource Source { get; private set; }

        public static void Select(GovernorateData governorate, SelectionSource source)
        {
            Selected = governorate;
            Source = source;
            Core.AppState.Select(governorate.Id, source == SelectionSource.Scan ? "scan" : "manuel");
        }

        /// <summary>The selection, or the catalog's first governorate when a scene is opened on its own.</summary>
        public static GovernorateData Resolve(ContentCatalog catalog) => Selected != null ? Selected : catalog.Default;
    }
}
