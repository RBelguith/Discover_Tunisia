using UnityEditor;

namespace DiscoverTunisia.EditorTools
{
    /// <summary>Import rules for the identity card photos that the scan screen tracks with Vuforia.</summary>
    public sealed class IdCardImporter : AssetPostprocessor
    {
        public const string Folder = "Assets/DiscoverTunisia/Settings/Resources/IdCards/";

        public override uint GetVersion() => 1;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
