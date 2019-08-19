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
 */
function SetFileHTML(botToken, elem, previewId, fileId, needDownload) {
    let isVideo = false;
    let previewWasGotten = false;
    let fileWasGotten = false;
    const spinnerElem = elem.lastChild;
    const getURL = "https://api.telegram.org/bot" + botToken + "/getFile";
    if (previewId != undefined) {
        $.ajax({
            url: getURL,
            type: 'get',
            data: { file_id: previewId },
            success: function (data) {
                const file_path = data.result.file_path;
                const pathURL = "https://api.telegram.org/file/bot" + botToken + "/" + file_path;
                if (isVideo) {
                    const videoElem = elem.getElementsByTagName('video')[0];
                    videoElem.poster = pathURL;
                }
                else {
                    const previewElem = document.createElement("img");
                    previewElem.src = pathURL;
                    elem.appendChild(document.createElement("br"));
                    elem.appendChild(previewElem);
                }
                previewWasGotten = true;
                PostAction();
            },
            error: function (data) {
                ShowError(data.responseJSON.description, spinnerElem);
            }
        });
    }

    $.ajax({
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
            if (needDownload) {
                jqSpinner.addClass("oi oi-cloud-download");
            }
            const linkElem = document.createElement("a");
            linkElem.href = pathURL;
            linkElem.text = fileName;
            linkElem.target = "_blank";
            if (fileFolder == "voice" || fileFolder == "music") {
                elem.appendChild(document.createElement("br"));
                const audioElem = document.createElement("audio");
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
                    videoElem.setAttribute("controls", "");
                    const sourceElem = document.createElement("source");
                    sourceElem.src = pathURL;
                    videoElem.appendChild(sourceElem);
                    //videoElem.appendChild(linkElem); // если не поддерживается
                    elem.appendChild(videoElem);
                    isVideo = true;
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
                }
            }
            if (needDownload) {
                spinnerElem.appendChild(linkElem);
            }
            fileWasGotten = true;
            PostAction();
        },
        error: function (data) {
            ShowError(data.responseJSON.description, spinnerElem);
        }
    });

    let postActions = [function () {
        console.log("End loading");
    }];

    function AddPostAction(action) {
        if (action != null) {
            postActions.push(action);
        }
    }

    function PostAction() {
        if (previewWasGotten && fileWasGotten) {
            while (postActions.length > 0) {
                postActions.pop()();
            }
        }
    }

    function ShowError(msg, elem) {
        jqElem = $(elem);
        jqElem.removeClass("spinner-border spinner-border-sm text-secondary");
        jqElem.addClass("oi oi-x text-danger");
        elem.innerText = msg;
    }
}