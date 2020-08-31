using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace TextureArrayBaker.Editor
{
    public class TextureArrayBaker : EditorWindow
    {
        private GUIStyle smallStyle  = null;
        private GUIStyle bigStyle  = null;
        private GUIStyle buttonStyle = null;
        
        private string imageFolderPath;
        private string prevFolderPath;
        private string[] imageFilePaths;
        
        private float minFrame;
        private float maxFrame;
      
        public Texture2DArray textureArray;
        private string textureAssetName;
        private string textureAssetPath ;

        private bool foldout;
        
        private void OnEnable()
        {
            smallStyle = null;
            buttonStyle = null;
            bigStyle = null;
            imageFolderPath = null;
            imageFilePaths = null;
            textureArray = null;
        }

        [MenuItem("Uncharted Limbo Collective/Tools/Texture2D Array Baker")]
        private static void ShowWindow()
        {
            var window = GetWindow<TextureArrayBaker>();
            window.titleContent = new GUIContent("Texture2D Array Baker");
            window.Show();
        }

        
        private void OnGUI()
        {
            
            BuildStyles();
                         
            Header();

            SelectAndShowFolder();
            EditorGUILayout.Space();

            // Empty folder
            if (string.IsNullOrWhiteSpace(imageFolderPath)) return;
            
            LoadAndShowFiles(".jpg",".png");
            EditorGUILayout.Space();
            
            // Empty files
            if (imageFilePaths == null) return;
            
            LoadImageData();
            EditorGUILayout.Space();
            
            // Empty texture array
            if (textureArray == null) return;
            
            SaveAsset();
        }


        private void Header()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Texture2D Array Baker", bigStyle);
                      
            EditorGUILayout.Separator();

        }
        
        
        private void BuildStyles()
        {
            if (smallStyle == null)
            {
                smallStyle = new GUIStyle(GUI.skin.textField){fontSize = 9, fontStyle = FontStyle.Italic};
            }
        
            if (bigStyle == null)
            {
                bigStyle = new GUIStyle(GUI.skin.label){fontSize = 18, fontStyle = FontStyle.Bold, stretchHeight = true, fixedHeight = 60, alignment = TextAnchor.UpperLeft};
            }
                    
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button){fontSize = 15, fontStyle = FontStyle.Bold, fixedHeight = 40};
            }
        }

        
        private void SelectAndShowFolder()
        {
            if (GUILayout.Button("Select a folder containing frames!", buttonStyle))
            {
                imageFolderPath=  EditorUtility.OpenFolderPanel("Select a folder containing frames",   
                                                                prevFolderPath ?? Application.dataPath,  
                                                                prevFolderPath ?? Application.dataPath);
            }

            if (string.IsNullOrWhiteSpace(imageFolderPath))
                return;
            
            if (Directory.Exists(imageFolderPath))
            {
                prevFolderPath = imageFolderPath;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Folder Path", imageFolderPath);
                EditorGUI.EndDisabledGroup();
            }
            else 
            {
                EditorGUILayout.HelpBox("The directory is invalid", MessageType.Error);
            }
        }
       
        
        private void LoadAndShowFiles(params string[] extensions)
        {
            if (!Directory.Exists(imageFolderPath)) 
                return;
            
            // Get filenames if button pressed
            if (GUILayout.Button("Try Load Images!", buttonStyle))
                imageFilePaths = CheckForFiles(imageFolderPath, ".jpg", ".png");
         
            // If no array was yet created return
            if (imageFilePaths == null)
                return;
           
           // No files found 
            if (imageFilePaths.Length == 0)
            {
                EditorGUILayout.HelpBox("No files of the specified types exist", MessageType.Warning);
                return;
            }

            // Show 10 file names
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "File Names");
           
            if (foldout)
            {
                var min = Mathf.Min(10, imageFilePaths.Length);
                    
                for (var i = 0; i < min; i++)
                {
                    EditorGUILayout.LabelField(Path.GetFileName(imageFilePaths[i]), smallStyle);   
                }

                if (imageFilePaths.Length > 10)
                {
                    EditorGUILayout.LabelField($"+ {imageFilePaths.Length -10} more files...", new GUIStyle(GUI.skin.label){fontStyle = FontStyle.Bold});   
                }
            }
          
            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        private void LoadImageData()
        {
            if (imageFilePaths == null)
            {
                return;
            }
            
            EditorGUILayout.LabelField("Frame Range");
            EditorGUILayout.BeginHorizontal();
           
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField((int) minFrame, GUILayout.Width(60) );
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.MinMaxSlider(ref minFrame, ref maxFrame, 0, imageFilePaths.Length);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField((int) maxFrame,GUILayout.Width(60));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();


            if (GUILayout.Button("Merge Selected Frame Range into a Texture2D array", buttonStyle))
            {
                if (maxFrame - minFrame > 2048)
                {
                    maxFrame = minFrame + 2048;
                }
            
                // Create temporary textures in memory
                var tex       = new Texture2D[(int)maxFrame - (int)minFrame];
                var maxWidth  = Mathf.NegativeInfinity;
                var maxHeight = Mathf.NegativeInfinity;

                for (var i = (int)minFrame; i < (int)maxFrame; i++)
                {
                    var path      = Path.GetFullPath(imageFilePaths[i]);
                    var byteArray = File.ReadAllBytes(path);
                    tex[i] = new Texture2D(2, 2);
                    tex[i].LoadImage(byteArray);
                   
                    if ( tex[i].width > maxWidth)
                        maxWidth = tex[i].width;
                
                    if ( tex[i].height > maxHeight)
                        maxHeight = tex[i].height;
                }   
            
                // Generate TextureArray
                textureArray = new Texture2DArray((int)maxWidth, (int)maxHeight, (int)maxFrame-(int)minFrame, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
             
                for (var i = 0; i < tex.Length; i++)
                {
                    textureArray.SetPixels(tex[i].GetPixels(), i);
                }
                textureAssetName  = $"{tex.Length}_Merged_Textures";
                textureArray.name = textureAssetName;
                textureArray.Apply();  
            }
            
            EditorGUILayout.ObjectField(textureArray, typeof(Texture2DArray), false);

        }


        private void SaveAsset()
        {
            if (!GUILayout.Button("Save To Assets", buttonStyle)) 
                return;
            
            textureAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine("Assets", textureAssetName + ".asset"));
            AssetDatabase.CreateAsset(textureArray, textureAssetPath);
        }
      
        
        private static string[] CheckForFiles(string folderPath, params string[] extensions)
        {
            var dir = new DirectoryInfo(folderPath);
         
            if (extensions == null) 
                throw new ArgumentNullException(nameof(extensions));
            
            var files = dir.EnumerateFiles();
           
            return files.Where(f => extensions.Contains(f.Extension)).Select(f => f.FullName).ToArray();
        }
  
    }
}