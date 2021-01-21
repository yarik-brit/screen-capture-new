window.onload = init;

var baseUrl = "https://testscreenshot.gear.host";
//var baseUrl = "https://localhost:44336";

async function init() {
    if (document.location.pathname == "/Screenshot" && document.location.search.split(/[?=]+/)[1] == "id") {
        if (document.querySelector('.name > h2') != null) {
            document.querySelector('.btn-copy').onclick = () => {
                navigator.clipboard.writeText(baseUrl + document.location.pathname + document.location.search);
            };
            document.querySelector('.btn-delete').onclick = async function () {
                deleteImg().then(result => location.replace(baseUrl + "/Screenshot?id=" + result))
                    .catch(error => location.replace(baseUrl + "/Screenshot?id=" + result))
            };
        }
    }
}


function deleteImg() {
    var formData = new FormData();
    formData.append("imgName", document.querySelector(".name > h2").textContent);

    return new Promise((resolve, reject) =>
        $.ajax({
            type: "POST",
            url: `${baseUrl}/Screenshot/DeleteImageByName`,
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