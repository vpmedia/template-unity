using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The MainController class is responsible for Core System Management.
/// </summary>
public class MainController : MonoBehaviour
{

	// ================================================================================
	// Variables
	// ================================================================================
	
	#region Enums

	private enum SceneState
	{
		Reset,
		Preload,
		Load,
		Unload,
		Postload,
		Ready,
		Run,
		Count }
	;

	#endregion

	#region Variables

	private static MainController mainController;
	private EventBus eventBus;
	private string currentSceneName;
	private string nextSceneName;
	private AsyncOperation resourceUnloadTask;
	private AsyncOperation sceneLoadTask;
	private SceneState sceneState;
	private delegate void UpdateDelegate ();

	private UpdateDelegate[] updateDelegates;

	#endregion
	
	// ================================================================================
	// Static Methods
	// ================================================================================

	// Use this for initialization
	public static void SwitchScene (string newSceneName)
	{
		#if DEBUG
		Debug.Log ("MainController::SwitchScene: " + newSceneName);
		#endif
		if (mainController != null && mainController.currentSceneName != newSceneName) {
			mainController.nextSceneName = newSceneName;
		}
	}

	// ================================================================================
	// Protected Methods
	// ================================================================================

	// Entry point
	protected void Awake ()
	{
		#if DEBUG
		Debug.Log ("MainController::Awake");
			
		string sysInfo = "System Information: ";
		sysInfo += "\nversion: " + Application.version;
		sysInfo += "\nabsoluteURL: " + Application.absoluteURL;
		sysInfo += "\nbundleIdentifier: " + Application.bundleIdentifier;	
		sysInfo += "\ncompanyName: " + Application.companyName;		
		sysInfo += "\ndataPath: " + Application.dataPath;			
		sysInfo += "\nisConsolePlatform: " + Application.isConsolePlatform;	
		sysInfo += "\nisEditor: " + Application.isEditor;		
		sysInfo += "\nisMobilePlatform: " + Application.isMobilePlatform;	
		sysInfo += "\nisPlaying " + Application.isPlaying;		
		sysInfo += "\nlevelCount " + Application.levelCount;	
		sysInfo += "\nloadedLevel " + Application.loadedLevel;	
		sysInfo += "\nloadedLevelName " + Application.loadedLevelName;		
		sysInfo += "\nplatform " + Application.platform;			
		sysInfo += "\nproductName " + Application.productName;		
		sysInfo += "\nsandboxType " + Application.sandboxType;		
		sysInfo += "\nsystemLanguage " + Application.systemLanguage;		
		sysInfo += "\nunityVersion: " + Application.unityVersion;	
		Debug.Log (sysInfo);
		#endif

		// Keep alive between scenes
		Object.DontDestroyOnLoad (gameObject);

		// Setup singleton
		mainController = this;
		
		// Initialize Signal
		eventBus = new EventBus ();
	
		// Initialize SceneManager
		updateDelegates = new UpdateDelegate[(int)SceneState.Count];
		updateDelegates [(int)SceneState.Reset] = UpdateSceneReset;
		updateDelegates [(int)SceneState.Preload] = UpdateScenePreload;
		updateDelegates [(int)SceneState.Load] = UpdateSceneLoad;
		updateDelegates [(int)SceneState.Unload] = UpdateSceneUnload;
		updateDelegates [(int)SceneState.Postload] = UpdateScenePostload;
		updateDelegates [(int)SceneState.Ready] = UpdateSceneReady;
		updateDelegates [(int)SceneState.Run] = UpdateSceneRun;

		currentSceneName = "Scene0";
		nextSceneName = "Scene1";
		sceneState = SceneState.Reset;

	}
	
	// Use this for disposing
	protected void OnDestroy ()
	{
		#if DEBUG
		Debug.Log ("MainController::OnDestroy");
		#endif		
		// clean up SceneManager
		if (updateDelegates != null) {
			int sceneStateCount = (int)SceneState.Count;
			for (int i = 0; i < sceneStateCount; i++) {
				updateDelegates [i] = null;
			}
			updateDelegates = null;
		}	
		// clean up EventManager
		if (eventBus != null) {
			eventBus.Dispose ();
			eventBus = null;
		}	
		// clean up Singleton
		if (mainController != null) {
			mainController = null;
		}
	}
		
	// Enabled event
	protected void OnEnable ()
	{
		#if DEBUG
		Debug.Log ("MainController::OnEnable");
		#endif
	}

	// Disabled event
	protected void OnDisable ()
	{
		#if DEBUG
		Debug.Log ("MainController::OnDisable");
		#endif
	}

	// Use this for initialization
	protected void Start ()
	{
		#if DEBUG
		Debug.Log ("MainController::Start");
		#endif	
	}

	// Update is called once per frame
	protected void Update ()
	{
		updateDelegates [(int)sceneState] ();	
	}
	
	// ================================================================================
	// Private Methods
	// ================================================================================

	// Start to load new level
	private void UpdateSceneReset ()
	{
		#if DEBUG
		Debug.Log ("MainController::UpdateSceneReset: " + currentSceneName);
		#endif
		System.GC.Collect ();
		sceneState = SceneState.Preload;
		
	}
	
	// Load next level
	private void UpdateScenePreload ()
	{
		#if DEBUG
		Debug.Log ("MainController::UpdateScenePreload: " + nextSceneName);
		#endif
		sceneLoadTask = Application.LoadLevelAsync (nextSceneName);
		sceneState = SceneState.Load;
		
	}
	
	// Level preloader
	private void UpdateSceneLoad ()
	{
		if (sceneLoadTask.isDone) {
			#if DEBUG
			Debug.Log ("MainController::UpdateSceneLoad::Complete: " + nextSceneName);
			#endif
			sceneState = SceneState.Unload;
		} else {
			// TODO: Progress bar
		}
		
	}
	
	// Clean up unused resources by unloading them
	private void UpdateSceneUnload ()
	{
		if (resourceUnloadTask == null) {
			#if DEBUG
			Debug.Log ("MainController::UpdateSceneUnload::Start: " + currentSceneName);
			#endif
			/*#if UNITY_5_2
			Application.UnloadLevel(currentSceneName);
			#endif*/
			resourceUnloadTask = Resources.UnloadUnusedAssets ();
		} else  if (resourceUnloadTask.isDone) {
			#if DEBUG
			Debug.Log ("MainController::UpdateSceneUnload::Complete: " + currentSceneName);
			#endif
			resourceUnloadTask = null;
			sceneState = SceneState.Postload;
		}
		
	}
	
	// Handle anything that needs to happen immediately after loading
	private void UpdateScenePostload ()
	{
		#if DEBUG
		Debug.Log ("MainController::UpdateScenePostload: " + nextSceneName);
		#endif
		currentSceneName = nextSceneName;
		sceneState = SceneState.Ready;
		
	}
	
	// Handle anything that needs to happen immediately before running
	private void UpdateSceneReady ()
	{
		#if DEBUG
		Debug.Log ("MainController::UpdateSceneReady: " + currentSceneName);
		#endif
		System.GC.Collect ();
		sceneState = SceneState.Run;
		
	}
	
	// Wait a scene change
	private void UpdateSceneRun ()
	{
		if (currentSceneName != nextSceneName) {
			sceneState = SceneState.Reset;
		}

		
	}

}
