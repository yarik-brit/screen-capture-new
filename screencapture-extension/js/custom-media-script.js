async function f(recordedBlobs){
    var superBuffer = new Blob(recordedBlobs, {
        type: 'video/webm'
    });

    var xhr = new XMLHttpRequest();
    xhr.open("POST", `${config.values.base_url}Data/PrepareDirectory`, true);
    xhr.onload = async function(){
      console.log(xhr.response);
      for (let index = 0; index < recordedBlobs.length; index++) {
        const element = recordedBlobs[index];
        console.log(element.size);
        var formData = new FormData();
        formData.append('filepart', element, index.toString());
        await request(formData);
        await forSeconds(0.2);
      }
      uploadComplete();
    }
    xhr.send(null);
}


var uploadComplete = function () { 
    var xhr2 = new XMLHttpRequest();
    xhr2.onreadystatechange = function() {
      if (xhr2.readyState === xhr2.DONE) {
        if (xhr2.status === 200) {
          console.log(xhr2.response);
        }
      }
    }
    xhr2.open("POST", `${config.values.base_url}Data/UploadComplete`, true); //combine the chunks together
    xhr2.send(null);
  };
  
  
var request = function(formData){
    var xhr = new XMLHttpRequest();
    xhr.onload = function () {
      if (xhr.readyState === xhr.DONE) {
        if (xhr.status === 200) {
          if(xhr.response.split('/').includes("https:")){
            var win = window.open(xhr.response, '_blank');
            win.focus();
          }
          console.log(xhr.response);
        }
      }
    };
    xhr.open("POST", `${config.values.base_url}Data/MultiUpload`, true);
    xhr.send(formData);
  }


chrome.runtime.onMessage.addListener(
  async function(message, sender){
    switch(message.context){
        case config.keys.requestSaveImg:
        saveImg(message, sender);
        break;
      }
    }
);


async function saveImg(message, sender){
  var resultText = "";
  console.log(message.data);
  
  var formData = new FormData();
  formData.append('fileurl', message.data);

  var xhr = new XMLHttpRequest();
  xhr.open('POST', `${config.values.base_url}Data/GetImageUrl`, true);
  xhr.onload = function () {
      console.log(this.responseText);
      resultText = this.responseText;
      chrome.tabs.query({active: true}, function(tabs) {
        chrome.tabs.sendMessage(tabs[0].id, new ExtensionMessage(message.context, resultText));
      });
  };
  xhr.send(formData);
};