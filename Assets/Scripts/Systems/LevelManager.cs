 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 using UnityEngine.SceneManagement;
 
 public class LevelManager : MonoBehaviour
 {
     public static LevelManager Instance { get; private set; }
 
     [Header("Scene Order")]
     public string startMenuScene = "StartMenu";
     public List<string> levelScenes = new List<string> { "Level_1", "Level_2", "Level_3" };
 
     [Header("Transition Settings")]
     public float nextLevelDelay = 1f;
 
     private HashSet<Enemy> activeEnemies = new HashSet<Enemy>();
     private bool isTransitioning = false;
 
     void Awake()
     {
         if (Instance == null)
         {
             Instance = this;
             DontDestroyOnLoad(gameObject);
             SceneManager.sceneLoaded += OnSceneLoaded;
         }
         else
         {
             Destroy(gameObject);
         }
     }
 
     private void OnDestroy()
     {
         if (Instance == this)
         {
             SceneManager.sceneLoaded -= OnSceneLoaded;
         }
     }
 
     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
     {
         isTransitioning = false;
 
         if (!IsLevelScene(scene.name))
         {
             activeEnemies.Clear();
             return;
         }
 
         // Re-register enemies present in the scene (use HashSet to avoid duplicates)
         Enemy[] enemiesInScene = FindObjectsOfType<Enemy>();
         foreach (Enemy enemy in enemiesInScene)
         {
             RegisterEnemy(enemy);
         }
 
         // Handle case where a level starts with zero enemies (e.g., already cleared)
         if (activeEnemies.Count == 0)
         {
             StartCoroutine(LoadNextLevelWithDelay());
         }
     }
 
     public void RegisterEnemy(Enemy enemy)
     {
         if (enemy == null || !IsLevelScene(SceneManager.GetActiveScene().name)) return;
 
         activeEnemies.Add(enemy);
     }
 
     public void UnregisterEnemy(Enemy enemy)
     {
         if (enemy == null || activeEnemies.Count == 0) return;
 
         activeEnemies.Remove(enemy);
 
         if (!isTransitioning && activeEnemies.Count == 0 && IsLevelScene(SceneManager.GetActiveScene().name))
         {
             StartCoroutine(LoadNextLevelWithDelay());
         }
     }
 
     private IEnumerator LoadNextLevelWithDelay()
     {
         isTransitioning = true;
         yield return new WaitForSeconds(nextLevelDelay);
         LoadNextLevel();
     }
 
    private void LoadNextLevel()
     {
         string currentScene = SceneManager.GetActiveScene().name;
         int currentIndex = levelScenes.IndexOf(currentScene);
 
         if (currentIndex >= 0 && currentIndex < levelScenes.Count - 1)
         {
             SceneManager.LoadScene(levelScenes[currentIndex + 1]);
         }
         else
         {
            ShowGameOverUI();
         }
     }

    private void ShowGameOverUI()
    {
        GameOverUI gameOverUI = FindObjectOfType<GameOverUI>(true);
        if (gameOverUI != null)
        {
            gameOverUI.Show();
        }
        else
        {
            ReturnToMainMenu();
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(startMenuScene);
    }
 
     private bool IsLevelScene(string sceneName)
     {
         return levelScenes.Contains(sceneName);
     }
 }
 
