"use strict";

const nodeTypes = Object.freeze({
    "root": 1,
    "info": 2,
    "section": 3,
    "product": 4,
    "input": 5,
    "sendOrder": 6
});

const collectionTypes = Object.freeze({
    "block": 1,
    "flipper": 2
});

const inputTypes = Object.freeze({
    "text": 1,
    "time": 2,
    "image": 3,
    "audio": 4,
    "video": 5,
    "document": 6
});

/** Параметры для узла-секции. */
class SectionParams extends BaseParams {
    /**
     * Создаёт параметры узла-секции.
     * @param {string} name Название узла.
     * @param {string} message Сообщение узла.
     * @param {number} collectionType Число, которое обозначает тип отправки коллекции.
     */
    constructor(name, message, collectionType) {
        super(nodeTypes.section, name, message);
        /** Число, которое обозначает тип отправки коллекции. */
        this.collType = collectionType;
    }
}

/** Характеристика товара. */
class ProductProperty {
    /**
     * Создаёт характеристику товара.
     * @param {string} name Название характеристики.
     * @param {string[]} types Виды данной характеристики.
     */
    constructor(name, types) {
        /** Название характеристики. */
        this.name = name;
        /** Виды данной характеристики. */
        this.types = types;
    }
}

/** Параметры для продуктового узла. */
class ProductParams extends BaseParams {
    /**
     * Создаёт параметры товарного узла.
     * @param {string} name Название узла.
     * @param {string} message Сообщение узла.
     * @param {ProductProperty[]} properties Характеристики товара.
     */
    constructor(name, message, properties) {
        super(nodeTypes.product, name, message);
        /** Характеристики товара. */
        this.properties = properties;
        this.updateCount();
        /** Значения всех подтипов товара.
         * @type {number[]}
         */
        this.values = new Array(this.count).fill(0.00);
    }

    updateCount() {
        /** Количество всех комбинаций подтипов. */
        this.count = this.properties.reduce(function (previous, current) {
            return previous * current.types.length;
        }, 1);
    }

    //TODO: написать методы для красивого добавления и удаления параметров.
}

/** Параметры для input-узла. */
class InputParams extends BaseParams {
    /**
     * Создаёт параметры для input-узла.
     * @param {string} name Название узла.
     * @param {string} message Сообщение узла.
     * @param {number} inputType Число, соответствующее типу input-узла.
     * @param {boolean} isTemplate Является ли узел шаблонным.
     */
    constructor(name, message, inputType, isTemplate) {
        super(nodeTypes.input, name, message);
        /** Число, соответствующее типу input-узла. */
        this.inputType = inputType;
        this.isTemplate = isTemplate;
    }
}

/** Узел для ввода данных. */
class InputNode extends TreeNode {
    /**
     * Создаёт input-узел.
     * @param {string} name Название узла.
     * @param {string} message Сообщение узла.
     * @param {number} inputType Число, соответствующее типу input-узла.
     * @param {boolean} isTemplate Является ли узел шаблонным.
     */
    constructor(name, message, inputType, isTemplate) {
        super(new InputParams(name, message, inputType, isTemplate));
    }

    insertChild() {
        throw new Error("Input-узел может иметь только одного ребёнка!");
    }

    appendChild(child) {
        if (this.childrenWrappers.length == 0) {
            super.appendChild(child);
        }
        else {
            throw new Error("Input-узел может иметь только одного ребёнка!");
        }
    }
}

/**
 * Шаблонные узлы.
 * @type {ReadonlyArray.<TreeNode>}
 */
const templates = Object.freeze([
    undefined,
    new RootNode("Корень", "Добро пожаловать в начало!"),
    new TreeNode(new BaseParams(nodeTypes.info, "Инфо-узел", "Тут может быть любая информация для пользователя.").makeTemplate()),
    new TreeNode(new SectionParams("Раздел", "Этот узел позволяет удобно работать с большим количеством детских узлов.", collectionTypes.block).makeTemplate()),
    new TreeNode(new ProductParams("Товар", "Тут можно настроить цены товаров с разными подтипами.", [
        new ProductProperty("Характеристика 1", ["Подтип 1", "Подтип 2", "Подтип 3"]),
        new ProductProperty("Характеристика 2", ["Подвид 1", "Подвид 2", "Подвид 3"])
    ]).makeTemplate()),
    new InputNode("Ввод данных", "Тут пользователь должен ввести данные нужного типа.", 1, true),
    new TreeNode(new BaseParams(nodeTypes.sendOrder, "Отправить заказ", "При переходе сюда сформированный заказ отправляется Вам.").makeTemplate())
]);