const API = "/api/tasks";

const form = document.getElementById("taskForm");
const list = document.getElementById("tasksList");

let editingTask = null;

async function request(url, method = "GET", body = null) {
    const options = { method };

    if (body !== null) {
        options.headers = { "Content-Type": "application/json" };
        options.body = JSON.stringify(body);
    }

    const response = await fetch(url, options);

    if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
    }

    if (response.status === 204) return null;

    return response.json();
}

async function execute(action) {
    try {
        document.getElementById("message").textContent = "";
        await action();
    } catch (error) {
        document.getElementById("message").textContent = error.message;
    }
}

async function loadTasks() {
    const search = document.getElementById("search").value;
    const sort = document.getElementById("sort").value;

    const params = new URLSearchParams({ search, sort });
    const tasks = await request(`${API}?${params}`);

    list.replaceChildren();

    for (const task of tasks) {
        const li = document.createElement("li");
        li.className = "list-group-item";

        const title = document.createElement("span");
        title.textContent = `${task.isCompleted ? "✅ " : ""}${task.title} `;

        const complete = document.createElement("button");
        complete.textContent = task.isCompleted ? "Отменить" : "Готово";
        complete.className = "btn btn-sm btn-success mx-1";
        complete.onclick = () => execute(() => completeTask(task));

        const edit = document.createElement("button");
        edit.textContent = "Изменить";
        edit.className = "btn btn-sm btn-primary mx-1";
        edit.onclick = () => editTask(task);

        const remove = document.createElement("button");
        remove.textContent = "Удалить";
        remove.className = "btn btn-sm btn-danger mx-1";
        remove.onclick = () => execute(() => deleteTask(task.id));

        li.append(title, complete, edit, remove);
        list.append(li);
    }
}


async function saveTask() {
    const date = document.getElementById("dueAt").value;

    const task = {
        title: document.getElementById("title").value,
        description: document.getElementById("description").value,
        dueAt: date ? new Date(date).toISOString() : null,
        isCompleted: editingTask?.isCompleted ?? false
    };

    const method = editingTask ? "PUT" : "POST";
    const url = editingTask ? `${API}/${editingTask.id}` : API;

    await request(url, method, task);

    cancelEdit();
    await loadTasks();
}


function editTask(task) {
    editingTask = task;

    document.getElementById("title").value = task.title;
    document.getElementById("description").value = task.description || "";

    if (task.dueAt) {
        const date = new Date(task.dueAt);
        date.setMinutes(date.getMinutes() - date.getTimezoneOffset());

        document.getElementById("dueAt").value =
            date.toISOString().slice(0, 16);
    } else {
        document.getElementById("dueAt").value = "";
    }

    document.getElementById("saveButton").textContent = "Сохранить";
    document.getElementById("cancelButton").hidden = false;
}


function cancelEdit() {
    editingTask = null;
    form.reset();

    document.getElementById("saveButton").textContent = "Создать";
    document.getElementById("cancelButton").hidden = true;
}

async function completeTask(task) {
    await request(`${API}/${task.id}`, "PUT", {
        title: task.title,
        description: task.description,
        dueAt: task.dueAt,
        isCompleted: !task.isCompleted
    });

    await loadTasks();
}


async function deleteTask(id) {
    if (!confirm("Удалить задачу?")) return;

    await request(`${API}/${id}`, "DELETE");
    await loadTasks();
}


form.onsubmit = event => {
    event.preventDefault();
    execute(saveTask);
};

document.getElementById("cancelButton").onclick = cancelEdit;

document.getElementById("filterForm").onsubmit = event => {
    event.preventDefault();
    execute(loadTasks);
};

document.getElementById("sort").onchange = () => execute(loadTasks);

execute(loadTasks);