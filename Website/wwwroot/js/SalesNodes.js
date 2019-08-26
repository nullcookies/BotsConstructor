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
            }),
            $("<label>").addClass("form-check-label").text("Block node")
        ])
    ]))).end();

CloneInputDiv(sectionModal.find("legend:contains(Collection type) ~ div.form-check"), collectionTypes.flipper, "Flipper node");

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

const baseAddBtn = $("<button>").addClass("btn btn-outline-primary rounded-circle m-auto").attr({
        style: "width: 3rem; height: 3rem",
        type: "button"
    }).append($("<span>").addClass("oi oi-plus"));

const basePropDiv = $("<div>").addClass("border border-secondary rounded bg-light w-100 my-1 p-1").height("3rem");
const productPropDiv = basePropDiv.clone().addClass("input-group").append([
    $("<div>").addClass("input-group-prepend").append($("<button>").attr("type", "button").
        addClass("btn btn-outline-danger remove-prop").append($("<span>").addClass("oi oi-trash"))),
    $("<input>").addClass("form-control prop-name").attr({
        type: "text",
        placeholder: "Property"
    }).prop("required", true)
]);

const baseParamDiv = $("<div>").addClass("card border border-secondary rounded m-1 p-1").width("179px").height("270px");
const productParamDiv = baseParamDiv.clone().addClass("param-div").append([
    $("<div>").addClass("card-header input-group p-1").append([
        $("<input>").addClass("form-control rounded border-0 param-name").attr({
            type: "text",
            placeholder: "Parameter"
        }).prop("required", true),
        $("<div>").addClass("input-group-append pl-2").append($("<button>").attr("type", "button").addClass("close").append($("<span>").html("&times;")))
    ]),
    $("<div>").addClass("card-body overflow-auto p-0").append(basePropDiv.clone().
        addClass("text-center d-flex flex-column justify-content-center prop-appender").append(baseAddBtn.clone().
            addClass("add-prop p-0").width("2rem").height("2rem")))
]);

const productModal = baseModal.clone(true).find(".modal-body > form").append($("<div>").
    addClass("row d-flex flex-wrap align-items-stretch border border-secondary rounded my-1 mx-auto p-2").append(baseParamDiv.clone().
        addClass("text-center d-flex flex-column justify-content-center param-appender").append(baseAddBtn.clone().addClass("add-param").css("font-size", "larger")))).
    append($("<div>").addClass("table-responsive").append($("<table>").addClass("table table-sm param-table"))).end();

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

    get modal() {
        return productModal;
    }

    openModal() {
        const self = this;
        const modal = super.openModal();
        modal.find(".param-div").remove();
        const jqAppender = modal.find(".param-appender");
        const jqParamBox = jqAppender.parent();
        const jqTable = modal.find(".param-table");
        jqTable.children().remove();
        const jqTheadTr = $("<tr>").appendTo($("<thead>").appendTo(jqTable));
        const jqTbody = $("<tbody>").appendTo(jqTable).append($("<tr>").append($("<td>").width("6rem").append($("<input>").addClass("form-control prop-price").attr({
            type: "number",
            step: "any",
            min: 0,
            placeholder: "0.00"
        }))));
        for (let i = this.properties.length - 1; i >= 0; i--) {
            const iParamDiv = productParamDiv.clone().find(".param-name").val(this.properties[i].name).end();
            const iParamPropAppender = iParamDiv.find(".prop-appender");
            jqTheadTr.prepend($("<th>").attr("scope", "col").text(this.properties[i].name));
            const jqPrevRows = jqTbody.children();
            iParamPropAppender.before(productPropDiv.clone().find(".prop-name").val(this.properties[i].types[0]).end());
            for (let j = 1; j < this.properties[i].types.length; j++) {
                iParamPropAppender.before(productPropDiv.clone().find(".prop-name").val(this.properties[i].types[j]).end());
                jqTbody.append(jqPrevRows.clone().prepend($("<td>").text(this.properties[i].types[j])));
            }
            jqPrevRows.prepend($("<td>").text(this.properties[i].types[0]));

            //jqAppender.before(iParamDiv);
            jqParamBox.prepend(iParamDiv);
        }
        jqTheadTr.prepend($("<th>").attr("scope", "col").text("#"));
        const jqAllTbodyRows = jqTbody.children();
        for (let i = 0; i < this.values.length; i++) {
            const iNumTh = document.createElement("th");
            iNumTh.setAttribute("scope", "row");
            iNumTh.textContent = i + 1;
            jqAllTbodyRows[i].prepend(iNumTh);
            jqAllTbodyRows[i].getElementsByTagName("input")[0].value = this.values[i];
        }
        jqTheadTr.append($("<th>").attr("scope", "col").text("Price"));
        modal.one("hide.bs.modal", function () {
            modal.find(".param-div").each(function (index) {
                const jqThisParamDiv = $(this);
                self.properties[index].name = jqThisParamDiv.find(".param-name").val();
                self.properties[index].types = jqThisParamDiv.find(".prop-name").map(function () {
                    return $(this).val();
                }).get();
            });
            self.values = modal.find(".prop-price").map(function () {
                return parseFloat($(this).val());
            }).get();
        });
        return modal;
    }

    updateCount() {
        /** Количество всех комбинаций подтипов. */
        this.count = this.properties.reduce(function (previous, current) {
            return previous * current.types.length;
        }, 1);
    }

    //TODO: написать методы для красивого добавления и удаления параметров.
}

const inputModal = baseModal.clone(true).find(".modal-body > form").append($("<fieldset>").
    addClass("form-group collection-set mb-0").append($("<div>").addClass("row").append([
        $("<legend>").addClass("col-form-label col-auto pt-0").text("Input type"),
        $("<div>").addClass("col-auto").append([
            $("<div>").addClass("form-check").append([
                $("<input>").attr({
                    type: "radio",
                    name: "input-type",
                    value: inputTypes.text,
                    class: "form-check-input"
                }),
                $("<label>").addClass("form-check-label").text("Text")
            ])
        ])
    ]))).end();

const cloneableInputModalInputDiv = inputModal.find("legend:contains(Input type) ~ div.col-auto > div.form-check");

CloneInputDiv(cloneableInputModalInputDiv, inputTypes.time, "Time");
CloneInputDiv(cloneableInputModalInputDiv, inputTypes.image, "Image");
CloneInputDiv(cloneableInputModalInputDiv, inputTypes.audio, "Audio");
CloneInputDiv(cloneableInputModalInputDiv, inputTypes.video, "Video");
CloneInputDiv(cloneableInputModalInputDiv, inputTypes.document, "Document");

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

    get modal() {
        return inputModal;
    }

    openModal() {
        const self = this;
        const modal = super.openModal();
        modal.find("input[name=input-type]").prop("checked", false).filter("[value=" + this.inputType + "]").prop("checked", true);
        modal.one("hide.bs.modal", function () {
            self.inputType = modal.find("input[name=input-type]:checked").val();
        });

        return modal;
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