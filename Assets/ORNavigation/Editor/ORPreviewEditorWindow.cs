using UnityEngine;
using UnityEditor;
using System.Collections;

public enum ORPreviewState {
	Playing,
	Paused,
	Stopped
}

[ExecuteInEditMode]
public class ORPreviewEditorWindow : EditorWindow {
	
	private static int WindowWidth = 250;
	private static int WindowHeight = 380;
	private static string PreviewCameraName = "ORNavigationPreviewCamera";
	private static int TextureDepth	= 16;
	
    [MenuItem("Window/ORNavigation")]
    public static void Init() {
    	
        EditorWindow previewEditorWindow = GetWindow<ORPreviewEditorWindow>(false, "ORNavigation Preview");
        previewEditorWindow.minSize = new Vector2(WindowWidth, WindowHeight);
        previewEditorWindow.maxSize = new Vector2(WindowWidth, WindowHeight);
        previewEditorWindow.autoRepaintOnSceneChange = true;
        previewEditorWindow.wantsMouseMove = false;
        
    }
    
    private Camera _previewCamera;
    private RenderTexture _renderTexture;
    private int _textureWidth;
    private int _textureHeight;
    private string[] _navigationItems;
    private ORNavigation[] _navigationsFound;
    private int _selectedNavigationIndex;
    private ORPreviewState _state;
    private float _t = 0;
    private float _duration = 5f;
    
    public ORNavigation selectedNavigation {
    	get {
    		
    		if (_navigationsFound == null || _selectedNavigationIndex < 0 || _selectedNavigationIndex >= _navigationsFound.Length)
    			return null;
    			
    		return _navigationsFound[_selectedNavigationIndex];
    	
    	}
    }
    
    // MonoBehaviour

    private void Awake() {
    	
    	_textureWidth = 250;
    	_textureHeight = 250;
    	
    	_selectedNavigationIndex = 0;
    	_state = ORPreviewState.Stopped;
    	
    	CreateRenderTexture();
    	CreatePreviewCamera();
    	
    	ReloadNavigationsList();
    	
    }
    
    private void OnDestroy() {
    	DestroyPreviewCamera();
    	DestroyRenderTexture();
    }
    
    private void OnGUI() {
    	
    	GUILayout.Label("Choose a navigation:");
    	_selectedNavigationIndex = EditorGUILayout.Popup(_selectedNavigationIndex, _navigationItems);
    	
    	_t = EditorGUILayout.Slider("Seek:", _t, 0f, 1f);
    	_duration = EditorGUILayout.Slider("Preview Speed:", _duration, .1f, 10f);
    	
    	EditorGUILayout.BeginHorizontal();
    	
    		if (_state == ORPreviewState.Playing) {
	    	
		    	if (GUILayout.Button("Pause"))
		    		Pause();
		    		
    		} else {
	    	
		    	if (GUILayout.Button("Play"))
		    		Play();
		    		
    		}
	    		
	    	if (GUILayout.Button("Stop"))
	    		Stop();
	    		
    	EditorGUILayout.EndHorizontal();

    	EditorGUILayout.Space();
    	GUILayout.Label("Preview:");
    	
    	if (_renderTexture != null)
	    	GUI.DrawTexture(new Rect(0, 130, _textureWidth, _textureHeight), _renderTexture, ScaleMode.ScaleToFit, false, 1.0f);
    	
    }
    
    private void Update() {

    	if (_previewCamera == null || !_previewCamera.gameObject)
    		CreatePreviewCamera();
    	
    	ORNavigation currentNavigation = selectedNavigation;

    	if (currentNavigation) {
    		
	    	if (_state == ORPreviewState.Playing)
	    		_t += currentNavigation.GetTIncrement(ORNavigationDirection.Normal, (1f / 100f) * _duration);
	    		
	    	if (_t > 1)
	    		_t = 1;

	    	currentNavigation.SetOnPath(_previewCamera.transform, _t);
	    	
		    _previewCamera.Render();
		    
		    if (_t >= 1)    	
		    	_state = ORPreviewState.Stopped;
		    
    	}
	    	
    }
    
    private void OnFocus() {
    	ReloadNavigationsList();
    }
    
    private void OnLostFocus() {
    	Pause();
    }
    
    // Private
    
    private void CreateRenderTexture() {
    	
    	DestroyRenderTexture();
    	
    	_renderTexture = new RenderTexture(_textureWidth, _textureHeight, TextureDepth);
    	
    }
    
    private void CreatePreviewCamera() {
    	
    	DestroyPreviewCamera();
    	
    	_previewCamera = FindPreviewCamera();
    	
    	if (_previewCamera == null) {
    	
	    	GameObject aGameObject = new GameObject(PreviewCameraName);
	    	_previewCamera = aGameObject.AddComponent<Camera>();
	    	
    	}
	    
	    _previewCamera.enabled = false;
	    _previewCamera.targetTexture = _renderTexture;
    	
    }
    
    private void DestroyRenderTexture() {
    	
    	if (_renderTexture != null)
    		DestroyImmediate(_renderTexture);
    		
    }
    
    private void DestroyPreviewCamera() {
    	
    	if (_previewCamera && _previewCamera.gameObject)
    		DestroyImmediate(_previewCamera.gameObject);
    		
    }
    
    private Camera FindPreviewCamera() {
    	
    	foreach (Camera aCamera in GameObject.FindObjectsOfType(typeof(Camera)))
    		if (aCamera.name == PreviewCameraName)
    			return aCamera;
    	
    	return null;
    	
    }
    
    private void ReloadNavigationsList() {
    	
    	ORNavigation currentNavigation = selectedNavigation;
    	
    	_navigationsFound = GameObject.FindObjectsOfType(typeof(ORNavigation)) as ORNavigation[];
    	_navigationItems = new string[_navigationsFound.Length];
    	
    	int i = 0;
    	
    	_selectedNavigationIndex = -1;
    	
    	foreach (ORNavigation aNavigation in _navigationsFound) {
    		
    		_navigationItems[i] = aNavigation.gameObject.name + " - " + aNavigation.navigationName;
    		
    		if (aNavigation == currentNavigation)
    			_selectedNavigationIndex = i;
    		
    		i++;
    		
    	}
    	
    	if (_selectedNavigationIndex < 0)
    		_selectedNavigationIndex = 0;
    	
    	currentNavigation = selectedNavigation;
    	
    	if (currentNavigation != null)
    		currentNavigation.CalculateTVelocity();
    		
    }
    
    private void Play() {
    	
    	_state = ORPreviewState.Playing;
    	
    	if (_t >= 1)
    		_t = 0;
    		
    }
    
    private void Pause() {
    	_state = ORPreviewState.Paused;
    }
    
    private void Stop() {
    	
    	_state = ORPreviewState.Stopped;
    	
    	_t = 0;
    	
    }
    
}
