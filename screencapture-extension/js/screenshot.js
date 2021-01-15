console.log("Hello, extension!");


chrome.runtime.onMessage.addListener(
  function(message, sender){
    switch(message.context){
      case "pageScreen":
        dataUrl = message.data;
        loadCropper();
        break;
      case config.keys.requestSaveImg:
        screenshotSaved(message.data);
        break;
    }
  }
  );
  
  
  var dataUrl;
  var div, canvas, btnFinalize, btnSave, btnColor, btnExit, btnCopy;
  var img;
  var cropParams = {
    x: 0,
    y: 0,
    width: 0,
    height: 0
  };
  var x, y, xX, yY, width, height;
  var strokeColor = "#FF0000";
  
  document.addEventListener('keyup', (event) => {
    if (event.code == "Escape") {
      screenshotSaved();
      totalExit();
    }
  }, false);
  
  
  
  
  function loadCropper(){
    div = document.createElement('div');
    div.setAttribute('id', "extDiv");
    div.width = window.innerWidth;
    div.height = window.innerHeight;
    
    canvas = document.createElement('canvas');
    canvas.setAttribute('id', "extCanvas");
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
    
    img = new Image();
    
    
    div.appendChild(canvas);
    document.body.appendChild(div);
    
    document.documentElement.style.overflowY = "hidden";
    
    img.onload = drawImage;
    
    img.src = dataUrl;
    
    selectCropArea();
  };
  
  function drawImage(){
    var ctx = canvas.getContext('2d');
    var canvasStyle = getComputedStyle(canvas);
    var canvasWidth = canvasStyle.width.replace("px", "");
    var imageRatio = img.width / img.height;
    var canvasHeight = canvasWidth / imageRatio;
    
    ctx.drawImage(img, 0, 0, canvasWidth, canvasHeight);
  };
  
  
  
  function selectCropArea(){
    canvas.onmousedown = function(event){_listenerCanvas(event)};
    canvas.onmouseup = function(event){_listenerCanvas(event)};
  };
  
  
  
  function drawOnCanvas(){
    // get canvas 2D context and set him correct size
    var ctx = canvas.getContext('2d');
    console.log(ctx);
    
    // last known position
    var pos = { x: 0, y: 0 };
    
    window.addEventListener('resize', resize);
    document.addEventListener('mousemove', draw);
    document.addEventListener('mousedown', setPosition);
    // document.addEventListener('mouseenter', setPosition);
    
    // new position from mouse event
    function setPosition(e) {
      pos.x = e.clientX;
      pos.y = e.clientY;
    }
    
    function resize() {
      ctx.canvas.width = window.innerWidth;
      ctx.canvas.height = window.innerHeight;
    }
    
    function draw(e) {
      // mouse left button must be pressed
      if (e.buttons !== 1) return;
      
      ctx.beginPath(); // begin
      
      ctx.lineWidth = 5;
      ctx.lineCap = 'round';
      ctx.strokeStyle = strokeColor;
      
      var canvasX = ctx.canvas.offsetLeft;
      var canvasY = ctx.canvas.offsetTop;
      var canvasWidth = ctx.canvas.offsetWidth;
      var canvasHeight = ctx.canvas.offsetHeight;
      
      
      
      if((pos.x >= canvasX && pos.x <= (canvasX + canvasWidth)) && (pos.y >= canvasY && pos.y <= (canvasY + canvasHeight)) && e.target.tagName == "CANVAS"){
        ctx.moveTo(Math.abs(canvasX - pos.x), Math.abs(canvasY - pos.y)); // from
        setPosition(e);
        ctx.lineTo(Math.abs(canvasX - pos.x), Math.abs(canvasY - pos.y)); // to
        
        ctx.stroke(); // draw it!
      }
    }
  };
  
  
  function _listenerCanvas(e){
    
    
    var ctx = canvas.getContext('2d');
    ctx.filter = "grayscale(50%)";
    
    if(e.type == "mousedown"){
      var canvasStyle = getComputedStyle(canvas);
      var canvasWidth = canvasStyle.width.replace("px", "");
      var canvasHeight = canvasStyle.height.replace("px", "");
      ctx.beginPath();
      ctx.clearRect(0, 0, canvasWidth, canvasHeight);
      
      drawImage();
      x = e.clientX - ctx.canvas.offsetLeft;
      y = e.clientY - ctx.canvas.offsetTop;
    }
    
    if(e.type == "mouseup"){
      xX = e.clientX - ctx.canvas.offsetLeft;
      yY = e.clientY - ctx.canvas.offsetTop;
      width = xX - x;
      height = yY - y;
      
      var ctx = canvas.getContext("2d");
      ctx.fillStyle = "#f7b5d699";
      ctx.rect(x, y, width, height);
      ctx.fill();
      
      
      
      if((xX - x) > 10 || (yY - y) > 10){
        cropParams.x = (xX > x) ? x : xX;
        cropParams.y = (yY > y) ? y : yY;
        cropParams.width = Math.abs(xX - x);
        cropParams.height = Math.abs(yY - y);
        
        console.log("area selected!");
        console.log(x + ":" + y + " to " + xX + ":" + yY);
        console.log("width::height --- " + Math.abs(xX - x) + "::" + Math.abs(yY - y));
      }
      
      _listenerStart();
    }
  };
  
  function _listenerStart(){
    canvas.onmousedown = function(){};
    canvas.onmouseup = function(){};
    cropper.start(document.getElementById("extCanvas"), 1);
    cropper.showImage(dataUrl);
    cropper.startCropping(cropParams);
    
    btnFinalize = document.createElement('button');
    btnFinalize.setAttribute('class', "extCanvasButton");
    btnFinalize.setAttribute('id', "extCBFinalize");
    btnFinalize.textContent = "Finalize selection";
    btnFinalize.style.top = window.getComputedStyle(canvas).top + "px";
    btnFinalize.onclick = function(){_listenerBtnFinalize()};
    div.appendChild(btnFinalize);
  };
  
  function _listenerBtnFinalize(){
    var croppedResult = cropper.getCroppedImageSrc();
    var w = croppedResult.width;
    var h = croppedResult.height;
    console.log(w + "xxx" + h);
    canvas.setAttribute('width', w);
    canvas.setAttribute('height', h);
    img.src = croppedResult.url;
    drawImage();
    drawOnCanvas();
    
    btnFinalize.parentElement.removeChild(btnFinalize);
    
    
    btnSave = document.createElement('button');
    btnSave.setAttribute('class', "extCanvasButton");
    btnSave.setAttribute('id', "extCBSave");
    btnSave.textContent = "Save crop";
    btnSave.style.top = canvas.offsetTop + canvas.height + "px";
    btnSave.style.left = canvas.offsetLeft + canvas.width / 1.75 + "px";
    btnSave.onclick = async function(){
      _listenerBtnSave()
    };
    div.appendChild(btnSave);
    
    btnColor = document.createElement('input');
    btnColor.type = "color";
    btnColor.setAttribute('id', "extCBColor");
    btnColor.setAttribute('class', "extCanvasButton");
    btnColor.setAttribute('value', "#FF0000");
    btnColor.style.top = canvas.offsetTop + canvas.height + "px";
    btnColor.style.left = btnSave.offsetLeft + 150 + "px";
    btnColor.onchange = function(){strokeColor = this.value;}
    div.appendChild(btnColor);
    
    
    btnExit = document.createElement('button');
    btnExit.setAttribute('class', "extCanvasButton");
    btnExit.setAttribute('id', "extCBExit");
    btnExit.textContent = "Exit";
    btnExit.style.top = canvas.offsetTop + canvas.height + "px";
    btnExit.style.left = canvas.offsetLeft + "px";
    btnExit.onclick = function(){totalExit()};
    
  };
  
  async function _listenerBtnSave(){
    var finalImgUrl = canvas.toDataURL("png");
    
    canvas.parentElement.removeChild(canvas);
    
    var buttons = document.querySelectorAll(".extCanvasButton");
    buttons.forEach(b => {
      if(b.id != "extCBExit"){
        b.parentElement.removeChild(b);
      }
      else{
        b.classList.add("hiddenElement");
      }
    });
    loading(true);
    chrome.runtime.sendMessage(new ExtensionMessage(config.keys.requestSaveImg, finalImgUrl));
  };
  
  
  async function screenshotSaved(msg){
    var finalDiv = document.createElement("div");
    finalDiv.setAttribute('id', "extFinalDiv");
    
    var finalLabel = document.createElement("label");
    finalLabel.setAttribute('id', "extFinalLabel");
    
    var finalInput = document.createElement("input");
    finalInput.setAttribute('id', "extFinalInput");
    finalInput.type = "url";
    
    await forSeconds(4);
    loading(false);
    
    
    if(msg == ""){
      finalLabel.textContent = "Oops... Something happened... Error occured!";
      finalLabel.style.fontSize = "30px";
      div.appendChild(btnExit);
    }
    else{
      finalLabel.textContent = "Here is link to your image!";
      finalInput.value = msg;
      finalDiv.appendChild(finalInput);
      
      
      btnCopy = document.createElement('button');
      btnCopy.setAttribute('class', "extCanvasButton");
      btnCopy.setAttribute('id', "extCBCopy");
      btnCopy.textContent = "Copy";
      btnCopy.onclick = function(){
        navigator.clipboard.writeText(msg);
      };
      
      btnCopy.style.top = 350 + "px";
      btnCopy.style.left = 800 + "px";
      finalDiv.appendChild(btnCopy);
      finalDiv.appendChild(btnExit);
    }
     
    
    btnExit.style.top = 350 + "px";
    btnExit.style.left = 600 + "px";
    
    btnExit.classList.remove("hiddenElement");
    finalDiv.appendChild(finalLabel);
    div.appendChild(finalDiv);
    finalDiv.style.top = window.innerHeight / 2  - finalDiv.offsetHeight / 2 - 20 + "px";
  }
  
  function totalExit(){
    div.parentElement.removeChild(div);
    document.documentElement.style.overflowY = "scroll";
  }
  
  
  function loading(is){
    if(is){
      var img = document.createElement("img");
      img.setAttribute('id', "extLoadingImg");
      img.src = chrome.runtime.getURL("../assets/images/loading.gif");
      
      div.appendChild(img);
      img.style.top = (div.offsetHeight - img.offsetHeight) / 2 + "px";
      img.style.left = (div.offsetWidth - img.offsetWidth) / 2 + "px";
    }
    else{
      var img = document.getElementById("extLoadingImg");
      img.parentElement.removeChild(img);
    }
  }