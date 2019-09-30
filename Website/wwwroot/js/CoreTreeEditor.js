"use strict";

/**
 * Выполняет глубокое копирование объекта.
 * @template T Тип объекта.
 * @param {T} src Объект, который необходимо скопировать.
 * @returns {T} Возвращает клон объекта.
 */
function deepClone(src) {
    let clone;
    if (Array.isArray(src)) {
        clone = new Array();
    }
    else {
        clone = Object.create(Object.getPrototypeOf(src));
    }
    for (let prop in src) {
        if (src[prop] != null && typeof (src[prop]) === "object") {
            clone[prop] = deepClone(src[prop]);
        }
        else {
            clone[prop] = src[prop];
        }
    }

    return clone;
}

/**
 * JSON разметки.
 * @type {string}
 */
let jsonMarkup = null;

/** Возвращает текущую разметку в виде JSON. */
function getCurrentMarkup() {
    root.id = 0;
    const allNodes = [root];
    const rootChildren = root.children;
    for (let i = 0; i < rootChildren.length; i++) {
        depthAdding(rootChildren[i]);
    }
    /**
     * Добавляет узлы в массив, идя в глубину.
     * @param {TreeNode} node Текущий узел.
     */
    function depthAdding(node) {
        node.id = allNodes.length;
        node.parentId = node.parent.id;
        allNodes.push(node);
        const children = node.children;
        for (let i = 0; i < children.length; i++) {
            depthAdding(children[i]);
        }
    }

    return JSON.stringify(allNodes);
}

window.onbeforeunload = function (evt) {
    if (jsonMarkup === getCurrentMarkup()) return null;

    const message = "Данные не были сохранены. Вы уверены, что хотите выполнить это действие?";
    if (typeof evt == "undefined") {
        evt = window.event;
    }
    if (evt) {
        evt.returnValue = message;
    }
    return message;
}

/** Отправляет данные на сервер. */
function sendToServer() {
    const jqSelf = $(this).prop("disabled", true).switchClass("btn-success btn-danger", "btn-warning");
    const jqSaveSpinner = jqSelf.find("span").removeClass("oi oi-cloud-upload oi-circle-x").addClass("spinner-border spinner-border-sm");

    const sentMarkup = getCurrentMarkup();

    $.ajax({
        url: '/BotForSalesEditing/SaveTree',
        type: 'post',
        data: { botId: botId, tree: sentMarkup },
        success: function () {
            jqSaveSpinner.removeClass("spinner-border spinner-border-sm").addClass("oi oi-cloud-upload");
            jqSelf.switchClass("btn-warning", "btn-success").prop("disabled", false);
            jsonMarkup = sentMarkup;
        },
        error: function (data) {
            jqSaveSpinner.removeClass("spinner-border spinner-border-sm").addClass("oi oi-circle-x");
            jqSelf.switchClass("btn-warning", "btn-danger").prop("disabled", false);

            console.log("Error: " + data.responseText);
        }
    });
}

/**
 * Восстанавливает данные из JSON.
 */
function loadFromJSON() {
    const jqMainCont = $("#main-container");
    jqMainCont.children().remove();
    const jqSpinnerDiv = $("<div>").addClass("spinner-border text-primary m-auto centered").attr("role", "status").css("font-size", "5rem").appendTo(jqMainCont);
    try {
        const array = JSON.parse(jsonMarkup);
        const firstNodeParams = array[0].parameters;
        if (firstNodeParams.type != 1) throw new Error("Первый узел не является корнем!");
        root = new RootNode(firstNodeParams.name, firstNodeParams.message, firstNodeParams.fileId);
        root.parameters.previewId = firstNodeParams.previewId;
        const nodes = [root];
        for (let i = 1; i < array.length; i++) {
            const jsonParams = array[i].parameters;
            const template = templates[jsonParams.type];
            const node = Object.create(Object.getPrototypeOf(template));
            node.parameters = Object.create(Object.getPrototypeOf(template.parameters));
            const nodeParams = node.parameters;
            for (let prop in jsonParams) {
                if (jsonParams[prop] != null && typeof (jsonParams[prop]) === "object") {
                    nodeParams[prop] = deepClone(jsonParams[prop]);
                }
                else {
                    nodeParams[prop] = jsonParams[prop];
                }
            }
            const clonedNode = node.cloneNode();
            nodes[array[i].parentId].appendChild(clonedNode);
            nodes.push(clonedNode);
        }

        jqMainCont.children().fadeOut("slow", function () {
            $(root.container).attr("id", "main").hide().appendTo(jqMainCont.children().remove().end()).fadeIn("fast");
        });
    } catch (e) {
        showError(e.message);
    }

    function showError(msg) {
        jqSpinnerDiv.removeClass("spinner-border text-primary centered").addClass("text-danger").append([
            $("<p>").addClass("oi oi-x text-center"),
            $("<p>").addClass("h3").text("Ошибка загрузки данных:"),
            $("<p>").addClass("h3").text(msg),
            $("<p>").addClass("h5 text-dark").text("Попробуйте обновить страницу.")
        ]);
    }
}

/**
 * Выбранный узел.
 * @type {TreeNode}
 */
let selectedNode = null;

const fileTypes = Object.freeze({
    "Auto": 0,
    "Image": 1,
    "Audio": 2,
    "Video": 3,
    "Document": 4
});

const acceptTypes = Object.freeze({
    0: "*/*",
    1: "image/*",
    2: "audio/*",
    3: "video/*",
    4: "*/*"
});

/**
 * Основная функция изменения типа файла в базовом модальном окне.
 * @param {string} newType Название нового типа.
 */
function changeInputType(newType) {
    const inputGroup = this.parentNode.parentNode;
    const numType = fileTypes[newType];
    inputGroup.firstChild.textContent = newType;
    const input = inputGroup.parentElement.children[1].getElementsByTagName("input")[0];
    input.setAttribute("data-type", numType);
    input.setAttribute("accept", acceptTypes[numType]);
}

const chooseFile = "Choose file";
const noFileSelectedSpan = $("<span>").addClass("text-secondary display-4 overflow-hidden").css("word-break", "break-word").text("No file selected");

/** Базовое модальное окно. */
const baseModal = $("<div>").attr({
    class: "modal fade",
    tabindex: -1,
    role: "dialog",
    "aria-hidden": true
}).append($("<div>").attr({
    class: "modal-dialog modal-dialog-centered modal-lg",
    role: "document"
}).append($("<div>").addClass("modal-content").append([
    $("<div>").addClass("modal-header").append([
        $("<input>").attr({
            class: "modal-title bg-transparent w-100 border-0 h5",
            type: "text"
        }).val("Node name"),
        $("<button>").attr({
            class: "close",
            "data-dismiss": "modal",
            "aria-label": "Close"
        }).append($("<span>").attr("aria-hidden", "true").html('&times;'))
    ]),
    $("<div>").addClass("modal-body").append($("<form>").attr("enctype", "multipart/form-data").append([
        $("<div>").addClass("form-row align-items-stretch").append([
            $("<div>").addClass("col col-5 d-flex flex-column justify-content-start align-items-stretch").append([
                $("<div>").addClass("fileHolder flex-fill align-self-stretch rounded border border-secondary text-center d-flex flex-column justify-content-center").
                    append(noFileSelectedSpan),
                $("<div>").addClass("input-group mt-1 bg-white").append([
                    $("<div>").addClass("input-group-prepend").append($("<button>").attr({
                        class: "btn btn-outline-danger base-remove-file",
                        type: "button"
                    }).on("click", function () {
                        $(this).parent().parent().children(".custom-file").children(".base-file").data("file_id", null).data("preview_id", null).val("").end().
                            children(".custom-file-label").text(chooseFile).end().parent().parent().children(".fileHolder").empty().append(noFileSelectedSpan.clone());
                    }).append($("<span>").addClass("oi oi-trash"))),
                    $("<div>").addClass("custom-file").append([
                        $("<input>").attr({
                            class: "form-control-file custom-file-input base-file",
                            type: "file",
                            "data-type": 0
                        }).on("change", function () {
                            let file = this.files[0];
                            let jqFileInputDiv = $(this.parentElement);
                            const jqFileHolder = jqFileInputDiv.parent().parent().children(".fileHolder");
                            jqFileHolder.children().remove();
                            if (file != null) {
                                jqFileInputDiv.children(".custom-file-label").text(file.name);
                                let fileType = parseInt(this.getAttribute("data-type"));
                                jqFileHolder.append($("<span>").addClass("spinner-border text-secondary"));
                                let request;
                                switch (fileType) {
                                    case fileTypes.Image:
                                        request = SendPhoto(botToken, userId, file);
                                        break;
                                    case fileTypes.Audio:
                                        request = SendAudio(botToken, userId, file);
                                        break;
                                    case fileTypes.Video:
                                        request = SendVideo(botToken, userId, file);
                                        break;
                                    case fileTypes.Document:
                                        request = SendDocument(botToken, userId, file);
                                        break;
                                    default:
                                        fileType = file.type.split('/').shift();
                                        switch (fileType) {
                                            case "image":
                                                request = SendPhoto(botToken, userId, file);
                                                break;
                                            case "audio":
                                                request = SendAudio(botToken, userId, file);
                                                break;
                                            case "video":
                                                request = SendVideo(botToken, userId, file);
                                                break;
                                            default:
                                                request = SendDocument(botToken, userId, file);
                                                break;
                                        }
                                        break;
                                }
                                const jqSelf = $(this);

                                const setPromise = request.promise.then(function (data) {
                                    jqSelf.data("file_id", data.fileId).data("preview_id", data.previewId);
                                    SetFileHTML(botToken, jqFileHolder[0], data.previewId, data.fileId, false);
                                },
                                function (jqXHR, textStatus) {
                                    let errMsg = textStatus;
                                    if (jqXHR.responseJSON) {
                                        errMsg = jqXHR.responseJSON.description;
                                    }
                                    jqFileHolder.children(".spinner-border").first().removeClass("spinner-border text-secondary").
                                        addClass("oi oi-x text-danger").text(errMsg);
                                });

                                const abortRequest = function () {
                                    request.abort();
                                    setPromise.always(function () {
                                        jqFileHolder.children("span.oi-x:contains(abort)").remove();
                                    });
                                };
                                jqSelf.one("change", abortRequest);
                                const modal = jqFileHolder.closest(".modal").one("hide.bs.modal", abortRequest);
                                const jqRemoveFileBtn = jqSelf.closest(".input-group").find(".base-remove-file").one("click", abortRequest);
                                request.promise.always(function () {
                                    jqSelf.off("change", abortRequest);
                                    modal.off("hide.bs.modal", abortRequest);
                                    jqRemoveFileBtn.off("click", abortRequest);
                                });
                            }
                            else {
                                $(this).data("file_id", null).data("preview_id", null);
                                jqFileHolder.append(noFileSelectedSpan.clone());
                                jqFileInputDiv.children(".custom-file-label").text(chooseFile);
                            }
                        }),
                        $("<label>").addClass("custom-file-label overflow-hidden").text(chooseFile)
                    ]),
                    $("<div>").addClass("input-group-append").append([
                        $("<button>").attr({
                            class: "btn btn-outline-secondary dropdown-toggle base-file-type",
                            type: "button",
                            "data-toggle": "dropdown",
                            "aria-haspopup": true,
                            "aria-expanded": false
                        }).text("Auto"),
                        $("<div>").addClass("dropdown-menu").append([
                            $("<a>").addClass("dropdown-item").attr("href", "#").text("Image").
                                on("click", function () { changeInputType.call(this, "Image") }),
                            $("<a>").addClass("dropdown-item").attr("href", "#").text("Audio").
                                on("click", function () { changeInputType.call(this, "Audio") }),
                            $("<a>").addClass("dropdown-item").attr("href", "#").text("Video").
                                on("click", function () { changeInputType.call(this, "Video") }),
                            $("<a>").addClass("dropdown-item").attr("href", "#").text("Document").
                                on("click", function () { changeInputType.call(this, "Document") }),
                            $("<div>").addClass("dropdown-divider").attr("role", "separator"),
                            $("<a>").addClass("dropdown-item").attr("href", "#").text("Auto").
                                on("click", function () { changeInputType.call(this, "Auto") })
                        ])
                    ])
                ])
            ]),
            $("<div>").addClass("col").append($("<textarea>").attr({
                class: "form-control base-message h-100",
                rows: 10,
                cols: 50,
                placeholder: "Place for text message"
            }).css("resize", "none"))
        ])
    ]))
])));

/**
 * Клонирует div с input'ами, вставляя его родителю с изменёнными параметрами.
 * @param {JQuery<HTMLElement>} inputDiv Элемент, который нужно скопировать.
 * @param {any} inputValue Новое значение input'а.
 * @param {string} labelText Новый текст надписи.
 */
function CloneInputDiv(inputDiv, inputValue, labelText) {
    inputDiv.clone().
        children("input").attr("value", inputValue).end().
        children("label").text(labelText).end().
        appendTo(inputDiv.parent());
}

/** Базовый класс параметров узлов. */
class NodeParams {
    /**
     * Создаёт обычный набор параметров.
     * @param {number} type Число, соответствующее типу узла.
     * @param {string} name Имя узла, которое будет отображаться на кнопке.
     * @param {string} message Сообщение, которое пользователь увидит при переходе на узел.
     * @param {string} fileId ID файла (если есть), прикреплённого к сообщению.
     */
    constructor(type, name, message, fileId) {
        /** Число, соответствующее типу узла. */
        this.type = type;
        /** Имя узла, которое будет отображаться на кнопке. */
        this.name = name;
        /** Сообщение, которое пользователь увидит при переходе на узел. */
        this.message = message;
        /** ID файла (если есть), прикреплённого к сообщению. */
        this.fileId = fileId;
    }

    /**
     * Делает текущий узел шаблонным.
     * @returns Возвращает текущий класс.
     */
    makeTemplate() {
        /** Является ли узел шаблонным. */
        this.isTemplate = true;
        return this;
    }

    /** Прикреплённое модальное окно для этого типа параметров. */
    get modal() {
        return baseModal;
    }

    /**
     * Открывает форму для редактирования параметров.
     * @returns Возвращает форму.
     */
    openModal() {
        const self = this;
        this.modal.find(".modal-title").val(this.name).end().
            find(".base-message").val(this.message).end().
            find(".base-file").data("file_id", this.fileId).end().
            find(".base-file").data("preview_id", this.previewId).end().
            find(".fileHolder").empty().append(noFileSelectedSpan.clone()).end().
            find(".base-file").val("").end().
            find(".custom-file-label").text(chooseFile).end().
            modal("show");
        if (this.fileId != null) {
            const jqFileHolder = this.modal.find(".fileHolder");
            jqFileHolder.empty().append($("<span>").addClass("spinner-border text-secondary"));
            SetFileHTML(botToken, jqFileHolder[0], this.previewId, this.fileId, false).then(function () {
                self.modal.find(".custom-file-label").text("Uploaded file");
            });
        }
        this.modal.one("hide.bs.modal", function () {
            self.name = self.modal.find(".modal-title").val();
            self.message = self.modal.find(".base-message").val();
            self.fileId = self.modal.find(".base-file").data("file_id");
            self.previewId = self.modal.find(".base-file").data("preview_id");
        });
        return this.modal;
    }
}

/**Базовый класс для всех узлов.*/
class TreeNode {
    /**
     * Создаёт обычный узел.
     * @param {NodeParams} parameters Параметры узла.
     */
    constructor(parameters) {
        /** Набор параметров узла. */
        this.parameters = parameters;

        this.container = document.createElement("div");

        let nodeElement = document.createElement("div");
        nodeElement.className = "node bg" + parameters.type;
        $(nodeElement).draggable({
            cancel: ".btn",
            revert: "invalid",
            containment: "document",
            cursorAt: { top: 50, left: 50 },
            helper: "clone",
            cursor: "move",
            zIndex: 1000,
            opacity: 0.75,
            start: function (event, ui) {
                //ui.helper.data('dropped', false);
                if (this.parameters.isTemplate) {
                    selectedNode = this.cloneNode();
                }
                else {
                    selectedNode = this;
                    let jqWrCont = $(this.wrapper.container);
                    jqWrCont.children(".childContainer").addClass("transparent50");
                    jqWrCont.find(".ui-droppable").not(".verticalFillArrow:first").droppable("disable");
                }
            }.bind(this),
            stop: function (event, ui) {
                if (selectedNode == this) {
                    selectedNode = null;
                }
                if (!this.parameters.isTemplate) {
                    let jqWrCont = $(this.wrapper.container);
                    jqWrCont.children(".childContainer").removeClass("transparent50");
                    jqWrCont.find(".ui-droppable").droppable("enable");
                }
            }.bind(this)
        });

        this.nodeName = document.createElement("p");
        this.nodeName.innerText = this.parameters.name;
        this.nodeName.className = "nodeName";

        nodeElement.appendChild(this.nodeName);
        this.container.appendChild(nodeElement);

        // Шаблонам не нужны стрелки и другие дополнительные элементы.
        if (!parameters.isTemplate) {
            /** Родительский узел.
             * @type {TreeNode}
             */
            this.parent = null;
            /**
             * Обёртка первого из узлов-наследников.
             * @type {NodeWrapper}
             */
            this.firstChild = null;
            /**
             * Обёртка последнего из узлов-наследников.
             * @type {NodeWrapper}
             */
            this.lastChild = null;

            let buttonsDiv = document.createElement("div");
            buttonsDiv.className = "buttonsDiv";
            this.deleteBtn = document.createElement("button");
            this.deleteBtn.className = "btn btn-outline-danger nodeButton";
            let trashSpan = document.createElement("span");
            trashSpan.className = "oi oi-trash";
            trashSpan.style = "margin-left: 2px";
            this.editBtn = document.createElement("button");
            this.editBtn.className = "btn btn-outline-primary nodeButton";
            let self = this;
            this.editBtn.onclick = function () {
                self.parameters.openModal().one("hide.bs.modal", function () {
                    self.nodeName.innerText = self.parameters.name;
                });
            }
            let wrenchSpan = document.createElement("span");
            wrenchSpan.className = "oi oi-wrench";

            let appenderDiv = document.createElement("div");
            appenderDiv.className = "appenderDiv";

            let arrowsDiv = document.createElement("div");
            arrowsDiv.className = "arrows";
            let vertAdderArr = document.createElement("div");
            vertAdderArr.className = "baseArrow verticalAdderArrow";
            let horizAdderArr = document.createElement("div");
            horizAdderArr.className = "baseArrow horizontalAdderArrow";

            let appenderHolder = document.createElement("div");
            appenderHolder.className = "nodeHolder";

            let dropOptions = {
                accept: ".node",
                tolerance: "pointer",
                drop: function (event, ui) {
                    if (this.checkAddPermission(selectedNode)) {
                        this.appendChild(selectedNode);
                        //ui.helper.data('dropped', true);
                    }
                    else {
                        alert("Ошибка! Невозможно добавить узел.");
                    }
                }.bind(this)
            };

            $(vertAdderArr).droppable(dropOptions).on("dropover", function (event, ui) {
                if (this.lastChild != null) {
                    $(this.lastChild.container).
                        children(".arrows").children(".verticalFillArrow").addClass("hoveredArrow");
                }
            }.bind(this)).on("dropout drop", function (event, ui) {
                if (this.lastChild != null) {
                    $(this.lastChild.container).
                        children(".arrows").children(".verticalFillArrow").removeClass("hoveredArrow");
                }
            }.bind(this));
            $(horizAdderArr).droppable(dropOptions);
            $(appenderHolder).droppable(dropOptions);

            buttonsDiv.appendChild(this.deleteBtn);
            this.deleteBtn.appendChild(trashSpan);
            buttonsDiv.appendChild(this.editBtn);
            this.editBtn.appendChild(wrenchSpan);
            nodeElement.appendChild(buttonsDiv);
            arrowsDiv.appendChild(vertAdderArr);
            arrowsDiv.appendChild(horizAdderArr);
            appenderDiv.appendChild(arrowsDiv);
            appenderDiv.appendChild(appenderHolder);
            this.container.appendChild(appenderDiv);

            /** Обёртка текущего узла. */
            this.wrapper = new NodeWrapper(this);
        }
        else {
            nodeElement.className += " template";
        }
    }

    /**
     * Проверяет, можно ли добавить узел в текущую ветвь.
     * @param {TreeNode} node Добавляемый узел.
     * @returns {boolean} Возвращает true, если можно, иначе false.
     */
    checkBranchPermission(node) {
        let tmpParent = this;

        while (tmpParent != null) {
            if (tmpParent == node) {
                return false;
            }
            tmpParent = tmpParent.parent;
        }

        return true;
    }

    /**
     * Проверяет, можно ли добавить узел.
     * @param {TreeNode} node Добавляемый узел.
     * @returns {boolean} Возвращает true, если можно, иначе false.
     */
    checkAddPermission(node) {
        return this.checkBranchPermission(node);
    }

    /**
     * Проверяет, может ли текущий узел быть промежуточным.
     * @param {TreeNode} node Узел, перед которым необходимо добавить текущий как промежуточный.
     * @returns {boolean} Возвращает true, если можно, иначе false.
     */
    canBeMiddle(node) {
        return true;
    }

    /**
     * Проверяет, может ли текущий узел быть групповым.
     * @param {TreeNode} node Узел, перед которым необходимо добавить текущий как групповой.
     * @returns {boolean} Возвращает true, если можно, иначе false.
     */
    canBeGroup(node) {
        return true;
    }

    /** Удаляет текущий узел и всех его детей рекурсивно. */
    remove() {
        while (this.lastChild != null) {
            this.removeChild(this.lastChild.node);
        }
        $(this.container).fadeOut("slow", $(this).remove);
    }

    /**
     * Клонирует узел без родителя и детей.
     * @returns {TreeNode} Возвращает новый узел с такими же параметрами, но не шаблонный.
     */
    cloneNode() {
        const parameters = deepClone(this.parameters);
        delete parameters.isTemplate;
        return new TreeNode(parameters);
    }

    /**
     * Возвращает детей текущего узла.
     * @returns {TreeNode[]} Возвращает массив детей.
     */
    get children() {
        let childrenNodes = [];
        let tmpChildWrapper = this.firstChild;
        while (tmpChildWrapper != null) {
            childrenNodes.push(tmpChildWrapper.node);
            tmpChildWrapper = tmpChildWrapper.next;
        }

        return childrenNodes;
    }

    /**
     * Вставляет новый узел в указанное место.
     * @param {NodeWrapper} afterWrapper Обёртка, перед которой необходимо вставить узел.
     * @param {TreeNode} child Узел, который вставляется.
     */
    insertChild(afterWrapper, child) {
        let childWrapper = child.detach();
        child.parent = this;
        afterWrapper.setPrev(childWrapper);
        if (this.firstChild == afterWrapper) {
            this.firstChild = childWrapper;
        }
        this.container.insertBefore(childWrapper.container, afterWrapper.container);
    }

    /**
     * Добавляет новый узел в конец коллекции.
     * @param {TreeNode} child Узел, который добавляется.
     */
    appendChild(child) {
        let childWrapper = child.detach();
        child.parent = this;
        if (this.firstChild == null) {
            this.firstChild = childWrapper;
        }
        else {
            this.lastChild.setNext(childWrapper);
        }
        this.lastChild = childWrapper;
        $(this.container).children(".appenderDiv").before(childWrapper.container);
    }

    /**
     * Отсоединяет узел от родителя.
     * @param {TreeNode} child Узел, который отсоединяют.
     * @returns {NodeWrapper} Возвращает обёртку узла.
     */
    detachChild(child) {
        child.parent = null;
        let childWrapper = child.wrapper;

        if (this.firstChild == childWrapper) {
            this.firstChild = this.firstChild.next;
        }
        else if (this.lastChild == childWrapper) {
            this.lastChild = this.lastChild.prev;
        }

        return childWrapper.detach();
    }

    /**
     * Отсоединяет узел.
     * @returns {NodeWrapper} Возвращает обёртку узла.
     */
    detach() {
        if (this.parent == null) {
            return this.wrapper;
        }
        else {
            return this.parent.detachChild(this);
        }
    }

    /**
     * Удаляет узел.
     * @param {TreeNode} child Узел, который необходимо удалить.
     */
    removeChild(child) {
        let childWrapper = child.wrapper;

        if (this.firstChild == childWrapper) {
            this.firstChild = this.firstChild.next;
        }
        else if (this.lastChild == childWrapper) {
            this.lastChild = this.lastChild.prev;
        }

        childWrapper.detach();

        // меняет родителя для наследников удаляемого узла
        // TODO?

        childWrapper.remove();
    }

    /**
     * Добавляет промежуточный узел между родителем и ребёнком.
     * @param {NodeWrapper} afterWrapper Обёртка узла, перед которой нужно добавить промежуточный узел.
     * @param {TreeNode} child Промежуточный узел, который добавляется в качестве ребёнка текущему узлу.
     */
    addMiddleNode(afterWrapper, child) {
        this.insertChild(afterWrapper, child);
        child.appendChild(afterWrapper.node);
    }

    /**
     * Добавляет промежуточный узел между родителем и группой детей.
     * @param {NodeWrapper} afterWrapper Обёртка узла, с которой начинается формирование группы до конца массива детей.
     * @param {TreeNode} child Промежуточный узел, который добавляется в качестве ребёнка текущему узлу.
     */
    addGroupNode(afterWrapper, child) {
        this.insertChild(afterWrapper, child);
        afterWrapper.prev = child.lastChild;
        if (child.firstChild == null) {
            child.firstChild = afterWrapper;
        }
        child.lastChild = this.lastChild;
        this.lastChild = child.wrapper;
        child.wrapper.next = null;
        let tmpChildWrapper = afterWrapper;
        while (tmpChildWrapper != null) {
            tmpChildWrapper.node.parent = child;
            $(child.container).children(".appenderDiv").before(tmpChildWrapper.container);
            tmpChildWrapper = tmpChildWrapper.next;
        }
    }

    toJSON() {
        return {
            id: this.id,
            parentId: (this.parent == null) ? null : this.parent.id,
            parameters: this.parameters
        };
    }
}

/**Неудаляемый корень. */
class RootNode extends TreeNode {
    /**
     * Создаёт неудаляемый корень дерева.
     * @param {string} name Имя узла.
     * @param {string} message Сообщение узла.
     * @param {string} fileId ID файла (если есть), прикреплённого к сообщению.
     */
    constructor(name, message, fileId) {
        super(new NodeParams(1, name, message, fileId));
        this.deleteBtn.setAttribute("disabled", "");
        $(this.container).children(".node").draggable("disable");
    }

    remove() {
        throw new Error("Корень нельзя удалить!");
    }

    cloneNode() {
        throw new Error("Корень нельзя склонировать!");
    }

    canBeMiddle() {
        throw new Error("Корень нельзя превратить в промежуточный узел!");
    }

    canBeGroup() {
        throw new Error("Корень нельзя превратить в групповой узел!");
    }

    toJSON() {
        return {
            id: this.id,
            parameters: this.parameters
        };
    }
}

/** Базовый класс для узлов с одним ребёнком. */
class OneChildNode extends TreeNode {
    /**
     * Создаёт основу для узла с одним ребёнком.
     * @param {NodeParams} parameters Параметры узла.
     */
    constructor(parameters) {
        super(parameters);
    }

    cloneNode() {
        const parameters = deepClone(this.parameters);
        delete parameters.isTemplate;
        return new OneChildNode(parameters);
    }

    checkAddPermission(node) {
        if (super.checkAddPermission(node)) {
            return (this.firstChild == null);
        }
        else {
            return false;
        }
    }

    canBeMiddle(node) {
        return (this.firstChild == null || node == null);
    }

    canBeGroup(node) {
        return (node == null || (this.firstChild == null && node.wrapper.next == null));
    }
}

/** Базовый класс для узлов, которые не могут иметь детей. */
class ZeroChildNode extends TreeNode {
    /**
     * Создаёт основу для узла без детей.
     * @param {NodeParams} parameters Параметры узла.
     */
    constructor(parameters) {
        super(parameters);
    }

    cloneNode() {
        const parameters = deepClone(this.parameters);
        delete parameters.isTemplate;
        return new ZeroChildNode(parameters);
    }

    checkAddPermission() {
        return false;
    }

    canBeMiddle() {
        return false;
    }

    canBeGroup() {
        return false;
    }
}

/**Обёртка для узлов.*/
class NodeWrapper {
    /**
     * Создаёт основную обёртку узла.
     * @param {TreeNode} node Узел.
     */
    constructor(node) {
        /** Узел, который находится в этой обёртке. */
        this.node = node;
        /**
         * Предыдущая обёртка в коллекции родителя.
         * @type {NodeWrapper}
         */
        this.prev = null;
        /**
         * Следующая обёртка в коллекции родителя.
         * @type {NodeWrapper}
         */
        this.next = null;

        this.node.deleteBtn.onclick =
            /**
             * Удаляет узел с его детьми рекурсивно. Если узел имеет детей, то нужно подтвердить действие.
             * @this {NodeWrapper}
             */
            function () {
                if (this.node.firstChild == null || confirm("Вы точно хотите удалить узел вместе с его детьми?")) {
                    this.node.parent.removeChild(this.node);
                }
            }.bind(this);

        this.container = document.createElement("div");
        this.container.className = "wrapperContainer";

        let arrowsDiv = document.createElement("div");
        arrowsDiv.className = "arrows";
        let vertNodeArr = document.createElement("div");
        vertNodeArr.className = "baseArrow verticalNodeArrow";
        let horizArr = document.createElement("div");
        horizArr.className = "baseArrow horizontalAdderArrow";
        let fillArrow = document.createElement("div");
        fillArrow.className = "baseArrow verticalFillArrow";

        let outNodeFunc = function (event, ui) {
            if (this.prev != null) {
                $(this.prev.container).
                    children(".arrows").children(".verticalFillArrow").removeClass("hoveredArrow");
            }
        }.bind(this);
        
        $(vertNodeArr).droppable({
            accept: ".node",
            tolerance: "pointer",
            drop: function (event, ui) {
                outNodeFunc(event, ui);

                let parentNode = this.node.parent;
                if (parentNode.checkBranchPermission(selectedNode) && selectedNode.canBeGroup(this.node)) {
                    parentNode.addGroupNode(this, selectedNode);
                    //ui.helper.data('dropped', true);
                }
                else {
                    alert("Ошибка! Невозможно вставить групповой узел.");
                }
            }.bind(this),
            over: function (event, ui) {
                if (this.prev != null) {
                    $(this.prev.container).
                        children(".arrows").children(".verticalFillArrow").addClass("hoveredArrow");
                }
            }.bind(this),
            out: outNodeFunc
        });

        $(horizArr).droppable({
            accept: ".node",
            tolerance: "pointer",
            drop: function (event, ui) {
                let parentNode = this.node.parent;
                if (parentNode.checkBranchPermission(selectedNode) && selectedNode.canBeMiddle(this.node)) {
                    parentNode.addMiddleNode(this, selectedNode);
                    //ui.helper.data('dropped', true);
                }
                else {
                    alert("Ошибка! Невозможно вставить промежуточный узел.");
                }
            }.bind(this)
        });

        let outFillFunc = function (event, ui) {
            if (this.next != null) {
                $(this.next.container).
                    children(".arrows").children(".verticalNodeArrow").removeClass("hoveredArrow");
            }
            else {
                $(this.node.parent.container).children(".appenderDiv").
                    children(".arrows").children(".verticalAdderArrow").removeClass("hoveredArrow");
            }
        }.bind(this);

        $(fillArrow).droppable({
            accept: ".node",
            tolerance: "pointer",
            drop: function (event, ui) {
                outFillFunc(event, ui);

                let parentNode = this.node.parent;
                if (this.next != null) {
                    if (parentNode.checkBranchPermission(selectedNode) && selectedNode.canBeGroup(this.next.node)) {
                        parentNode.addGroupNode(this.next, selectedNode);
                        //ui.helper.data('dropped', true);
                    }
                    else {
                        alert("Ошибка! Невозможно вставить групповой узел.");
                    }
                }
                else {
                    if (parentNode.checkAddPermission(selectedNode)) {
                        parentNode.appendChild(selectedNode);
                        //ui.helper.data('dropped', true);
                    }
                    else {
                        alert("Ошибка! Невозможно добавить узел.");
                    }
                }
            }.bind(this),
            over: function (event, ui) {
                if (this.next != null) {
                    $(this.next.container).
                        children(".arrows").children(".verticalNodeArrow").addClass("hoveredArrow");
                }
                else {
                    $(this.node.parent.container).children(".appenderDiv").
                        children(".arrows").children(".verticalAdderArrow").addClass("hoveredArrow");
                }
            }.bind(this),
            out: outFillFunc
        });

        let childContainer = document.createElement("div");
        childContainer.className = "childContainer";

        let inserter = document.createElement("div");
        inserter.className = "nodeHolder";
        $(inserter).droppable({
            accept: ".node",
            tolerance: "pointer",
            drop: function (event, ui) {
                let parentNode = this.node.parent;
                if (parentNode.checkAddPermission(selectedNode)) {
                    parentNode.insertChild(this, selectedNode);
                    //ui.helper.data('dropped', true);
                }
                else {
                    alert("Ошибка! Невозможно вставить узел.");
                }
            }.bind(this)
        });

        arrowsDiv.appendChild(vertNodeArr);
        arrowsDiv.appendChild(horizArr);
        arrowsDiv.appendChild(fillArrow);

        childContainer.appendChild(inserter);
        childContainer.appendChild(this.node.container);

        this.container.appendChild(arrowsDiv);
        this.container.appendChild(childContainer);
    }

    /**
     * Устанавливает обёртку перед текущей.
     * @param {NodeWrapper} wrapper Новая обёртка.
     */
    setPrev(wrapper) {
        let oldPrev = this.prev;
        this.prev = wrapper;
        if (oldPrev != null) {
            oldPrev.next = wrapper;
        }
        if (wrapper != null) {
            wrapper.prev = oldPrev;
            wrapper.next = this;
        }
    }

    /**
     * Устанавливает обёртку после текущей.
     * @param {NodeWrapper} wrapper Новая обёртка.
     */
    setNext(wrapper) {
        let oldNext = this.next;
        this.next = wrapper;
        if (oldNext != null) {
            oldNext.prev = wrapper;
        }
        if (wrapper != null) {
            wrapper.next = oldNext;
            wrapper.prev = this;
        }
    }

    /**
     * Отсоединяет текущую обёртку.
     * @returns {NodeWrapper} Возвращает обёртку.
     */
    detach() {
        if (this.prev != null) {
            this.prev.next = this.next;
        }

        if (this.next != null) {
            this.next.prev = this.prev;
        }

        this.prev = null;
        this.next = null;

        return this;
    }

    /** Полностью удаляет обёртку вместе с узлом. */
    remove() {
        this.node.remove();
        $(this.container).slideUp("slow", $(this).remove);
    }
}