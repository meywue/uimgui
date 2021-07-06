using ImGuiNET;
using System.Text;
using UImGui.Platform;
using UnityEditor;
using UnityEngine;

namespace UImGui.Editor
{
	[CustomEditor(typeof(UImGui))]
	internal class UImGuiEditor : UnityEditor.Editor
	{
		private SerializedProperty _doGlobalEvents;
		private SerializedProperty _camera;
		private SerializedProperty _renderFeature;
		private SerializedProperty _renderer;
		private SerializedProperty _platform;
		private SerializedProperty _enableInput;
		private SerializedProperty _initialConfiguration;
		private SerializedProperty _fontAtlasConfiguration;
		private SerializedProperty _iniSettings;
		private SerializedProperty _shaders;
		private SerializedProperty _style;
		private SerializedProperty _cursorShapes;
		private readonly StringBuilder _messages = new StringBuilder();

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			CheckRequirements();

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(_doGlobalEvents);
			if (RenderUtility.IsUsingURP())
			{
				EditorGUILayout.PropertyField(_renderFeature);
			}

			EditorGUILayout.PropertyField(_camera);
			EditorGUILayout.PropertyField(_renderer);
			EditorGUILayout.PropertyField(_platform);
			EditorGUILayout.PropertyField(_enableInput);
			EditorGUILayout.PropertyField(_initialConfiguration);
			EditorGUILayout.PropertyField(_fontAtlasConfiguration);
			EditorGUILayout.PropertyField(_iniSettings);
			EditorGUILayout.PropertyField(_shaders);
			EditorGUILayout.PropertyField(_style);
			EditorGUILayout.PropertyField(_cursorShapes);

			bool changed = EditorGUI.EndChangeCheck();
			if (changed)
			{
				serializedObject.ApplyModifiedProperties();
			}

			if (!Application.isPlaying) return;

			bool reload = GUILayout.Button("Reload");
			if (changed || reload)
			{
				(target as UImGui)?.Reload();
			}
		}

		private void OnEnable()
		{
			_doGlobalEvents = serializedObject.FindProperty("_doGlobalEvents");
			_camera = serializedObject.FindProperty("_camera");
			_renderFeature = serializedObject.FindProperty("_renderFeature");
			_renderer = serializedObject.FindProperty("_rendererType");
			_platform = serializedObject.FindProperty("_platformType");
			_enableInput = serializedObject.FindProperty("_enableInput");
			_initialConfiguration = serializedObject.FindProperty("_initialConfiguration");
			_fontAtlasConfiguration = serializedObject.FindProperty("_fontAtlasConfiguration");
			_iniSettings = serializedObject.FindProperty("_iniSettings");
			_shaders = serializedObject.FindProperty("_shaders");
			_style = serializedObject.FindProperty("_style");
			_cursorShapes = serializedObject.FindProperty("_cursorShapes");
		}

		private void CheckRequirements()
		{
			EditorGUILayout.LabelField("ImGUI Version: " + ImGui.GetVersion());
			EditorGUILayout.Space();

			_messages.Clear();
			if (_camera.objectReferenceValue == null)
			{
				_messages.AppendLine("Must assign a Camera.");
			}

			if (RenderUtility.IsUsingURP() && _renderFeature.objectReferenceValue == null)
			{
				_messages.AppendLine("Must assign a RenderFeature when using the URP.");
			}

			SerializedProperty configFlags = _initialConfiguration.FindPropertyRelative("ImGuiConfig");
			if (!PlatformUtility.IsAvailable((InputType)_platform.enumValueIndex))
			{
				_messages.AppendLine("Platform not available.");
			}
			else if ((InputType)_platform.enumValueIndex != InputType.InputSystem &&
				(configFlags.intValue & (int)ImGuiConfigFlags.NavEnableSetMousePos) != 0)
			{
				_messages.AppendLine("Will not work NavEnableSetPos with InputManager.");
			}

			if ((configFlags.intValue & (int)ImGuiConfigFlags.ViewportsEnable) != 0)
			{
				_messages.AppendLine("Unity hasn't support different viewports.");
			}

			if (_shaders.objectReferenceValue == null || _style.objectReferenceValue == null)
			{
				_messages.AppendLine("Must assign a Shader Asset and a Style Asset in configuration section.");
			}

			if (_messages.Length > 0)
			{
				EditorGUILayout.HelpBox(_messages.ToString(), MessageType.Error);
			}
		}
	}
}
