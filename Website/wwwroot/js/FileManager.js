"use strict";

const imgModal = $("<div>").attr({
    class: "modal fade",
    tabindex: -1,
    role: "dialog",
    "aria-hidden": true
}).append($("<div>").attr({
    class: "modal-dialog",
    role: "document",
    style: "max-width: 100%; max-height: 100%; overflow-y: initial !important"
}).append($("<div>").addClass("modal-content").css("max-height", "calc(100vh - 3.5rem)").append([
    $("<div>").addClass("modal-header").append([
        $("<h5>").addClass("modal-title").text("File name"),
        $("<button>").attr({
            type: "button",
            class: "close",
            "data-dismiss": "modal",
            "aria-label": "Close"
        }).append($("<span>").attr("aria-hidden", true).html("&times;"))
    ]),
    $("<div>").addClass("modal-body p-0").css("overflow", "auto").append($("<img>")),
    $("<div>").addClass("modal-footer").append($("<a>").attr({
        type: "button",
        class: "btn btn-primary",
        target: "_blank",
        href: "#"
    }).text("Download"))
])));

/**
 * Отображает полученный файл.
 * @param {string} botToken Токен бота, которому принадлежит файл.
 * @param {HTMLElement} elem Элемент, к которому необходимо добавить файл.
 * @param {string} previewId ID превью файла.
 * @param {string} fileId ID файла.
 * @param {boolean} needDownload Нужно ли отображать ссылку для скачивания.
 * @returns Возвращает promise.
 */
function SetFileHTML(botToken, elem, previewId, fileId, needDownload) {
    const spinnerElem = elem.getElementsByClassName("spinner-border")[0];
    const getURL = "https://api.telegram.org/bot" + botToken + "/getFile";
    let previewXHR;
    let fileXHR;
    if (previewId != undefined) {
        previewXHR = $.ajax({
            url: getURL,
            type: 'get',
            data: { file_id: previewId },
            success: function (data) {
                const file_path = data.result.file_path;
                const pathURL = "https://api.telegram.org/file/bot" + botToken + "/" + file_path;
                const previewElem = document.createElement("img");
                previewElem.src = pathURL;
                previewElem.className = "mw-100 mh-100";
                if (needDownload) {
                    elem.appendChild(document.createElement("br"));
                }
                elem.appendChild(previewElem);
            },
            error: function (data) {
                ShowError(data.responseJSON.description, spinnerElem);
            }
        });
    }

    fileXHR = $.ajax({
        url: getURL,
        type: 'get',
        data: { file_id: fileId },
        success: function (data) {
            const file_path = data.result.file_path;
            const parts = file_path.split('/');
            const fileName = parts.pop();
            const fileFolder = parts.pop();
            const pathURL = "https://api.telegram.org/file/bot" + botToken + "/" + file_path;
            const jqSpinner = $(spinnerElem);
            jqSpinner.removeClass("spinner-border spinner-border-sm text-secondary");
            const linkElem = document.createElement("a");
            linkElem.href = pathURL;
            linkElem.text = fileName;
            linkElem.target = "_blank";
            if (needDownload) {
                jqSpinner.addClass("oi oi-cloud-download").append(linkElem);
            }
            if (fileFolder == "voice" || fileFolder == "music") {
                elem.appendChild(document.createElement("br"));
                const audioElem = document.createElement("audio");
                audioElem.className = "mw-100 mh-100";
                audioElem.setAttribute("controls", "");
                const sourceElem = document.createElement("source");
                sourceElem.src = pathURL;
                audioElem.appendChild(sourceElem);
                //audioElem.appendChild(linkElem); // если не поддерживается
                elem.appendChild(audioElem);
            }
            else {
                if (fileFolder == "video_notes" || fileFolder == "videos" || fileFolder == "animations") {
                    elem.appendChild(document.createElement("br"));
                    const videoElem = document.createElement("video");
                    videoElem.className = "mw-100 mh-100";
                    videoElem.setAttribute("controls", "");
                    const sourceElem = document.createElement("source");
                    sourceElem.src = pathURL;
                    videoElem.appendChild(sourceElem);
                    //videoElem.appendChild(linkElem); // если не поддерживается
                    elem.appendChild(videoElem);
                    AddPostAction(function () {
                        const previewElem = elem.getElementsByTagName("img")[0];
                        videoElem.poster = previewElem.src;
                        previewElem.remove();
                    });
                }
                else {
                    if (fileFolder == "stickers" || fileFolder == "photos") {
                        AddPostAction(function () {
                            const previewElem = elem.getElementsByTagName("img")[0];
                            previewElem.style = "cursor: zoom-in";
                            previewElem.onclick = function () {
                                imgModal.find(".modal-title").text(fileName).end().
                                    find(".modal-body > img").one("load", function () {
                                        $(this).parent().parent().parent().css("width", this.width + 2 + "px");
                                    }).attr("src", pathURL).end().
                                    find(".modal-footer > .btn").attr("href", pathURL).end().modal("show")
                            };
                        });
                    }
                    else {
                        if (previewId == null) {
                            jqSpinner.addClass("oi oi-file display-1");
                        }
                    }
                }
            }
        },
        error: function (data) {
            ShowError(data.responseJSON.description, spinnerElem);
        }
    });

    const postActions = [];

    function AddPostAction(action) {
        if (action != null) {
            postActions.push(action);
        }
    }

    return $.when(previewXHR, fileXHR).then(function () {
        while (postActions.length > 0) {
            postActions.pop()();
        }
    },
    function (jqXHR, textStatus) {
        let errMsg = textStatus;
        if (jqXHR.responseJSON) {
            errMsg = jqXHR.responseJSON.description;
        }
        ShowError(errMsg, spinnerElem);
    });

    function ShowError(msg, elem) {
        jqElem = $(elem);
        jqElem.removeClass("spinner-border spinner-border-sm text-secondary");
        jqElem.addClass("oi oi-x text-danger");
        elem.innerText = msg;
    }
}

/**
 * Базовый метод для отправки файлов.
 * @param {string} fileType Тип файла Telegram.
 * @param {string} botToken Токен бота.
 * @param {number} userId ID пользователя, которому отправится файл.
 * @param {File} file Файл, который необходимо отправить.
 * @returns Возвращает XMLHttpRequest.
 */
function SendFile(fileType, botToken, userId, file) {
    const upperType = fileType.charAt(0).toUpperCase() + fileType.slice(1);
    const fd = new FormData();
    fd.append(fileType, file);
    fd.append('chat_id', userId);
    return $.ajax({
        url: 'https://api.telegram.org/bot' + botToken + '/send' + upperType,
        type: 'POST',
        method: 'POST',
        data: fd,
        contentType: false,
        processData: false
    });
}

/**
 * Базовый метод для отправки изображений.
 * @param {string} botToken Токен бота.
 * @param {number} userId ID пользователя, которому отправится изображение.
 * @param {File} file Изображение, которое необходимо отправить.
 * @returns Возвращает объект из abort и promise.
 */
function SendPhoto(botToken, userId, file) {
    const jqXHR = SendFile("photo", botToken, userId, file);
    return {
        abort: function () {
            jqXHR.abort();
        },
        promise: jqXHR.then(function (data) {
            return {
                jqXHR: jqXHR,
                previewId: data.result.photo[0].file_id,
                fileId: data.result.photo.pop().file_id
            };
        })
    };
}

/**
 * Базовый метод для отправки аудиофайлов.
 * @param {string} botToken Токен бота.
 * @param {number} userId ID пользователя, которому отправится аудиофайл.
 * @param {File} file Аудиофайл, который необходимо отправить.
 * @returns Возвращает объект из abort и promise.
 */
function SendAudio(botToken, userId, file) {
    const jqXHR = SendFile("audio", botToken, userId, file);
    return {
        abort: function () {
            jqXHR.abort();
        },
        promise: jqXHR.then(function (data) {
            const audio = data.result.audio;
            let previewId = null;
            if (audio.thumb) {
                previewId = audio.thumb.file_id;
            }
            return {
                jqXHR: jqXHR,
                previewId: previewId,
                fileId: audio.file_id
            };
        })
    };
}

/**
 * Базовый метод для отправки видеофайлов.
 * @param {string} botToken Токен бота.
 * @param {number} userId ID пользователя, которому отправится видеофайл.
 * @param {File} file Видеофайл, который необходимо отправить.
 * @returns Возвращает объект из abort и promise.
 */
function SendVideo(botToken, userId, file) {
    const jqXHR = SendFile("video", botToken, userId, file);
    return {
        abort: function () {
            jqXHR.abort();
        },
        promise: jqXHR.then(function (data) {
            const video = data.result.video;
            let previewId = null;
            if (video.thumb) {
                previewId = video.thumb.file_id;
            }
            return {
                jqXHR: jqXHR,
                previewId: previewId,
                fileId: video.file_id
            };
        })
    };
}

/**
 * Базовый метод для отправки документов.
 * @param {string} botToken Токен бота.
 * @param {number} userId ID пользователя, которому отправится документ.
 * @param {File} file Документ, который необходимо отправить.
 * @returns Возвращает объект из abort и promise.
 */
function SendDocument(botToken, userId, file) {
    const jqXHR = SendFile("document", botToken, userId, file);
    return {
        abort: function () {
            jqXHR.abort();
        },
        promise: jqXHR.then(function (data) {
            const document = data.result.document;
            let previewId = null;
            if (document.thumb) {
                previewId = document.thumb.file_id;
            }
            return {
                jqXHR: jqXHR,
                previewId: previewId,
                fileId: document.file_id
            };
        })
    };
}