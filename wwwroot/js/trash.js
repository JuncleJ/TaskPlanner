const trashList = document.getElementById("trashList");
const trashMessage = document.getElementById("trashMessage");
const trashSearch = document.getElementById("trashSearch");
const trashSort = document.getElementById("trashSort");

let deletedTasks = [];

async function loadTrash() {
    const response = await fetch("/api/tasks/trash");

    if (!response.ok) {
        throw new Error(`Ошибка загрузки: ${response.status}`);
    }

    deletedTasks = await response.json();
    renderTrash();
}

function compareDue(a, b, descending) {
    const left = a.dueAt ? new Date(a.dueAt).getTime() : null;
    const right = b.dueAt ? new Date(b.dueAt).getTime() : null;

    if (left === null) return right === null ? 0 : 1;
    if (right === null) return -1;

    return descending ? right - left : left - right;
}

function renderTrash() {
    trashList.replaceChildren();

    const search = trashSearch.value.trim().toLowerCase();

    const tasks = deletedTasks.filter(task =>
        task.title.toLowerCase().includes(search)
    );

    switch (trashSort.value) {
        case "oldest":
            tasks.sort((a, b) =>
                new Date(a.createdAt) - new Date(b.createdAt));
            break;

        case "due_asc":
            tasks.sort((a, b) => compareDue(a, b, false));
            break;

        case "due_desc":
            tasks.sort((a, b) => compareDue(a, b, true));
            break;

        default:
            tasks.sort((a, b) =>
                new Date(b.createdAt) - new Date(a.createdAt));
    }

    if (tasks.length === 0) {
        const empty = document.createElement("li");
        empty.className = "list-group-item";
        empty.textContent = "Удалённых задач нет";
        trashList.append(empty);
        return;
    }

    for (const task of tasks) {
        const item = document.createElement("li");
        item.className =
            "list-group-item d-flex justify-content-between gap-3";

        const title = document.createElement("span");
        title.textContent = task.title;

        const actions = document.createElement("div");
        actions.className = "d-flex gap-2";

        const restore = document.createElement("button");
        restore.className = "btn btn-sm btn-outline-success";
        restore.textContent = "Восстановить";

        restore.addEventListener("click", () =>
            execute(async () => {
                await changeTask(
                    `/api/tasks/${task.id}/restore`,
                    "POST"
                );
            })
        );

        const remove = document.createElement("button");
        remove.className = "btn btn-sm btn-outline-danger";
        remove.textContent = "Удалить навсегда";

        remove.addEventListener("click", () =>
            execute(async () => {
                if (!confirm("Удалить задачу без возможности восстановления?"))
                    return;

                await changeTask(
                    `/api/tasks/trash/${task.id}`,
                    "DELETE"
                );
            })
        );

        actions.append(restore, remove);
        item.append(title, actions);
        trashList.append(item);
    }
}

async function changeTask(url, method) {
    const response = await fetch(url, { method });

    if (!response.ok) {
        throw new Error(`Ошибка операции: ${response.status}`);
    }

    await loadTrash();
}

async function execute(action) {
    trashMessage.textContent = "";

    try {
        await action();
    } catch (error) {
        trashMessage.textContent = error.message;
    }
}

trashSearch.addEventListener("input", renderTrash);
trashSort.addEventListener("change", renderTrash);

execute(loadTrash);