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

/** Базовый класс параметров узлов. */
class NodeParams {
    /**
     * Создаёт обычный набор параметров.
     * @param {number} type Число, соответствующее типу узла.
     * @param {string} name Имя узла, которое будет отображаться на кнопке.
     * @param {string} message Сообщение, которое пользователь увидит при переходе на узел.
     */
    constructor(type, name, message) {
        /** Число, соответствующее типу узла. */
        this.type = type;
        /** Имя узла, которое будет отображаться на кнопке. */
        this.name = name;
        /** Сообщение, которое пользователь увидит при переходе на узел. */
        this.message = message;
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

    /** Открывает форму для редактирования параметров. */
    openModal() {
        alert(this.name);
        //TODO
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
                    jqWrCont.find(".ui-droppable").not("verticalFillArrow:first").droppable("disable");
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
                let jqWrCont = $(this.wrapper.container);
                jqWrCont.children(".childContainer").removeClass("transparent50");
                jqWrCont.find(".ui-droppable").droppable("enable");
            }.bind(this)
        });

        let nodeName = document.createElement("p");
        nodeName.innerText = this.parameters.name;
        nodeName.className = "nodeName";

        nodeElement.appendChild(nodeName);
        this.container.appendChild(nodeElement);

        // Шаблонам не нужны стрелки и другие дополнительные элементы.
        if (!parameters.isTemplate) {
            /** Родительский узел.
             * @type {TreeNode}
             */
            this.parent = null;
            /** Обёртки узлов-наследников.
             * @type {NodeWrapper[]}
             */
            this.childrenWrappers = [];

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
            this.editBtn.onclick = this.parameters.openModal.bind(this.parameters);
            let wrenchSpan = document.createElement("span");
            wrenchSpan.className = "oi oi-wrench";

            let appenderDiv = document.createElement("div");
            appenderDiv.className = "appenderDiv";

            let arrowsDiv = document.createElement("div");
            arrowsDiv.className = "arrows";
            let vertAdderArr = document.createElement("div");
            vertAdderArr.className = "verticalAdderArrow";
            let horizAdderArr = document.createElement("div");
            horizAdderArr.className = "horizontalAdderArrow";

            let appenderHolder = document.createElement("div");
            appenderHolder.className = "nodeHolder";
            $(appenderHolder).droppable({
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
            });

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
        for (let i = this.childrenWrappers.length - 1; i >= 0; i--) {
            this.removeChild(i);
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

    /** Возвращает детей текущего узла.
     * @returns {TreeNode[]} Возвращает массив детей.
    */
    get children() {
        return this.childrenWrappers.map(function (wrapper) {
            return wrapper.node;
        });
    }

    /**
     * Вставляет новый узел в указанное место.
     * @param {number} index Индекс узла.
     * @param {TreeNode} child Узел, который вставляется.
     */
    insertChild(index, child) {
        child.parent = this;
        let childWrapper = child.wrapper.setIndex(index);
        this.childrenWrappers.splice(index, 0, childWrapper);

        for (let i = index + 1; i < this.childrenWrappers.length; i++) {
            this.childrenWrappers[i].index = i;
        }

        $(this.childrenWrappers[index].container).before(childWrapper.container);
    }

    /**
     * Добавляет новый узел в конец коллекции.
     * @param {TreeNode} child Узел, который добавляется.
     */
    appendChild(child) {
        child.parent = this;
        let childWrapper = child.wrapper.setIndex(this.childrenWrappers.length);
        this.childrenWrappers.push(childWrapper);
        this.container.insertBefore(childWrapper.container, this.container.lastChild);
    }

    /**
     * Удаляет узел.
     * @param {number} index Индекс узла.
     */
    removeChild(index) {
        let childWrapper = this.childrenWrappers[index];
        this.childrenWrappers.splice(index, 1);

        // меняет родителя для наследников удаляемого узла
        //let child = childWrapper.node;
        //this.childrenWrappers.splice(index, 1, child.childrenWrappers);
        //for (let c = 0; c < child.childrenWrappers.length; c++) {
        //    child.childrenWrappers[c].node.parent = this;
        //}

        for (let i = index; i < this.childrenWrappers.length; i++) {
            this.childrenWrappers[i].index = i;
        }

        childWrapper.remove();
    }

    /**
     * Добавляет промежуточный узел между родителем и ребёнком.
     * @param {number} index Индекс узла, где нужно добавить промежуточный узел.
     * @param {TreeNode} child Промежуточный узел, который добавляется в качестве ребёнка текущему узлу.
     */
    addMiddleNode(index, child) {
        child.parent = this;
        let childWrapper = child.wrapper.setIndex(index);
        let oldWrapper = this.childrenWrappers[index];
        oldWrapper.node.parent = child;
        oldWrapper.index = 0;
        this.childrenWrappers[index] = childWrapper;
        child.childrenWrappers.push(oldWrapper);
        $(oldWrapper.container).before(childWrapper.container);
        $(child.container.lastChild).before($(oldWrapper.container).detach());
    }

    /**
     * Добавляет промежуточный узел между родителем и группой детей.
     * @param {number} index Индекс узла, с которого начинается формирование группы до конца массива детей.
     * @param {TreeNode} child Промежуточный узел, который добавляется в качестве ребёнка текущему узлу.
     */
    addGroupNode(index, child) {
        child.parent = this;
        let childWrapper = child.wrapper.setIndex(index);
        let newIndex = 0;
        $(this.childrenWrappers[index].container).before(childWrapper.container);
        for (let i = index; i < this.childrenWrappers.length; i++) {
            let oldWrapper = this.childrenWrappers[i];
            oldWrapper.node.parent = child;
            oldWrapper.index = newIndex;
            child.childrenWrappers.push(oldWrapper);
            newIndex++;
            $(child.container.lastChild).before($(oldWrapper.container).detach());
        }
        this.childrenWrappers.splice(index, this.childrenWrappers.length, childWrapper);
    }
}

/**Неудаляемый корень. */
class RootNode extends TreeNode {
    /**
     * Создаёт неудаляемый корень дерева.
     * @param {string} name Имя узла.
     * @param {string} message Сообщение узла.
     */
    constructor(name, message) {
        super(new NodeParams(1, name, message));
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
        if (this.childrenWrappers.length == 0) {
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
        /** Индекс узла в коллекции родителя. */
        this.index = 0;
        /** Узел, который находится в этой обёртке. */
        this.node = node;

        this.node.deleteBtn.onclick =
            /**
             * Удаляет узел с его детьми рекурсивно. Если узел имеет детей, то нужно подтвердить действие.
             * @this {NodeWrapper}
             */
            function () {
                if (this.node.childrenWrappers.length == 0 || confirm("Вы точно хотите удалить узел вместе с его детьми?")) {
                    this.node.parent.removeChild(this.index);
                }
            }.bind(this);

        this.container = document.createElement("div");
        this.container.className = "wrapperContainer";

        let arrowsDiv = document.createElement("div");
        arrowsDiv.className = "arrows";
        let vertNodeArr = document.createElement("div");
        vertNodeArr.className = "verticalNodeArrow";
        let horizArr = document.createElement("div");
        horizArr.className = "horizontalAdderArrow";
        let fillArrow = document.createElement("div");
        fillArrow.className = "verticalFillArrow";

        let childContainer = document.createElement("div");
        childContainer.className = "childContainer";

        let inserter = document.createElement("div");
        inserter.className = "nodeHolder";

        arrowsDiv.appendChild(vertNodeArr);
        arrowsDiv.appendChild(horizArr);
        arrowsDiv.appendChild(fillArrow);

        childContainer.appendChild(inserter);
        childContainer.appendChild(this.node.container);

        this.container.appendChild(arrowsDiv);
        this.container.appendChild(childContainer);
    }

    /**
     * Устанавливает индекс узла в коллекции родителя.
     * @param {number} index Индекс узла.
     * @returns {NodeWrapper} Возвращает текущую обёртку.
     */
    setIndex(index) {
        this.index = index;
        return this;
    }

    /** Полностью удаляет обёртку вместе с узлом. */
    remove() {
        this.node.remove();
        $(this.container).slideUp("slow", $(this).remove);
        //this.container.remove();
    }
}