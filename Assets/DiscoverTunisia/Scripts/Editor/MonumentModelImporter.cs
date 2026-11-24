using UnityEditor;

namespace DiscoverTunisia.EditorTools
{
    /// <summary>Import rules for the monument models exported by Tools/Blender/monuments.py.</summary>
    public sealed class MonumentModelImporter : AssetPostprocessor
    {
        public const string Folder = "Assets/DiscoverTunisia/Art/Models/";

        public override uint GetVersion() => 1;

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Folder)) return;
            var importer = (ModelImporter)assetImporter;

            // Blender already bakes the axis conversion and writes meters.
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.bakeAxisConversion = false;

            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = false;
            importer.importBlendShapes = false;
            importer.importConstraints = false;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.addCollider = false;

            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.None;
            importer.generateSecondaryUV = false;

            // Material slots are named after the assets in Art/Materials, so the models share the project palette.
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.RecursiveUp);
        }
    }
}
