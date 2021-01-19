window.onload = init;
//var baseUrl = "http://tensionx-002-site8.btempurl.com/";
var baseUrl = "https://localhost:44336/";

async function init() {
    if (document.location.pathname == "/Video" && document.location.search.split(/[?=]+/)[1] == "id") {
        if (document.querySelector('.name > h2') != null) {
            var textArea = document.createElement('textarea');
            textArea.hidden = true;
            textArea.value = document.location.href;
            document.body.appendChild(textArea);
            document.querySelector('.btn-copy').onclick = () => {
                textArea.select();
                document.execCommand('copy');
                //navigator.clipboard.writeText(document.location.href);
            };
            document.querySelector('.btn-delete').onclick = async function () {
                deleteImg().then(result => location.replace(baseUrl + "/Video?id=" + result))
                    .catch(error => location.replace(baseUrl + "/Video?id=" + result))
            };

            var trigger = setInterval(function () {
                var xhr = new XMLHttpRequest();
                xhr.onload = function () {
                    if (xhr.readyState === xhr.DONE) {
                        if (xhr.status === 200) {
                            if (xhr.response != "false") {
                                document.querySelector("#loading").setAttribute("hidden", "true");
                                document.querySelector("#uploadingText").setAttribute("hidden", "true");
                                
                                document.querySelector("video").removeAttribute("hidden");
                                document.querySelector("video").setAttribute("src", xhr.response);
                                clearInterval(trigger);
                            }
                            console.log(xhr.response);
                        }
                    }
                };
                xhr.open("POST", `${baseUrl}Video/IsVideoUploaded`, true);
                xhr.send(document.location.search.split(/[?=]+/)[2]);
            }, 5000);
        }
    }
}


function deleteImg() {
    var formData = new FormData();
    formData.append("vidName", document.querySelector(".name > h2").textContent);

    return new Promise((resolve, reject) =>
        $.ajax({
            type: "POST",
            url: `${baseUrl}Video/DeleteVideoByName`,
            cache: false,
            processData: false,
            contentType: false,
            //dataType: "json",
            data: formData,
            success: (result) => {
                resolve(result);
            },
            error: (result) => {
                console.log(result);
                reject(result);
            },
        })
    )
}