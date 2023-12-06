using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Red1chi.Tasks {
[CreateAssetMenu(fileName = "New Task List", menuName = "Task List", order = 0)]
public class TaskListSO : ScriptableObject {

    [SerializeField]
    List<string> tasks = new List<string>();
    public List<string> GetTasks() {
        return tasks;
    }
    public void AddTask(string task) {
        tasks.Add(task);
    }
    public void AddTasks(List<string> savedTasks) {
        tasks.Clear();
        tasks = savedTasks;
    }
}}