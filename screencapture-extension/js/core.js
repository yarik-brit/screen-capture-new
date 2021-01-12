class ExtensionMessage{
    constructor(context, data){
        this.context = context;
        this.data = data;
    }
};


//To Save Data From storage
function storageSet({key, value}){
    return new Promise((resolve) => {
        chrome.storage.sync.set({[key]: value}, function() {
            resolve();
          });
    });
}
function storageSetMultiple(...objects){
    let obj = {};

    objects.map(property => obj[property.key] = property.value);
    
    return new Promise((resolve) => {

        chrome.storage.sync.set(
            obj, 
            () =>{resolve();});
    });
}

//To Read Data From Storage
function storageGet(key){

    return new Promise((resolve) => {
        chrome.storage.sync.get([key], function(result) {
            resolve(result[key]);
            });
    });
}

function loading(active){
    let overlay = document.querySelector("div#loadingOverlay");
    if(overlay){
        overlay.style.display = active ? "flex" : "none";
    }
}


function sendPageMessage(data, specificID = null){
    return new Promise((resolve) =>{
        chrome.tabs.query({'active': true, 'lastFocusedWindow': true}, function (tabs) {
            if((tabs && tabs.length) || specificID){
                tabID =specificID || tabs[0].id;
                chrome.tabs.sendMessage(tabID, data);
          
                resolve();
            }
        });
    });
}

/**
 * Возвращает длину окружности, вычисленную заранее.
 *
 * @param  selector {string} Селектор элемента
 * @param  parent {HTMLElement} Родитель default null
 * @param  all {Boolean} Выбрать все default false
 * @return {HTMLElement} Длина окружности.
 */

function elementAppear(selector, parent = null, all = false){
    var cycles = 0;
    

    return new Promise((resolve) => {
        let id = setInterval(function(){
            cycles++;
            let element;

            if(all == false){
                if(parent != null){
                    element = parent.querySelector(selector);
                }else{
                    element = document.querySelector(selector);
                }
            }else{
                if(parent != null){
                    element = parent.querySelectorAll(selector);
                }else{
                    element = document.querySelectorAll(selector);
                }
            }

            if(all && (element.length > 0 || cycles>100)){
                clearInterval(id);
                resolve(element);
            }
            
            if((element || cycles>100) && all == false){
                clearInterval(id);
                resolve(element);
            }
        }, 100);
    });
}

function parentAppear(selector, parent = null){
    var cycles = 0;

    return new Promise((resolve) => {
        let id = setInterval(function(){
            cycles++;
            let element;
            if(parent != null){
                element = parent.closest(selector);
            }else{
                return;
            }
           

            if(element || cycles>100){
                clearInterval(id);
                resolve(element);
            }
        }, 100);
    });
}

function forSeconds(seconds){
    return new Promise((resolve) =>{setTimeout(function(){resolve()}, seconds * 1000)});
}



function activateContextMenu(menuId, activate){
    chrome.contextMenus.update(menuId, {
        visible: activate ? true : false,
  });
}

function requestBackground(request){
    return new Promise((resolve) =>{
        var handler = ({context, data}) => {
          if(context == request.context){
            chrome.runtime.onMessage.removeListener(handler);
            resolve(data);
          }
        };
        chrome.runtime.onMessage.addListener(handler);
          chrome.runtime.sendMessage(new ExtensionMessage(request.context, request.data));
      });
}



function floor(value, decimals = 0){
    return Math.floor(value * Math.pow(10, decimals)) / Math.pow(10, decimals);
}

function formDataToObj(form) {
    const formData = new FormData(form);
    const data = {};
    for(const [key, value] of formData.entries()){
        data[key] = value;
    }

    return data;
}

function formDataConvertObj(formData) {
    const data = {};
    for(const [key, value] of formData.entries()){
        data[key] = value;
    }

    return data;
}

function getCookie(name) {
    let matches = document.cookie.match(new RegExp(
      "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
    ));
    return matches ? decodeURIComponent(matches[1]) : undefined;
}

function setCookie(name, value, options = {}) {

    options = {
      path: '/',
      // при необходимости добавьте другие значения по умолчанию
      ...options
    };
  
    if (options.expires instanceof Date) {
      options.expires = options.expires.toUTCString();
    }
  
    let updatedCookie = encodeURIComponent(name) + "=" + encodeURIComponent(value);
  
    for (let optionKey in options) {
      updatedCookie += "; " + optionKey;
      let optionValue = options[optionKey];
      if (optionValue !== true) {
        updatedCookie += "=" + optionValue;
      }
    }
  
    document.cookie = updatedCookie;
}

function deleteCookie(name) {
    setCookie(name, "", {
      'max-age': -1
    })
}

function parseJwt (token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
}

function contactLoader(scrollArea, activate = false, body){
    return new Promise(async (resolve) => {
        let loader = body.querySelector("div#contact-loader");
        let containerParent = await elementAppear(config.selectors.dialogsContainer, body);
        
        if(!loader){
            loader = document.createElement("div");
            loader.id = "contact-loader";

            loader.innerHTML = `
            <div class="contact-loader-header">
                <p>Render contacts. Please wait</p>
                <img src="${chrome.runtime.getURL("images/gear.gif")}">
            </div>`;
            
            if(containerParent){
                let container = await parentAppear("._1enh._7q1s", containerParent);
    
                if(container){
                    container.style.position = "relative";
                    container.appendChild(loader);
                }
            }
        }

        
        if(scrollArea){
            scrollArea.style.overflowY = activate ? "hidden" : "scroll";

            let containerParent = await elementAppear(config.selectors.dialogsContainer, body);
            let scrollBlock = await parentAppear(".uiScrollableArea", containerParent);
            let scrollBar = await elementAppear(".uiScrollableAreaTrack", scrollBlock);
           
            if(activate){
                scrollBar.classList.add('hide');
                loader.style.display = "block";
            }else{
                scrollBar.classList.remove('hide');
                loader.style.display = "none";
            }
        }

        resolve();
    })
}

function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function parseMessageTemplate(message) {
    return new Promise(async (resolve) => {
        const matches = [...message.matchAll(/(?<=\[\[)([\d]+?)(?=\]\])/gm)];
       
        if(matches.length == 0){
            resolve(message);
            return;
        }

        const ids = matches.map((match) => match[0].replace('[[', '').replace(']]', ''));
        
        if(ids.length == 0){
            resolve(message);
            return;
        }

        let newMessage = message;

        for(const id of ids){
            const result = await requestBackground(new ExtensionMessage(config.keys.getMessageSegment, {id}));
            
            if(result.segments.length > 0){
                const segmentList = result.segments[0].Segment.split('&&&');
                
                const index = getRandomInt(0, segmentList.length - 1);
                newMessage = newMessage.replace(`[[${id}]]`, segmentList[index]); 
            }
        }

        resolve(newMessage);
    })
}

function timestampToDatetimeInputString(timestamp) {
    const timeZoneOffsetInMs = new Date().getTimezoneOffset() * -60 * 1000;
    const date = new Date(timestamp + timeZoneOffsetInMs);
    return date.toISOString().slice(0, 16);
}

function insertAtCaret(txtarea, text) {
    if (!txtarea) { return; }

    var scrollPos = txtarea.scrollTop;
    var strPos = 0;
    var br = ((txtarea.selectionStart || txtarea.selectionStart == '0') ?
        "ff" : (document.selection ? "ie" : false ) );
    if (br == "ie") {
        txtarea.focus();
        var range = document.selection.createRange();
        range.moveStart ('character', -txtarea.value.length);
        strPos = range.text.length;
    } else if (br == "ff") {
        strPos = txtarea.selectionStart;
    }

    var front = (txtarea.value).substring(0, strPos);
    var back = (txtarea.value).substring(strPos, txtarea.value.length);
    txtarea.value = front + text + back;
    strPos = strPos + text.length;
    if (br == "ie") {
        txtarea.focus();
        var ieRange = document.selection.createRange();
        ieRange.moveStart ('character', -txtarea.value.length);
        ieRange.moveStart ('character', strPos);
        ieRange.moveEnd ('character', 0);
        ieRange.select();
    } else if (br == "ff") {
        txtarea.selectionStart = strPos;
        txtarea.selectionEnd = strPos;
        txtarea.focus();
    }

    txtarea.scrollTop = scrollPos;
}


const toBase64 = file => new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => resolve(reader.result);
    reader.onerror = error => reject(error);
});

function copyImageToClipboard(url) {
    return new Promise((resolve) => {
        if(url == ""){
            resolve();
            return;
        }
        
        const img = new Image;
        img.crossOrigin = 'Anonymous';
        img.src = url;
    
        img.onload = () => {
            const canvas = document.createElement('canvas');
    
            canvas.width = img.width;
            canvas.height = img.height;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(img, 0, 0);
            
            canvas.toBlob(function (blob) {
                let data = [new ClipboardItem({ [blob.type]: blob })];
                navigator.clipboard.write(data);
            });

            resolve();
        }
    })
  }

  function storageLocalGet(key) {
    return new Promise((resolve) => {
        chrome.storage.local.get([key], function(result) {
            resolve(result[key]);
            });
    });
  }

  function storageLocalSet({key, value}) {
    return new Promise((resolve) => {
        chrome.storage.local.set({[key]: value}, function() {
            resolve();
          });
    });
  }

  async function sendSocketNotify(socket, data) {
      if(!(socket instanceof WebSocket)){
          return;
      }
      
      await waitConnect(socket);
      socket.send(data);
  }

  function waitConnect(socket) {
      return new Promise(async (resolve) => {
        if(!(socket instanceof WebSocket)){
            resolve();
            return;
        }

        let connect = 0;
        while(connect == 0){
            if(socket.readyState == 1){
                connect = 1;
            }
            await forSeconds(0.5);
        }

        resolve();

      });
  }