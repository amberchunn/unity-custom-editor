using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Red1chi.Tasks {
    public class TaskListEditor : EditorWindow
    {
        // Root Visual Element for Editor Window
        VisualElement container;
        // Variables for Existing File Loading
        ObjectField savedTaskObjectField;
        Button loadExistingTasksBtn;
        // Variables for Elements
        TextField taskText;
        Button addTaskBtn;
        Button saveProgressBtn;
        ScrollView taskList;
        ProgressBar taskProgressBar;
        ToolbarSearchField searchBox;
        Label notificationLabel;
        VisualElement notificationContainer;
        Button notificationDismissBtn;

        TaskListSO taskListSO;

        // Editor Window Path Reference
        public const string path = "Assets/Red1chi Tools/unity-custom-tasklist/Editor/EditorWindow/";

        // Menu Item Name
        [MenuItem("Red1chi Tools/Task List")]

        // Method for New Editor Payne
        public static void ShowWindow() {

            // Name of Editor Window (should match .uxml file)
            TaskListEditor window = GetWindow<TaskListEditor>();

            // Window Title
            window.titleContent = new GUIContent("Task List");
        }
        // Method for Rendering New GUI Window
        public void CreateGUI() {

            // Set Reference to Root Visual Element
            container = rootVisualElement;

            // Grab Editor uxml Path and Instantiate
            VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "TaskListEditor.uxml");
            container.Add(original.Instantiate());

            // Attach uss Stylesheet to Editor Window
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "TaskListEditor.uss");
            container.styleSheets.Add(styleSheet);

            // Add References for Editor Window Elements
            taskText = container.Q<TextField>("TaskText");
            taskText.value = "";
            taskText.RegisterCallback<KeyDownEvent>(AddTask);

            addTaskBtn = container.Q<Button>("AddTaskBtn");
            addTaskBtn.clicked += AddTask;
            taskList = container.Q<ScrollView>("TaskList");

            saveProgressBtn = container.Q<Button>("SaveProgressBtn");
            saveProgressBtn.clicked += SaveProgress;

            taskProgressBar = container.Q<ProgressBar>("TaskProgressBar");

            savedTaskObjectField = container.Q<ObjectField>("FileObject");
            savedTaskObjectField.objectType = typeof(TaskListSO);

            loadExistingTasksBtn = container.Q<Button>("FileLoadBtn");
            loadExistingTasksBtn.clicked += LoadTasks;

            searchBox = container.Q<ToolbarSearchField>("SearchBox");
            searchBox.RegisterValueChangedCallback(OnSearchTextChanged);

            notificationContainer = container.Q<VisualElement>("NotificationContainer");
            notificationLabel = container.Q<Label>("NotificationText");
            notificationDismissBtn = container.Q<Button>("NotificationDismissBtn");
            notificationDismissBtn.clicked += HideNotification;
            UpdateNotifications("Please load a task list to continue");

        }

        TaskItem CreateTask(string taskText) {
            TaskItem taskItem = new TaskItem(taskText);
            taskItem.GetTaskLabel().text = taskText;
            taskItem.GetTaskToggle().RegisterValueChangedCallback(UpdateProgress);
            return taskItem;
        }

        void AddTask() {
            if (!string.IsNullOrEmpty(taskText.value))
            {
                taskList.Add(CreateTask(taskText.value));
                SaveTask(taskText.value);
                taskText.value = "";
                taskText.Focus();
                UpdateProgress();
                UpdateNotifications("Task added successfully");
            }
        }

        void AddTask(KeyDownEvent e) {
            if(Event.current.Equals(Event.KeyboardEvent("Return"))) {
                AddTask();
            }
        }
        void LoadTasks () {
            taskListSO = savedTaskObjectField.value as TaskListSO;

            if (taskListSO != null) {
                taskList.Clear();
                List<string> tasks = taskListSO.GetTasks();

                foreach (string task in tasks)
                {
                    taskList.Add(CreateTask(task));
                }
                UpdateProgress();
                UpdateNotifications(taskListSO.name + " successfully loaded.");
            }
            else
            {
                UpdateNotifications("Failed to load task list.");
            }
        }

        void SaveTask(string task) {
            taskListSO.AddTask(task);
            EditorUtility.SetDirty(taskListSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UpdateNotifications("Task added successfully.");
        }

        void SaveProgress()
        {
            if (taskListSO != null)
            {
                List<string> tasks = new List<string>();

                foreach (TaskItem task in taskList.Children())
                {
                    if(!task.GetTaskToggle().value)
                    {
                        tasks.Add(task.GetTaskLabel().text);
                    }
                }

                taskListSO.AddTasks(tasks);
                EditorUtility.SetDirty(taskListSO);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                LoadTasks();
                UpdateNotifications("Tasks saved successfully.");
            }
        }

        void UpdateProgress()
        {
            int count = 0;
            int completed = 0;

            foreach (TaskItem task in taskList.Children())
            {
                if(task.GetTaskToggle().value)
                {
                    completed++;
                }
                count++;
            }

            if (count > 0)
            {
                float progress = completed / (float)count;
                taskProgressBar.value = progress;
                taskProgressBar.title = Mathf.Round(progress * 100).ToString() + "%";
                UpdateNotifications("Progress Updated. Don't forget to save!");
            }
            else
            {
                taskProgressBar.value = 1;
                taskProgressBar.title = string.Format("{0} %", 100);
            }
        }

        void UpdateProgress(ChangeEvent<bool> e)
        {
            UpdateProgress();
        }

        void HideNotification() {
            notificationContainer.AddToClassList("hide");
        }

        void OnSearchTextChanged(ChangeEvent<string> changeEvent)
        {
            string searchText = changeEvent.newValue.ToLower();

            foreach(TaskItem task in taskList.Children())
            {
                string taskText = task.GetTaskLabel().text.ToLower();

                if(!string.IsNullOrEmpty(searchText) && taskText.Contains(searchText))
                {
                    task.GetTaskLabel().AddToClassList("highlight");
                }
                else
                {
                    task.GetTaskLabel().RemoveFromClassList("highlight");
                }
            }
        }

        void UpdateNotifications(string text)
        {
            if(!string.IsNullOrEmpty(text))
            {
                notificationContainer.RemoveFromClassList("hide");
                notificationLabel.text = text;
            }
        }
    }
}