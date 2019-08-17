"use strict";

/**
 * Выполняет глубокое копирование объекта.
 * @template T Тип объекта.
 * @param {T} src Объект, который необходимо скопировать.
 * @returns {T} Возвращает клон объекта.
 */
function deepClone(src) {
    let clone = Object.create(Object.getPrototypeOf(src));
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
 * Выбранный узел.
 * @type {TreeNode}
 */
let selectedNode = null;

/** Следующий ID для нового узла. */
let nextId = 0;
/**
 * Все узлы в дереве.
 * @type {Object.<number, TreeNode>}
 */
const allNodes = {};

/** Базовое модальное окно. */
const baseModal = $("<div>").attr({
    class: "modal fade",
    tabindex: -1,
    role: "dialog",
    "aria-hidden": true
}).append($("<div>").attr({
    class: "modal-dialog modal-dialog-centered",
    role: "document"
}).append($("<div>").addClass("modal-content").append([
    $("<div>").addClass("modal-header").append([
        $("<input>").attr({
            class: "modal-title w-100 border-0 h5",
            type: "text"
        }).val("Node name"),
        $("<button>").attr({
            class: "close",
            "data-dismiss": "modal",
            "aria-label": "Close"
        }).append($("<span>").attr("aria-hidden", "true").html('&times;'))
    ]),
    $("<div>").addClass("modal-body").append($("<form>").append([
        $("<div>").addClass("form-row").append([
            $("<div>").addClass("col").append($("<input>").attr({
                class: "form-control base-file",
                type: "file"
            })),
            $("<div>").addClass("col").append($("<input>").attr({
                class: "form-control base-message",
                type: "text"
            }))
        ])
    ]))
])));

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

    /**
     * Открывает форму для редактирования параметров.
     * @returns Возвращает форму.
     */
    openModal() {
        let self = this;
        baseModal.find(".modal-title").val(this.name).end().modal("show");
        baseModal.one("hide.bs.modal", function () {
            self.name = baseModal.find(".modal-title").val();
        });
        return baseModal;
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
                if (selectedNode != null) {
                    delete allNodes[selectedNode.id];
                    if (selectedNode.id == nextId + 1) {
                        nextId = selectedNode.id;
                    }
                }
                ui.helper.data('dropped', false);
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
                    if (!ui.helper.data('dropped')) {
                        delete allNodes[selectedNode.id];
                        if (selectedNode.id == nextId + 1) {
                            nextId = selectedNode.id;
                        }
                    }
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

            this.id = nextId;
            nextId++;
            allNodes[this.id] = this;

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
                        ui.helper.data('dropped', true);
                    }
                    else {
                        alert("Ошибка! Невозможно добавить узел в качестве ребёнка в его ветвь.");
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
     * Проверяет, можно ли добавить узел.
     * @param {TreeNode} node Добавляемый узел.
     * @returns {boolean} Возвращает true, если можно, иначе false.
     */
    checkAddPermission(node) {
        let tmpParent = this;

        while (tmpParent != null) {
            if (tmpParent == node) {
                return false;
            }
            tmpParent = tmpParent.parent;
        }

        return true;
    }

    /** Удаляет текущий узел и всех его детей рекурсивно. */
    remove() {
        while (this.lastChild != null) {
            this.removeChild(this.lastChild.node);
        }
        $(this.container).fadeOut("slow", $(this).remove);
        //this.container.remove();
        delete allNodes[this.id];
    }

    /**
     * Клонирует узел без родителя и детей.
     * @returns {TreeNode} Возвращает новый узел с такими же параметрами, но не шаблонный.
     */
    cloneNode() {
        let parameters = deepClone(this.parameters);
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

    /** Вставка узлов запрещена для этого типа узлов! */
    insertChild() {
        throw new Error("Input-узел может иметь только одного ребёнка!");
    }

    /**
     * Добавляет нового ребёнка к узлу, если текущий узел ещё не имеет детей.
     * @param {TreeNode} child Новый узел, который необходимо добавить.
     */
    appendChild(child) {
        if (this.firstChild == null) {
            super.appendChild(child);
        }
        else {
            throw new Error("Input-узел может иметь только одного ребёнка!");
        }
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
                if (parentNode.checkAddPermission(selectedNode)) {
                    parentNode.addGroupNode(this, selectedNode);
                    ui.helper.data('dropped', true);
                }
                else {
                    alert("Ошибка! Невозможно вставить групповой узел в качестве ребёнка в его ветвь.");
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
                if (parentNode.checkAddPermission(selectedNode)) {
                    parentNode.addMiddleNode(this, selectedNode);
                    ui.helper.data('dropped', true);
                }
                else {
                    alert("Ошибка! Невозможно вставить промежуточный узел в качестве ребёнка в его ветвь.");
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
                if (parentNode.checkAddPermission(selectedNode)) {
                    if (this.next != null) {
                        parentNode.addGroupNode(this.next, selectedNode);
                    }
                    else {
                        parentNode.appendChild(selectedNode);
                    }
                    ui.helper.data('dropped', true);
                }
                else {
                    alert("Ошибка! Невозможно вставить групповой узел в качестве ребёнка в его ветвь.");
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
                    ui.helper.data('dropped', true);
                }
                else {
                    alert("Ошибка! Невозможно вставить узел в качестве ребёнка в его ветвь.");
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
        //this.container.remove();
    }
}