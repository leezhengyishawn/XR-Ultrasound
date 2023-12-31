using UnityEngine;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityImporters = UnityEditor.AssetImporters;
#else
using UnityImporters = UnityEditor.Experimental.AssetImporters;
#endif
using System.IO;
using System.Linq;
using Pixyz.RuleEngine.Editor;
using Pixyz.Commons.Extensions;
using Pixyz.Commons.Extensions.Editor;
using Pixyz.Plugin4Unity.Core;
using Pixyz.Plugin4Unity;

namespace Pixyz.ImportSDK {

    public class ScriptedImporterAttribute : UnityImporters.ScriptedImporterAttribute
    {
        public static string[] SupportedFormats {
            get {
                //To force constructor call for NativeInterface that will initialize PixyzAPI
                _ = API.Native.NativeInterface.PiXYZAPI_dll;

                if (Preferences.ImportFilesInAssets) {
                    return Formats.SupportedFormatsScriptedImporter;
                } else {
                    return new string[0];
                }
            }
        }

        public ScriptedImporterAttribute() : base(1, SupportedFormats) {
           
        }
    }

    [ScriptedImporter]
    public class ScriptedImporter : UnityImporters.ScriptedImporter {

        public ImportSettings importSettings;
        public RuleSet rules;

        private UnityImporters.AssetImportContext _context;
        public string file => _context?.assetPath;

        public override void OnImportAsset(UnityImporters.AssetImportContext context) {

            // Context is deleted after. Keep some references.
            _context = context;

            // Security : we ignore Pixyz binaries, which may contains file with extensions that could be trigger the scripted importer
            if (context.assetPath.Contains("/Binaries/")) {
                return;
            }

            // Only import if import settings are assigned to prevent unwanted file loading
            if (importSettings == null) {
                Debug.LogWarning($"The file '{context.assetPath}' can be imported by Pixyz.\nSelect it and assign Import Settings to import the file.");
                return;
            }

            if (!Configuration.CheckLicense()) {
                Debug.LogWarning($"A Pixyz license is required to import '{context.assetPath}'");
                return;
            }

            string fileToImport = context.assetPath;
            if (Formats.GetExtension2(context.assetPath) == ".plmxml") {
                // copy .plmxml outside of the project, and import this file
                // importer won't find any geometry and will only import product structure
                // we'll then instanciate the geometries one by one in a post process
                fileToImport = Path.Combine(Path.GetTempPath(), "tmp.plmxml");
                File.Copy(context.assetPath, fileToImport, true);
            }

            Selection.objects = new Object[0];

            Importer importer = new Importer(Path.GetFullPath(fileToImport), importSettings);
            importer.printMessageOnCompletion = Preferences.LogImportTime;
            importer.isAsynchronous = false; // The ScriptedImporter doesn't seems to work with Async stuff. As soon as it gets out of OnImportAsset, context reference is lost.
            importer.completed += importEnded;
            importer.progressed += importProgressed;
            importer.run();
        }

        private void importEnded(GameObject gameObject) {

            try {
                if (rules != null) {
                    rules.progressed = delegate { };
                    rules.progressed += rulesProgressed;
                    rules.run(true);
                }
            } catch(System.Exception exception) {
                Debug.LogException(exception);
            }

            EditorUtility.ClearProgressBar();

            // Names have to be unique to avoid warnings...
            // Now we mimick Unity's fbx scripted importer behavior
            var uniqueNames = new System.Collections.Generic.HashSet<string>();
            string GetUniqueName(string guid)
            {
                int i = 1;
                string currentGuid = guid;
                while (!uniqueNames.Add(currentGuid)) {
                    currentGuid = guid + ' ' + i;
                    i++;
                }
                return currentGuid;
            }

            //If file is a plmxml file, we run through a special process to attempt linking dependencies
            if (Formats.GetExtension2(file) == ".plmxml")
            {
                foreach (GameObject go in gameObject.GetChildren(true, true))
                {
                    Metadata metadata = go.GetComponent<Metadata>();
                    if (!metadata || !metadata.containsProperty("RepresentationLocation"))
                        continue;
                    // The metadata property "RepresentationLocation" contains the location of a dependency
                    string path = metadata.getProperty("RepresentationLocation").TrimStart('.', '/', '\\');
                    // Get path path relative to project
                    path = Path.Combine(Path.GetDirectoryName(file), path);
                    string absPath = Path.GetFullPath(path);
                    if (!File.Exists(absPath))
                    {
                        // The dependency was not found (the file is not present or not a the right place)
                        // We try looking for alternatives (files with the same name but different extensions)
                        string directory = Path.GetDirectoryName(path);
                        string[] alternatives = !Directory.Exists(directory) ? new string[0] : new DirectoryInfo(directory)
                            .GetFiles(Path.GetFileNameWithoutExtension(path) + "*")
                            .Select(x => x.Name)
                            .Where(x => Formats.IsFileSupported(x))
                            .ToArray();
                        if (alternatives.Length == 0)
                        {
                            Debug.LogWarning($"File '{path}' was not found. No alternatives (files with the same name but a different extension) were found either.");
                            continue;
                        }
                        else if (alternatives.Length > 1)
                        {
                            Debug.LogWarning($"File '{path}' was not found. Multiple alternatives were found, the first one will be used ({alternatives[0]}).");
                        }
                        // We use the first alternative found
                        path = Path.Combine(Path.GetDirectoryName(path), alternatives[0]);
                    }
                    // We try getting the prefab associated with the file (if any)
                    GameObject nestedObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (!nestedObject)
                    {
                        Debug.LogWarning($"File '{path}' haven't been imported yet");
                        continue;
                    }
                    // We instantiate the prefab and link it to the plmxml
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(nestedObject);
                    instance.TryGetComponent<ImportStamp>(out ImportStamp importStamp);
                    if (importStamp != null)
                    {
                        // File was imported by Pixyz. Cancel import transformation and assign parent.
                        instance.transform.rotation = Quaternion.identity;
                        instance.transform.localScale = Vector3.one;
                        instance.transform.SetParent(go.transform, false);
                    }
                    else
                    {
                        // Prefab was imported with another importer than Pixyz (likely Unity fbx importer)
                        // Apply a transformation around pivot point to be back in original coordinate system
                        GameObject pivot = new GameObject("Pixyz pivot");
                        pivot.transform.position = Vector3.zero;
                        instance.transform.SetParent(pivot.transform, true);
                        pivot.transform.SetParent(go.transform, false);
                        go.transform.localScale = new Vector3(-1, 1, 1);
                        instance.transform.SetParent(go.transform, true);
                        GameObject.DestroyImmediate(pivot);
                    }
                }
            }

            // ScriptedImporter-only. This is mandatory to avoid warnings.
            foreach (GameObject go in gameObject.GetChildren(true, true)) {
                go.name = GetUniqueName(go.name);
            }

            //_context.AddObjectToAsset(gameObject.name, gameObject, Resources.Load<Texture2D>("PixyzPrefabModel"));
            _context.AddObjectToAsset(gameObject.name, gameObject, EditorGUIUtility.IconContent((EditorGUIUtility.isProSkin ? "d_" : "") + "PrefabModel Icon").image as Texture2D);

            ImportStamp stamp = gameObject.GetComponent<ImportStamp>();
            _context.AddObjectToAsset("importSettings", stamp.importSettings);

            Object[] volatileDependencies = gameObject.GetNonPersistentDependenciesRecursive();
            foreach (Object dependency in volatileDependencies) {
                _context.AddObjectToAsset(GetUniqueName(dependency.name), dependency);
            }

            _context.SetMainObject(gameObject);
        }

        private void importProgressed(float progress, string message) {
            if (progress >= 1f)
                EditorUtility.ClearProgressBar();
            else
                EditorUtility.DisplayProgressBar($"Importing \"{Path.GetFileName(_context.assetPath)}\"", message, progress);
        }

        private void rulesProgressed(float progress, string message) {
            if (progress >= 1f)
                EditorUtility.ClearProgressBar();
            else
                EditorUtility.DisplayProgressBar("Rule Engine", message, progress);
        }
    }
}
