async function f(recordedBlobs){
    var superBuffer = new Blob(recordedBlobs, {
        type: 'video/webm'
    });

    var xhr = new XMLHttpRequest();
    xhr.open("POST", `${config.base_url}Data/PrepareDirectory`, true);
    xhr.onload = async function(){
      console.log(xhr.response);
      for (let index = 0; index < recordedBlobs.length; index++) {
        await forSeconds(1);
        const element = recordedBlobs[index];
        console.log(element.size);
        await request(element);
        await forSeconds(1);
      }
      uploadComplete();
    }
    xhr.send(null);
}


var uploadComplete = function () {
    // var formData = new FormData();
    // formData.append('fileName', "video.mp4");
    // formData.append('completed', true);
    
    var xhr2 = new XMLHttpRequest();
    xhr2.onreadystatechange = function() {
      if (xhr2.readyState === xhr2.DONE) {
        if (xhr2.status === 200) {
          console.log(xhr2.response);
          var win = window.open(xhr2.response, '_blank');
          win.focus();
        }
      }
    }
    xhr2.open("POST", `${config.base_url}Data/UploadComplete`, true); //combine the chunks together
    xhr2.send(null);
    
    
  };
  
  
  
  var request = function(chunk){
    var xhr = new XMLHttpRequest();
    xhr.onload = function () {
      if (xhr.readyState === xhr.DONE) {
        if (xhr.status === 200) {
          console.log(xhr.response);
        }
      }
    };
    xhr.open("POST", `${config.base_url}Data/MultiUpload`, true);
    xhr.send(chunk);
  }