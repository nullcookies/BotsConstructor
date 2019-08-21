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

const sectionModal = baseModal.clone(true).find(".modal-body > form").append($("<fieldset>").
    addClass("form-group collection-set mb-0").append($("<div>").addClass("row").append([
        $("<legend>").addClass("col-form-label col-auto").text("Collection type"),
        $("<div>").addClass("form-check form-check-inline col-auto").append([
            $("<input>").attr({
                type: "radio",
                name: "collection-type",
                value: collectionTypes.block,
                class: "form-check-input"
            }).prop("checked", true),
            $("<label>").addClass("form-check-label").text("Block node"),
        ]),
        $("<div>").addClass("form-check form-check-inline col-auto").append([
            $("<input>").attr({
                type: "radio",
                name: "collection-type",
                value: collectionTypes.flipper,
                class: "form-check-input"
            }),
            $("<label>").addClass("form-check-label").text("Flipper node")
        ])
    ]))).end();

/** Параметры для узла-секции. */
class SectionParams extends NodeParams {
    /**
     * Создаёт параметры узла-секции.
     * @param {string} name Название узла.
     * @param {string} message Сообщение узла.
     * @param {number} collectionType Число, которое обозначает тип отправки коллекции.
     * @param {string} fileId ID файла (если есть), прикреплённого к сообщению.
     */
    constructor(name, message, collectionType, fileId) {
        super(nodeTypes.section, name, message, fileId);
        /** Число, которое обозначает тип отправки коллекции. */
        this.collType = collectionType;
    }

    get modal() {
        return sectionModal;
    }

    openModal() {
        const self = this;
        const modal = super.openModal();
        modal.find("input[name=collection-type]").prop("checked", false).filter("[value=" + this.collType + "]").prop("checked", true);
        modal.one("hide.bs.modal", function () {
            self.collType = modal.find("input[name=collection-type]:checked").val();
        });

        return modal;
    }
}

/** Характеристика товара. */
class ProductProperty {
    /**
     * Создаёт характеристику товара.
     * @param {string} name Название характеристики.
     * @param {string[]} types Виды данной характеристики.
     * @param {string} message Сообщение (описание) характеристики (если есть).
     * @param {string} fileId ID файла (если есть), прикреплённого к сообщению.
     */
    constructor(name, types, message, fileId) {
        /** Название характеристики. */
        this.name = name;
        /** Виды данной характеристики. */
        this.types = types;
        /** Сообщение (описание) характеристики (если есть). */
        this.message = message;
        /** ID файла (если есть), прикреплённого к сообщению. */
        this.fileId = fileId;
    }
}

/** Параметры для продуктового узла. */
class ProductParams extends NodeParams {
    /**
     * Создаёт параметры товарного узла.
     * @param {string} name Название узла.
     * @param {string} message Сообщение узла.
     * @param {ProductProperty[]} properties Характеристики товара.
     * @param {string} fileId ID файла (если есть), прикреплённого к сообщению.
     */
    constructor(name, message, properties, fileId) {
        super(nodeTypes.product, name, message, fileId);
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
class InputParams extends NodeParams {
    /**
     * Создаёт параметры для input-узла.
     * @param {string} name Название узла.
     * @param {string} message Сообщение узла.
     * @param {number} inputType Число, соответствующее типу input-узла.
     * @param {string} fileId ID файла (если есть), прикреплённого к сообщению.
     */
    constructor(name, message, inputType, fileId) {
        super(nodeTypes.input, name, message, fileId);
        /** Число, соответствующее типу input-узла. */
        this.inputType = inputType;
    }
}

/**
 * Шаблонные узлы.
 * @type {ReadonlyArray.<TreeNode>}
 */
const templates = Object.freeze([
    undefined,
    new RootNode("Корень", "Добро пожаловать в начало!", null),
    new TreeNode(new NodeParams(nodeTypes.info, "Инфо-узел", "Тут может быть любая информация для пользователя.", null).makeTemplate()),
    new TreeNode(new SectionParams("Раздел", "Этот узел позволяет удобно работать с большим количеством детских узлов.", collectionTypes.block, null).makeTemplate()),
    new TreeNode(new ProductParams("Товар", "Тут можно настроить цены товаров с разными подтипами.", [
        new ProductProperty("Характеристика 1", ["Подтип 1", "Подтип 2", "Подтип 3"], null, null),
        new ProductProperty("Характеристика 2", ["Подвид 1", "Подвид 2", "Подвид 3"], null, null)
    ], null).makeTemplate()),
    new OneChildNode(new InputParams("Ввод данных", "Тут пользователь должен ввести данные нужного типа.", inputTypes.text, null).makeTemplate()),
    new OneChildNode(new NodeParams(nodeTypes.sendOrder, "Отправить заказ", "При переходе сюда сформированный заказ отправляется Вам.", null).makeTemplate())
]);