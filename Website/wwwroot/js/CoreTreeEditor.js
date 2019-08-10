"use strict";

function isObject(obj) {
    var type = typeof obj;
    return type === 'function' || type === 'object' && !!obj;
};

/**
 * Выполняет глубокое копирование объекта.
 * @template T Тип объекта.
 * @param {T} src Объект, который необходимо скопировать.
 * @returns {T} Возвращает клон объекта.
 */
function deepClone(src) {
    let clone = {};
    for (let prop in src) {
        if (src.hasOwnProperty(prop)) {
            if (isObject(src[prop])) {
                clone[prop] = deepClone(src[prop]);
            } else {
                clone[prop] = src[prop];
            }
        }
    }
    return clone;
}

/*
 * TreeNode имеет "добавитель" и множество обёрток, которые содержат в себе дочерние узлы, места для вставки и стрелки.
 * Каждая обёртка "знает" свой индекс в массиве детей родителя. При создании, обёртка цепляет слушатели событий.
 * Индекс обёртки может меняться при вставке и удалении узлов. Узлы в меню имеют свою собственную обёртку.
 */

/** Следующий ID для нового узла. */
let nextId = 0;
/** @type {Object.<number, TreeNode>} */
const allNodes = {};

/** Базовый класс параметров узлов. */
class BaseParams {
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
     * @returns Возвращает текущее свойство.
     */
    makeTemplate() {
        /** Является ли узел шаблонным. */
        this.isTemplate = true;
        return this;
    }
}

/**Базовый класс для всех узлов.*/
class TreeNode {
    /**
     * Создаёт обычный узел.
     * @param {BaseParams} parameters Параметры узла.
     */
    constructor(parameters) {
        /** Набор параметров узла. */
        this.parameters = parameters;
        /** Родительский узел.
         * @type {TreeNode}
         */
        this.parent = null;
        /** Обёртки узлов-наследников.
         * @type {NodeWrapper[]}
         */
        this.childrenWrappers = [];

        this.container = document.createElement("div");

        let nodeElement = document.createElement("div");
        nodeElement.className = "node bg" + parameters.type;

        let nodeName = document.createElement("p");
        nodeName.innerText = this.parameters.name;
        nodeName.className = "nodeName";

        nodeElement.appendChild(nodeName);
        this.container.appendChild(nodeElement);

        // Шаблонам не нужны стрелки и другие дополнительные элементы.
        if (!parameters.isTemplate) {
            this.id = nextId;
            nextId++;
            allNodes[this.id] = this;

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

            arrowsDiv.appendChild(vertAdderArr);
            arrowsDiv.appendChild(horizAdderArr);
            appenderDiv.appendChild(arrowsDiv);
            appenderDiv.appendChild(appenderHolder);
            this.container.appendChild(appenderDiv);
        }
        else {
            this.container.firstElementChild.className += " template";
        }
    }

    /** Удаляет текущий узел и всех его детей рекурсивно. */
    remove() {
        delete allNodes[this.id];
        while (this.childrenWrappers.length > 0) {
            this.childrenWrappers.pop().remove();
        }
        this.container.remove();
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
     * Создаёт обёртку узла.
     * @param {number} index Индекс узла.
     * @returns {NodeWrapper} Возвращает обёртку для узла.
     */
    createWrapper(index) {
        return new NodeWrapper(index, this);
    }

    /**
     * Вставляет новый узел в указанное место.
     * @param {number} index Индекс узла.
     * @param {TreeNode} child Узел, который вставляется.
     */
    insertChild(index, child) {
        child.parent = this;
        this.childrenWrappers.splice(index, 0, child.createWrapper(index));

        for (let i = index + 1; i < this.childrenWrappers.length; i++) {
            this.childrenWrappers[i].index = i;
        }
    }

    /**
     * Добавляет новый узел в конец коллекции.
     * @param {TreeNode} child Узел, который добавляется.
     */
    appendChild(child) {
        child.parent = this;
        let childWrapper = child.createWrapper(this.childrenWrappers.length);
        this.childrenWrappers.push(childWrapper);
        this.container.insertBefore(childWrapper.container, this.container.lastChild);
    }

    /**
     * Удаляет узел.
     * @param {number} index Индекс узла.
     */
    removeChild(index) {
        let childWrapper = this.childrenWrappers[index];
        let child = childWrapper.node;
        this.childrenWrappers.splice(index, 1, child.childrenWrappers);
        // меняет родителя для наследников удаляемого узла
        for (let c = 0; c < child.childrenWrappers.length; c++) {
            child.childrenWrappers[c].node.parent = this;
        }

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
        let oldWrapper = this.childrenWrappers[index];
        oldWrapper.node.parent = child;
        oldWrapper.index = 0;
        this.childrenWrappers[index] = child.createWrapper(index);
    }

    /**
     * Добавляет промежуточный узел между родителем и группой детей.
     * @param {number} index Индекс узла, с которого начинается формирование группы до конца массива детей.
     * @param {TreeNode} child Промежуточный узел, который добавляется в качестве ребёнка текущему узлу.
     */
    addGroupNode(index, child) {
        child.parent = this;
        let newIndex = 0;
        for (let i = index; i < this.childrenWrappers.length; i++) {
            let oldWrapper = this.childrenWrappers[i];
            oldWrapper.node.parent = child;
            oldWrapper.index = newIndex;
            newIndex++;
        }
        this.childrenWrappers.splice(index, this.childrenWrappers.length, child.createWrapper(index));
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
        super(new BaseParams(1, name, message));
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
     * @param {BaseParams} parameters Параметры узла.
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
     * @param {number} index Индекс узла в коллекции родителя.
     * @param {TreeNode} node Узел.
     */
    constructor(index, node) {
        this.index = index;
        this.node = node;
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

    /**Полностью удаляет обёртку вместе с узлом. */
    remove() {
        this.node.remove();
        this.container.remove();
    }
}