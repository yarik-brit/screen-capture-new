(function(cropper, undefined) {
	"use strict"; // helps us catch otherwise tricky bugs

	/* DRAWING STUFF */
	var canvas;
	var context;

	var image;
	var restoreImage;
	var currentDimens = {};
	var cropping = false;

	var colors = {
		white: "#ffffff",
		black: "#000000",
		overlay: "rgba(0, 0, 0, 0.6)"
	};

	var overlay;

	function draw() {
		// clear the canvas
		context.clearRect(0, 0, canvas.width, canvas.height);

		// if we don't have an image file, abort the draw at this point
		if(image === undefined) {
			return;
		}

		// draw the image
		var dimens = currentDimens;
		// context.drawImage(image, (canvas.width - image.width)/2, (canvas.height - image.height)/2, image.width, image.height);
		context.drawImage(img, 0, 0, canvas.width, canvas.height);

		// draw cropping stuff if we are cropping
		if(cropping) {
			// draw the overlay
			drawOverlay();

			// draw the resizer
			var x = overlay.x + overlay.width - 5,
				y = overlay.y + overlay.height - 5,
				w = overlay.resizerSide,
				h = overlay.resizerSide;

			context.save();
			context.fillStyle = colors.black;
			context.strokeStyle = colors.white;
			context.fillRect(x, y, w, h);
			context.strokeRect(x, y, w, h);
			context.restore();
		}
	}

	function initializeOverlay(overlayParams){
		overlay.x = overlayParams.x;
		overlay.y = overlayParams.y;
		overlay.width = overlayParams.width > 50 ? overlayParams.width : 50;
		overlay.height = overlayParams.height > 50 ? overlayParams.height : 50;
	}

	function drawOverlay() {
		// draw the overlay using a path made of 4 trapeziums (ahem)
		context.save();

		context.fillStyle = colors.overlay;
		context.beginPath();

		context.moveTo(0, 0);
		context.lineTo(overlay.x, overlay.y);
		context.lineTo(overlay.x + overlay.width, overlay.y);
		context.lineTo(canvas.width, 0);

		context.moveTo(canvas.width, 0);
		context.lineTo(overlay.x + overlay.width, overlay.y);
		context.lineTo(overlay.x + overlay.width, overlay.y + overlay.height);
		context.lineTo(canvas.width, canvas.height);

		context.moveTo(canvas.width, canvas.height);
		context.lineTo(overlay.x + overlay.width, overlay.y + overlay.height);
		context.lineTo(overlay.x, overlay.y + overlay.height);
		context.lineTo(0, canvas.height);

		context.moveTo(0, canvas.height);
		context.lineTo(overlay.x, overlay.y + overlay.height);
		context.lineTo(overlay.x, overlay.y);
		context.lineTo(0, 0);

		context.fill();

		context.restore();
	}

	function setRatio(ratio) {
		overlay.ratioXY = ratio;
		overlay.height = Math.floor(overlay.width * ratio);
	}

	function getScaledImageDimensions(width, height) {
		// choose the dimension to scale to, depending on which is "more too big"
		var factor = 1;
		if((canvas.width - width) < (canvas.height - height)) {
			// scale to width
			factor = canvas.width / width;
		} else {
			// scale to height
			factor = canvas.height / height;
		}
		// important "if,else" not "if,if" otherwise 1:1 images don't scale

		var dimens = {
			width: Math.floor(width * factor),
			height: Math.floor(height * factor),
			factor: factor
		};

		return dimens;
	}

	function getTouchPos(touchEvent) {
		var rect = canvas.getBoundingClientRect();

		return {
			x: touchEvent.touches[0].clientX - rect.left,
			y: touchEvent.touches[0].clientY - rect.top
		};
	}
	/**
	 * @param {Number} x position mouse / touch client event
	 * @param {Number} y position mouse / touch client event
	 */
	function getClickPos({x, y}) {
		return {
			x : x - window.scrollX,
			y : y - window.scrollY
		}	
	}

	function isInOverlay(x, y) {
		return x > overlay.x && x < (overlay.x + overlay.width) && y > overlay.y && y < (overlay.y + overlay.height);
	}

	function isInHandle(x, y) {
		return x > (overlay.x + overlay.width - overlay.resizerSide) && x < (overlay.x + overlay.width + overlay.resizerSide) && y > (overlay.y + overlay.height - overlay.resizerSide) && y < (overlay.y + overlay.height + overlay.resizerSide);
	}

	/* EVENT LISTENER STUFF */
	var drag = {
		type: "", // options: "moveOverlay", "resizeOverlay"
		inProgress: false,
		originalOverlayX: 0,
		originalOverlayY: 0,
		originalX: 0,
		originalY: 0,
		originalOverlayWidth: 0,
		originalOverlayHeight: 0
	};

	/**
	 * @param {Number} x position mouse / touch client event
	 * @param {Number} y position mouse / touch client event
	 */
	function initialCropOrMoveEvent({x, y}) {
		// if the mouse clicked in the overlay	
		if(isInOverlay(x, y)) {
			drag.type = "moveOverlay";
			drag.inProgress = true;
			drag.originalOverlayX = x - overlay.x;
			drag.originalOverlayY = y - overlay.y;
		}
		
		if(isInHandle(x, y)) {
			drag.type = "resizeOverlay";
			drag.inProgress = true;
			drag.originalX = x;
			drag.originalY = y;
			drag.originalOverlayWidth = overlay.width;
			drag.originalOverlayHeight = overlay.height;
		}
	}

	/**
	 * @param {Number} x horizontal position mouse or touch event
	 * @param {Number} y vertical position mour or touch event
	 * @description this function will be crop image inside canvas
	 */
	function startCropOrMoveEvent({x, y}) {

		// Set current cursor as appropriate
		if(isInHandle(x, y) || (drag.inProgress && drag.type === "resizeOverlay")) {
			canvas.style.cursor = 'nwse-resize'
		} 
		// else if(isInOverlay(x, y)) {
		// 	canvas.style.cursor = 'move'
		// } 
		else {
			canvas.style.cursor = 'auto'
		}

		// give up if there is no drag in progress
		if(!drag.inProgress) {
			return;
		}

		// check what type of drag to do
		if(drag.type === "moveOverlay") {
			overlay.x = x - drag.originalOverlayX;
			overlay.y = y - drag.originalOverlayY;

			// Limit to size of canvas.
			var xMax = canvas.width - overlay.width;
			var yMax = canvas.height - overlay.height;

			if(overlay.x < 0) {
				overlay.x = 0;
			} else if(overlay.x > xMax) {
				overlay.x = xMax;
			}

			if(overlay.y < 0) {
				overlay.y = 0;
			} else if(overlay.y > yMax) {
				overlay.y = yMax;
			}

			draw();
		} else if(drag.type === "resizeOverlay") {
			overlay.width = drag.originalOverlayWidth + (x - drag.originalX);
			overlay.height = drag.originalOverlayHeight + (y - drag.originalY);

			// do not allow the overlay to get too small
			if(overlay.width < 50) {
				overlay.width = 50;
			}

			if(overlay.height < 50) {
				overlay.height = 50;
			}

			// Don't allow crop to overflow
			if(overlay.x + overlay.width > canvas.width) {
				overlay.width = canvas.width - overlay.x;
			}

			if(overlay.y + overlay.height > canvas.height) {
				overlay.height = canvas.height - overlay.y;
			}


			draw();
		}
	}	

	function addEventListeners() {
		// add mouse listeners to the canvas
		canvas.onmousedown = function(event) {
			// depending on where the mouse has clicked, choose which type of event to fire
			var coords = canvas.getMouseCoords(event);
			initialCropOrMoveEvent(getClickPos(coords));
		};

		canvas.onmouseup = function(event) {
			// cancel any drags
			drag.inProgress = false;
		};

		canvas.onmouseout = function(event) {
			// cancel any drags
			drag.inProgress = false;
		};

		canvas.onmousemove = function(event) {
			var coords = canvas.getMouseCoords(event);

			startCropOrMoveEvent(getClickPos(coords));
		};

		canvas.addEventListener('touchstart', event => {
			initialCropOrMoveEvent(getTouchPos(event));
		});

		canvas.addEventListener('touchmove', event => {
			startCropOrMoveEvent(getTouchPos(event));
		});

		canvas.addEventListener('touchend', event => {
			drag.inProgress = false;
		})
	}

	function removeEventListeners() {
		canvas.onmousedown = function(event) {
		};

		canvas.onmouseup = function(event) {
		};

		canvas.onmouseout = function(event) {
		};

		canvas.onmousemove = function(event) {
		};

		canvas.addEventListener('touchstart', event => {
		});

		canvas.addEventListener('touchmove', event => {
		});

		canvas.addEventListener('touchend', event => {
		})
	}

	/* CROPPING FUNCTIONS */
	function cropImage(entire) {
		// if we don't have an image file, abort at this point
		if(image === undefined) {
			return false;
		}

		// if we aren't cropping, ensure entire is tru
		if(!cropping) {
			entire = true;
		}

		// assume we want to crop the entire image, this will be overriden below
		var x = 0;
		var y = 0;
		var width = image.width;
		var height = image.height;

		if(!entire) {
			// work out the actual dimensions that need cropping
			var factor = currentDimens.factor;
			x = Math.floor(overlay.x / factor);
			y = Math.floor(overlay.y / factor);
			width = Math.floor(overlay.width / factor);
			height = Math.floor(overlay.height / factor);

			// check the values are within range of the image
			if(x < 0){ x = 0; }
			if(x > image.width){ x = image.width; }
			if(y < 0){ y = 0; }
			if(y > image.height){ y = image.height; }

			if(x + width > image.width){ width = image.width - x; }
			if(y + height > image.height){ height = image.height - y; }
		}

		// load the image into the cropping canvas
		var cropCanvas = document.createElement("canvas");
		cropCanvas.setAttribute("width", width);
		cropCanvas.setAttribute("height", height);

		var cropContext = cropCanvas.getContext("2d");
		cropContext.drawImage(image, x, y, width, height, 0, 0, width, height);

		return cropCanvas;
	}


	function dataUrlToBlob(dataURI) {
		// convert base64 to raw binary data held in a string
		var byteString = atob(dataURI.split(',')[1]);

		// separate out the mime component
		var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];

		// write the bytes of the string to an ArrayBuffer
		var ab = new ArrayBuffer(byteString.length);
		var ia = new Uint8Array(ab);
		for (var i = 0; i < byteString.length; i++) {
			ia[i] = byteString.charCodeAt(i);
		}

		// write the ArrayBuffer to a blob, and you're done
		return new Blob([ia], {type: mimeString});
	}

	/* API FUNCTIONS */
	cropper.showImage = function(src) {
		cropping = false;
		image = new Image();
		image.onload = function() {
			currentDimens = getScaledImageDimensions(image.width, image.height) ; // work out the scaling
			draw();
		};
		image.src = src;
	};

	cropper.startCropping = function(cropParams) {
		// only continue if an image is loaded
		if(image === undefined) {
			return false;
		}

		// save the current state
		restoreImage = new Image();
		restoreImage.src = image.src;

		cropping = true;
		initializeOverlay(cropParams);
		draw();

		return true;
	};

	cropper.getCroppedImageSrc = function() {
		if(image) {
			// return the cropped image
			var cropCanvas = cropImage(!cropping); // cropping here controls if we get the entire image or not, desirable if the user is not cropping
			var url = cropCanvas.toDataURL("png");

			// show the new image, only bother doing this if it isn't already displayed, ie, we are cropping
			if(cropping) {
				cropper.showImage(url);
			}

			cropping = false;
			removeEventListeners();
			return {url: cropCanvas.toDataURL("png"), width: overlay.width, height: overlay.height};
		} else {
			return false;
		}
	};

	cropper.getCroppedImageBlob = function(type) {
		if(image) {
			// return the cropped image
			var cropCanvas = cropImage(!cropping); // cropping here controls if we get the entire image or not, desirable if the user is not cropping
			var url = cropCanvas.toDataURL(type || "png");

			// show the new image, only bother doing this if it isn't already displayed, ie, we are cropping
			if(cropping) {
				cropper.showImage(url);
			}

			cropping = false;

			// convert the url to a blob and return it
			return dataUrlToBlob(url);
		} else {
			return false;
		}
	};

	cropper.start = function(newCanvas, ratio) {
		// get the context from the given canvas
		canvas = newCanvas;
		if(!canvas.getContext) {
			return; // give up
		}
		context = canvas.getContext("2d");

		// Set default overlay position
		overlay = {
			x: 50,
			y: 50,
			width: 100,
			height: 100,
			resizerSide: 10,
			ratioXY: 1
		}

		// set up the overlay ratio
		if(ratio) {
			setRatio(ratio);
		}

		// setup mouse stuff
		addEventListeners();
	};

	cropper.restore = function() {
		if(restoreImage === undefined) {
			return false;
		}

		cropping = false;

		// show the saved image
		cropper.showImage(restoreImage.src);
		return true;
	};


	/* modify the canvas prototype to allow us to get x and y mouse coords from it */
	HTMLCanvasElement.prototype.getMouseCoords = function(event){
		// loop through this element and all its parents to get the total offset
		var totalOffsetX = 0;
		var totalOffsetY = 0;
		var canvasX = 0;
		var canvasY = 0;
		var currentElement = this;

		do {
			totalOffsetX += currentElement.offsetLeft;
			totalOffsetY += currentElement.offsetTop;
		}
		while(currentElement = currentElement.offsetParent)

		canvasX = event.pageX - totalOffsetX;
		canvasY = event.pageY - totalOffsetY;

		return {x:canvasX, y:canvasY}
	}

}(window.cropper = window.cropper || {}));