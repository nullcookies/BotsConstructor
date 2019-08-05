"use strict";
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

        this.nodeElement = document.createElement("div");
        this.nodeElement.className = "node";
        this.nodeElement.id = "node_" + this.id;

        this.nodeName = document.createElement("p");
        this.nodeName.innerText = this.parameters.name;
        this.nodeName.className = "nodeName";

        this.nodeElement.appendChild(this.nodeName);
        this.container.appendChild(this.nodeElement);

        this.appenderDiv = document.createElement("div");
        this.appenderDiv.className = "appenderDiv";

        this.arrowsDiv = document.createElement("div");
        this.arrowsDiv.className = "arrows";
        this.vertAdderArr = document.createElement("div");
        this.vertAdderArr.className = "verticalAdderArrow";
        this.horizAdderArr = document.createElement("div");
        this.horizAdderArr.className = "horizontalAdderArrow";

        this.appenderHolder = document.createElement("div");
        this.appenderHolder.className = "nodeHolder";

        this.arrowsDiv.appendChild(this.vertAdderArr);
        this.arrowsDiv.appendChild(this.horizAdderArr);
        this.appenderDiv.appendChild(this.arrowsDiv);
        this.appenderDiv.appendChild(this.appenderHolder);
        this.container.appendChild(this.appenderDiv);
    }

    /** Генерирует ID для узла и добавляет его в список всех узлов, если ID ещё нет. */
    addId() {
        if (this.id == undefined) {
            this.id = nextId;
            nextId++;
            allNodes[this.id] = this;
        }
    }

    /** Удаляет текущий узел. */
    remove() {
        delete allNodes[this.id];
        this.container.remove();
    }

    /**
     * Клонирует узел без родителя и детей.
     * @returns Возвращает новый узел с таким же сообщением и названием.
     */
    cloneNode() {
        return new TreeNode(this.name, this.message, null, []);
    }

    /** Возвращает детей текущего узла.
     * @returns Возвращает массив детей.
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
        child.addId();
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
        child.addId();
        child.parent = this;
        this.childrenWrappers.push(child.createWrapper(this.childrenWrappers.length));
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
            child.childrenWrappers[c].depth--;
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
        child.addId();
        child.parent = this;
        let oldWrapper = this.childrenWrappers[index];
        oldWrapper.node.parent = child;
        oldWrapper.depth++;
        oldWrapper.index = 0;
        this.childrenWrappers[index] = child.createWrapper(index);
    }

    /**
     * Добавляет промежуточный узел между родителем и группой детей.
     * @param {number} index Индекс узла, с которого начинается формирование группы до конца массива детей.
     * @param {TreeNode} child Промежуточный узел, который добавляется в качестве ребёнка текущему узлу.
     */
    addGroupNode(index, child) {
        child.addId();
        child.parent = this;
        let newIndex = 0;
        for (let i = index; i < this.childrenWrappers.length; i++) {
            let oldWrapper = this.childrenWrappers[i];
            oldWrapper.node.parent = child;
            oldWrapper.depth++;
            oldWrapper.index = newIndex;
            newIndex++;
        }
        this.childrenWrappers.splice(index, this.childrenWrappers.length, child.createWrapper(index));
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
        let prevNode = node.parent;
        let currentDepth = 0;
        while (prevNode != null) {
            currentDepth++;
            prevNode = prevNode.parent;
        }
        this.depth = currentDepth;
    }

    /**Полностью удаляет обёртку вместе с узлом. */
    remove() {
        this.node.remove();
    }

    /**
     * Устанавливает глубину для узла, а также меняет её для детей.
     * @param {number} newDepth Новая глубина узла.
     */
    set depth(newDepth) {
        this.depth = newDepth;
        let childDepth = newDepth + 1;
        for (let i = 0; i < this.node.childrenWrappers.length; i++) {
            this.node.childrenWrappers[i].depth = childDepth;
        }
    }
}