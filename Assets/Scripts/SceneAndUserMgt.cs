using UnityEngine;
using System.IO;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Drawing;
using UnityEngine.UI;

enum Degree
{
    EASY,
    MEDIUM,
    HARD
}

public class SceneAndUserMgt : MonoBehaviour
{
    public static SceneAndUserMgt Instance { get; private set; }
    private static string file;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private TextMeshProUGUI usernameText;
    private static TMP_InputField usernameInputField;
    private GameObject Degrees;


    void Awake()
    {
        if (!PlayerPrefs.HasKey("username"))
        {
            PlayerPrefs.SetString("username", "default");
        }

        if (!PlayerPrefs.HasKey("degree"))
        {
            PlayerPrefs.SetString("degree", Degree.EASY.ToString());
        }
        
        string dir = Path.Combine(Application.persistentDataPath, "UserData");
        Directory.CreateDirectory(dir);
        file = Path.Combine(dir, "userdata.json");

        Debug.Log("User data path: " + file);
        if (!File.Exists(file)) 
        {
            UserData defaultData = new(){ username = "default", highScore = 0 };
            Users users = defaultData.ConvertToUserList();
            Debug.Log("Creating default user data: " + JsonUtility.ToJson(users));
            File.WriteAllText(file, JsonUtility.ToJson(users));
        }
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    public static Users LoadUserData()
    {
        string json = File.ReadAllText(file);
        Debug.Log(json);
        Users users = JsonUtility.FromJson<Users>(json);
        users.users.Sort((a, b) => b.highScore.CompareTo(a.highScore));
        return users;
    }

    public static UserData GetUserData(string username)
    {
        Users users = LoadUserData();
        foreach (var user in users.users)
        {
            if (user.username.ToLower() == username.ToLower())
            {
                return user;
            }
        }
        return null; // Return null if user not found
    }

    public static void SaveUserData(Users users)
    {
        string json = JsonUtility.ToJson(users);
        File.WriteAllText(file, json);
    }

    public static void SaveUserHighScore(int highScore)
    {
        Users users = LoadUserData();
        for (int i = 0; i < users.users.Count; i++)
        {
            if (users.users[i].username.ToLower() == PlayerPrefs.GetString("username").ToLower())
            {
                users.users[i].highScore = highScore;
                SaveUserData(users);
                return;
            }
        }
    }

    public static int GetLastHighScore()
    {
        string username = PlayerPrefs.GetString("username", "default");
        UserData user = GetUserData(username);
        if (user != null)
        {
            return user.highScore;
        }
        return 0;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += (scene, mode) => AfterSceneLoad();
    }

    public void DegreeSet ()
    {
        Degrees = GameObject.FindWithTag("degree");
        SetupDegreeToggles();
    }

    private void SetupDegreeToggles()
    {
        if (Degrees == null) return;
        Toggle[] toggles = Degrees.GetComponentsInChildren<Toggle>();
        string savedDegree = PlayerPrefs.GetString("degree");
        foreach (var t in toggles)
        {
            t.isOn = t.name == savedDegree;
        }
    }

    void AfterSceneLoad()
    {
        try 
        {
            leaderboardPanel = GameObject.FindWithTag("leaderboardPanel");
            usernameText = GameObject.FindWithTag("usernameInput").GetComponent<TextMeshProUGUI>();
            usernameText.text = PlayerPrefs.GetString("username", "default");
            Degrees = GameObject.FindWithTag("degree");
            Debug.Log(PlayerPrefs.GetString("username"));
            Debug.Log(PlayerPrefs.GetString("degree"));

            foreach (Transform child in leaderboardPanel.transform)
            {
                Destroy(child.gameObject);
            }
            Users users = LoadUserData();
            for (int i = 0; i < users.users.Count; i++)
            {
                var user = users.users[i];
                var entryInstance = Instantiate(leaderboardEntryPrefab, leaderboardPanel.transform);
                entryInstance.transform.position = Vector3.zero;
                TextMeshProUGUI[] texts = entryInstance.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[1].text = $"{user.username}";
                    texts[0].text = user.highScore.ToString();
                }
                else
                {
                    Debug.LogError("Leaderboard entry prefab is missing TextMeshProUGUI components! Make sure it has two TextMeshProUGUI components for username and score.");
                }
            }
        }
        catch {
            Debug.LogError("Leaderboard panel or entry prefab not found! Make sure they are in the scene and tagged correctly.");
        }
    }

    public static void AddNewUser(UserData user)
    {
        Users users = LoadUserData();
        if (users.users.Exists(u => u.username.ToLower() == user.username.ToLower()))
        {
            user.username = user.username.ToLower();
            PlayerPrefs.SetString("username", user.username);
            return;
        }
        users.users.Add(user);
        PlayerPrefs.SetString("username", user.username.ToLower());
        SaveUserData(users);
    }

    public void NewUser()
    {
        usernameInputField = GameObject.FindWithTag("userNameData").GetComponent<TMP_InputField>();
        string newUsername = usernameInputField.text.Trim();
        if (string.IsNullOrEmpty(newUsername))
        {
            return;
        }
        AddNewUser(new UserData { username = newUsername, highScore = 0 });
        foreach (Transform child in leaderboardPanel.transform)
        {
            Destroy(child.gameObject);
        }
        usernameText.text = newUsername;
        Users users = LoadUserData();
        for (int i = 0; i < users.users.Count; i++)
        {
            var user = users.users[i];
            var entryInstance = Instantiate(leaderboardEntryPrefab, leaderboardPanel.transform);
            entryInstance.transform.position = Vector3.zero;
            TextMeshProUGUI[] texts = entryInstance.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[1].text = $"{user.username}";
                texts[0].text = user.highScore.ToString();
            }
            else
            {
                Debug.LogError("Leaderboard entry prefab is missing TextMeshProUGUI components! Make sure it has two TextMeshProUGUI components for username and score.");
            }
        }
    }
}

[System.Serializable]
public class Users
{
    public List<UserData> users = new();
}

[System.Serializable]
public class UserData
{
    public string username;
    public int highScore;

    public Users ConvertToUserList()
    {
        Users users = new Users();
        users.users.Add(this);
        return users;
    }
}